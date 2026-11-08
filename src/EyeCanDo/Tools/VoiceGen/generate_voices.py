"""Generate the narration WAVs: a warm, kids-TV doctor (Dr. Iris) and a young boy (Sam).

Reads lines.json (written by POC -> Rebuild All) and writes Assets/_Project/Audio/Narration/<id>.wav.
Run POC -> Rebuild All again afterwards to link them.

How it works:
  * Chatterbox (Resemble AI, MIT) clones each character's voice from a short reference clip in
    voices/ and has an emotion "exaggeration" control. Each line has a MOOD (calm, warm, bright)
    that sets that control, so explanations stay calm and only the happy moments get big.
  * Sam is a boy. Chatterbox drifts toward an adult woman's voice, so every Sam take is nudged
    back with the WORLD vocoder (a little higher pitch and formants: a smaller voice), and an
    age/gender classifier picks the version that sounds most like a child.
    voices/sam_ref.wav is Kokoro's am_fenrir voice made childlike the same way (pitch x2.0, formants x1.28).
  * Each sentence is voiced on its own and joined to the next with a short breath of a pause, and
    a rushed sentence is slowed down (WORLD again, same pitch), so the pace suits young children.
  * Each sentence is generated several times and a "director" keeps the best take:
      - Whisper must hear every word and nothing extra (Chatterbox sometimes adds a "Whoo!");
      - no shouted word (the loudest moment may not stand far above the rest of the line);
      - pitch movement close to the mood's target (not monotone, not over the top);
      - not rushed; and for Sam, how much like a child it sounds.

Setup (once, needs an NVIDIA GPU; CPU works but is slow):
    python -m venv venv
    venv/Scripts/python -m pip install chatterbox-tts faster-whisper pyworld
    venv/Scripts/python -m pip install --force-reinstall --no-deps torch==2.6.0 torchaudio==2.6.0 --index-url https://download.pytorch.org/whl/cu124

Usage:
    venv/Scripts/python generate_voices.py [--force] [--only p02_hi,p06_ooh] [--takes 5]

These are generated placeholders. Real recordings saved under the same file names replace them.
"""
import argparse
import json
import os
import re
from collections import Counter

import librosa
import numpy as np
import soundfile as sf
import torch

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "_Project", "Audio", "Narration"))

VOICES = {
    "Doctor": {"ref": "voices/doctor_ref.wav", "child": False},
    "Sam": {"ref": "voices/sam_ref.wav", "child": True},
}

# mood: (Chatterbox exaggeration, target pitch spread in semitones)
MOODS = {
    "calm": (0.45, 3.0),    # explaining, reassuring
    "warm": (0.6, 3.8),     # greeting, friendly
    "bright": (0.8, 4.8),   # cheering, celebrating
}
# Lines not listed are "warm".
LINE_MOODS = {
    "p02_sam": "calm", "p03_chart": "calm", "p05_eyes": "calm", "p05_cold": "calm", "p06_blink": "calm",
    "p10_hold": "calm", "p11_ready": "calm", "p13_look": "warm",
    "p02_hi": "bright", "p04_willdo": "bright", "p06_ooh": "bright", "p07_good": "bright", "p08_yay": "bright",
    "p12_did": "bright", "p13_great": "bright",
}
CFG = 0.35                # pacing: lower is slower
MAX_SHOUT_DB = 9.0        # loudest 200 ms may stand this far above the line's typical loudness
MAX_WORDS_PER_SECOND = 2.9  # unhurried, for young children; faster takes are slowed down
PAUSE_BETWEEN_SENTENCES = 0.35

REPLACE = {"Dr. ": "Doctor "}
SAME = {"oh": "ooh", "dr": "doctor", "alright": "all right", "okay": "ok"}


def words(text):
    out = []
    for w in re.sub(r"[^a-z' ]", " ", text.lower()).split():
        w = w.strip("'")
        w = re.sub(r"^o+h+$", "ooh", w)  # "Oooh", "Ooh", "Oh" are the same sound
        w = SAME.get(w, w)
        out.extend(w.split())
    return [w for w in out if w]


def liveliness(y, sr):
    """Pitch spread in semitones: how far the voice moves up and down (monotone ~ 2-3)."""
    f0, _, _ = librosa.pyin(y, fmin=70, fmax=800, sr=sr)
    f0 = f0[~np.isnan(f0)]
    if len(f0) < 5:
        return 0.0
    return float(np.std(12 * np.log2(f0 / np.median(f0))))


def shout(y, sr):
    """How far (dB) the loudest 200 ms stands above the median loudness of the speech."""
    hop = int(0.01 * sr)
    rms = librosa.feature.rms(y=y, frame_length=int(0.2 * sr), hop_length=hop)[0]
    db = 20 * np.log10(rms + 1e-6)
    speech = db[db > db.max() - 30]
    return float(db.max() - np.median(speech)) if len(speech) else 0.0


def finish(y, sr):
    """Trim silence, keep a breath of tail, peak-normalise to -1 dBFS."""
    loud = np.where(np.abs(y) > 0.01)[0]
    if len(loud):
        y = y[max(0, loud[0] - int(0.04 * sr)): loud[-1] + int(0.3 * sr)]
    peak = np.max(np.abs(y)) or 1.0
    return (y / peak * 0.89).astype(np.float32)


def childify(y, sr, f0_factor, formant_factor):
    """WORLD vocoder: raise the pitch and move the formants up (a smaller voice), separately."""
    import pyworld as pw
    y = y.astype(np.float64)
    f0, t = pw.harvest(y, sr, f0_floor=60, f0_ceil=800)
    sp = pw.cheaptrick(y, f0, t, sr)
    ap = pw.d4c(y, f0, t, sr)
    k = np.arange(sp.shape[1])
    src = k / formant_factor
    sp = np.stack([np.interp(src, k, row) for row in sp])
    ap = np.stack([np.interp(src, k, row) for row in ap])
    return pw.synthesize(f0 * f0_factor, sp, ap, sr).astype(np.float32)


class ChildJudge:
    """audeering wav2vec2 age/gender model (6 layers): probability that a voice is a child's."""
    NAME = "audeering/wav2vec2-large-robust-6-ft-age-gender"

    def __init__(self, device):
        import torch.nn as nn
        from transformers import Wav2Vec2Processor
        from transformers.models.wav2vec2.modeling_wav2vec2 import Wav2Vec2Model, Wav2Vec2PreTrainedModel

        class Head(nn.Module):
            def __init__(self, config, n):
                super().__init__()
                self.dense = nn.Linear(config.hidden_size, config.hidden_size)
                self.dropout = nn.Dropout(config.final_dropout)
                self.out_proj = nn.Linear(config.hidden_size, n)

            def forward(self, x):
                return self.out_proj(torch.tanh(self.dense(self.dropout(x))))

        class AgeGender(Wav2Vec2PreTrainedModel):
            def __init__(self, config):
                super().__init__(config)
                self.wav2vec2 = Wav2Vec2Model(config)
                self.age = Head(config, 1)
                self.gender = Head(config, 3)  # female, male, child
                self.post_init()

            def forward(self, x):
                h = self.wav2vec2(x)[0].mean(dim=1)
                return self.age(h), torch.softmax(self.gender(h), dim=1)

        self.device = device
        self.proc = Wav2Vec2Processor.from_pretrained(self.NAME)
        self.model = AgeGender.from_pretrained(self.NAME).eval().to(device)

    def child(self, y, sr):
        x = self.proc(librosa.resample(y, orig_sr=sr, target_sr=16000), sampling_rate=16000).input_values[0]
        with torch.no_grad():
            _, g = self.model(torch.tensor(x[None], dtype=torch.float32, device=self.device))
        return float(g[0, 2])


def stretch(y, sr, factor):
    """WORLD vocoder: slow speech down by `factor` without changing its pitch."""
    import pyworld as pw
    y = y.astype(np.float64)
    f0, t = pw.harvest(y, sr, f0_floor=60, f0_ceil=800)
    sp = pw.cheaptrick(y, f0, t, sr)
    ap = pw.d4c(y, f0, t, sr)
    n = len(f0)
    src = np.linspace(0, n - 1, int(round(n * factor)))
    i0 = np.floor(src).astype(int)
    i1 = np.minimum(i0 + 1, n - 1)
    w = (src - i0)[:, None]
    f0s = np.where((f0[i0] > 0) & (f0[i1] > 0), f0[i0] * (1 - w[:, 0]) + f0[i1] * w[:, 0], 0.0)
    sps = sp[i0] * (1 - w) + sp[i1] * w
    aps = ap[i0] * (1 - w) + ap[i1] * w
    return pw.synthesize(f0s, np.ascontiguousarray(sps), np.ascontiguousarray(aps), sr).astype(np.float32)


def sentences(text):
    """Splits a line into sentences; each is voiced on its own and joined with a short pause.
    A sentence of one or two words ("Hi!", "Now blink!") stays with the next one: alone it is too short to say well."""
    out = []
    carry = ""
    for x in re.split(r"(?<=[.!?])\s+", text.strip()):
        x = (carry + " " + x).strip()
        carry = ""
        if len(words(x)) <= 2:
            carry = x
            continue
        out.append(x)
    if carry:
        if out:
            out[-1] = out[-1] + " " + carry
        else:
            out.append(carry)
    return out


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true", help="overwrite existing WAVs (including real recordings!)")
    parser.add_argument("--only", default="", help="comma-separated ids")
    parser.add_argument("--takes", type=int, default=5, help="takes per sentence; the director keeps the best")
    args = parser.parse_args()

    from chatterbox.tts import ChatterboxTTS
    from faster_whisper import WhisperModel

    device = "cuda" if torch.cuda.is_available() else "cpu"
    tts = ChatterboxTTS.from_pretrained(device=device)
    asr = WhisperModel("small.en", device=device, compute_type="float16" if device == "cuda" else "int8")
    judge = None

    with open(os.path.join(HERE, "lines.json"), encoding="utf-8") as f:
        lines = json.load(f)
    only = {x for x in args.only.split(",") if x}
    os.makedirs(OUT_DIR, exist_ok=True)
    tmp = os.path.join(HERE, "_take.wav")

    def direct(line_id, text, v, exaggeration, target, takes):
        """Generates takes of one sentence and returns the best (score, audio)."""
        want = words(text)
        best = None
        for take in range(takes * 2):
            if take >= takes and best is not None and best[0] > -50:
                break  # enough takes, and at least one is clean
            # Short sentences are the hardest to get clean: after the first round, calmer takes.
            ex = exaggeration if take < takes else max(0.35, exaggeration - 0.25)
            wav = tts.generate(text, audio_prompt_path=os.path.join(HERE, v["ref"]), exaggeration=ex, cfg_weight=CFG)
            y = wav.squeeze(0).cpu().numpy()
            versions = [y]
            if v["child"]:
                versions = [childify(y, tts.sr, f, a) for f, a in [(1.0, 1.12), (1.08, 1.12), (1.15, 1.1)]]
            for y in versions:
                y = finish(y, tts.sr)
                rate = len(want) / max(0.5, len(y) / tts.sr)
                if rate > MAX_WORDS_PER_SECOND and len(want) > 2:
                    # Rushed: slow it to the target pace (at most a quarter slower).
                    y = finish(stretch(y, tts.sr, min(1.25, rate / MAX_WORDS_PER_SECOND)), tts.sr)
                    rate = len(want) / max(0.5, len(y) / tts.sr)
                sf.write(tmp, y, tts.sr)
                heard = " ".join(s.text for s in asr.transcribe(tmp, beam_size=5, language="en")[0]).strip()
                got = words(heard)
                missing = sum((Counter(want) - Counter(got)).values())
                extra = sum((Counter(got) - Counter(want)).values())
                lively = liveliness(y, tts.sr)
                loud = shout(y, tts.sr)
                child = judge.child(y, tts.sr) if v["child"] else 0.0
                score = -abs(lively - target) - max(0.0, loud - MAX_SHOUT_DB) - 2 * max(0.0, rate - MAX_WORDS_PER_SECOND) + 6 * child
                if missing or extra:
                    score -= 100 + 10 * (missing + extra)
                print(f"  {line_id} take {take + 1} x{ex:.2f}: {'ok ' if not (missing or extra) else 'BAD'} "
                      f"score={score:6.2f} lively={lively:4.1f} shout={loud:4.1f}dB rate={rate:3.1f}w/s child={child:.2f} heard='{heard}'",
                      flush=True)
                if best is None or score > best[0]:
                    best = (score, y)
        return best

    for line in lines:
        if only and line["id"] not in only:
            continue
        out = os.path.join(OUT_DIR, line["id"] + ".wav")
        if os.path.exists(out) and not args.force:
            print("skip (exists)", line["id"])
            continue
        v = VOICES.get(line["speaker"], VOICES["Doctor"])
        if v["child"] and judge is None:
            judge = ChildJudge(device)
        mood = LINE_MOODS.get(line["id"], "warm")
        exaggeration, target = MOODS[mood]
        text = line["text"]
        for a, b in REPLACE.items():
            text = text.replace(a, b)

        parts, scores = [], []
        gap = np.zeros(int(PAUSE_BETWEEN_SENTENCES * tts.sr), dtype=np.float32)
        for sentence in sentences(text):
            # Very short sentences ("Will do!") come out wrong more often: give them more tries.
            takes = args.takes * 2 if len(words(sentence)) <= 3 else args.takes
            score, y = direct(line["id"], sentence, v, exaggeration, target, takes)
            if parts:
                parts.append(gap)
            parts.append(y)
            scores.append(score)
        if min(scores) < -50:
            print(f"WARNING {line['id']}: a sentence was never heard correctly; keeping the closest. Listen to it.")
        y = np.concatenate(parts)
        sf.write(out, y, tts.sr, subtype="PCM_16")
        print(f"wrote {line['id']} ({line['speaker']}, {mood}, {len(y) / tts.sr:.1f}s, worst sentence {min(scores):.2f}): {line['text']}", flush=True)
    if os.path.exists(tmp):
        os.remove(tmp)


if __name__ == "__main__":
    main()
