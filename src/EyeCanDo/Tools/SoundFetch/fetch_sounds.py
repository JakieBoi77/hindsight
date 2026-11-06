"""Downloads the game's sound effects from CC0 packs on OpenGameArt and prepares them.

Every clip is public domain (CC0), so no credit is required; the sources are still listed in
Assets/ThirdParty/THIRD_PARTY_NOTICES.md. Each clip is trimmed of silence, faded out, made
mono, and levelled to the same peak, then written as a WAV into Assets/_Project/Audio under
the name the game uses (so the existing .meta files and the Sound Library keep working).

To change a sound, change its row in SOUNDS and run again:

    python Tools/SoundFetch/fetch_sounds.py

Needs Python 3.8+ and ffmpeg on the PATH.
"""
import math
import re
import shutil
import subprocess
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path

OGA = "https://opengameart.org/sites/default/files/"

# Packs: short name -> (download, OpenGameArt page, author). All CC0.
PACKS = {
    "kenney_ui": ("kenney_interfaceSounds.zip", "https://opengameart.org/content/interface-sounds", "Kenney"),
    "win": ("winjingle.zip", "https://opengameart.org/content/win-jingle", "fupi"),
    "complete": ("complete_0.wav", "https://opengameart.org/content/cute-mission-complete", "fupi"),
    "bell": ("bell.wav", "https://opengameart.org/content/correct-bell", "fupi"),
    "positive": ("positive_0.ogg", "https://opengameart.org/content/positive-sound", "ezduzziteh"),
    "pops": ("3pops.zip", "https://opengameart.org/content/3-pop-sounds", "wubitog"),
    "water": ("water-splash-slime-sfx.zip", "https://opengameart.org/content/40-cc0-water-splash-slime-sfx", "rubberduck"),
    "sfx100": ("100-CC0-SFX_0.zip", "https://opengameart.org/content/100-cc0-sfx", "rubberduck"),
    "coins": ("coins_-_starninjas.zip", "https://opengameart.org/content/12-coin-sound-effects", "starninjas"),
    "bubble1": ("bubbles-single1.wav", "https://opengameart.org/content/bubble-sound-effects", "bmaczero"),
    "bubble2": ("bubbles-single2.wav", "https://opengameart.org/content/bubble-sound-effects", "bmaczero"),
    "money": ("snd_purchase_0.wav", "https://opengameart.org/content/purchasing-sound-effect", "fupi"),
}

# Game sound -> list of (pack, file inside the pack or None for a single file), loudness 0-1,
# and optionally an extra ffmpeg filter. A sound with several clips picks one at random each
# time (footsteps, pops).
SOUNDS = {
    # Buttons and dialogue: a tiny, soft click with the bright edge taken off.
    "tap": ([("kenney_ui", "click_005.ogg")], 0.6, "lowpass=f=2200"),
    "pop": ([("pops", "pop1.ogg"), ("pops", "pop2.ogg"), ("pops", "pop3.ogg")], 0.8),
    "whoosh": ([("kenney_ui", "maximize_006.ogg")], 0.9),
    "hop": ([("kenney_ui", "maximize_008.ogg")], 0.8),
    "step": ([("kenney_ui", "drop_002.ogg"), ("kenney_ui", "drop_003.ogg"), ("kenney_ui", "drop_004.ogg")], 0.7),
    "drop_fall": ([("bubble1", None)], 0.8),
    "catch": ([("water", "splash_09.ogg"), ("water", "splash_10.ogg")], 0.9),
    "golden": ([("kenney_ui", "maximize_005.ogg")], 0.8),
    "plip": ([("sfx100", "plop_02.ogg")], 0.8),
    "shiver": ([("water", "bubble_01.ogg")], 0.7),
    "blink": ([("kenney_ui", "select_005.ogg")], 0.7),
    "wipe": ([("sfx100", "paper_02.ogg")], 0.7),
    "correct": ([("kenney_ui", "confirmation_001.ogg")], 1.0),
    "try_again": ([("kenney_ui", "question_003.ogg")], 0.8),
    "wiggle": ([("kenney_ui", "back_002.ogg")], 0.8),
    "star": ([("bell", None)], 0.9),
    "sparkle": ([("kenney_ui", "maximize_009.ogg")], 0.8),
    "tick": ([("kenney_ui", "tick_002.ogg")], 0.8),
    "count": ([("kenney_ui", "switch_005.ogg")], 0.8),
    "go": ([("kenney_ui", "confirmation_002.ogg")], 1.0),
    "round_end": ([("complete", None)], 1.0),
    "reward": ([("win", "WinVibraphone.wav")], 1.0),
    "cheer": ([("positive", None)], 0.9),
    # Coins landing in the purse: jingling coins, played once.
    "coin": ([("money", None)], 0.8),
    "purchase": ([("win", "WinBanjo.wav")], 1.0),
    "locked": ([("kenney_ui", "error_008.ogg")], 0.8),
}

PEAK_DB = -1.0
FADE_SECONDS = 0.04


def download(name, cache):
    target = cache / name
    if not target.exists():
        with urllib.request.urlopen(OGA + name, timeout=60) as response:
            target.write_bytes(response.read())
    return target


def extract(archive, member, out):
    """Reads one file out of a zip by its base name (never extracts the whole archive)."""
    with zipfile.ZipFile(archive) as z:
        matches = [n for n in z.namelist() if n.rsplit("/", 1)[-1] == member and "__MACOSX" not in n]
        if not matches:
            raise SystemExit(f"{member} is not in {archive.name}")
        out.write_bytes(z.read(matches[0]))
    return out


def peak_db(path):
    result = subprocess.run(["ffmpeg", "-hide_banner", "-i", str(path), "-af", "volumedetect", "-f", "null", "-"],
                            capture_output=True, text=True, check=True)
    match = re.search(r"max_volume: (-?[\d.]+) dB", result.stderr)
    return float(match.group(1)) if match else 0.0


def prepare(source, out, loudness, work, extra=None):
    trimmed = work / (out.stem + "_trim.wav")
    trim = ("silenceremove=start_periods=1:start_threshold=-55dB,"
            "areverse,silenceremove=start_periods=1:start_threshold=-55dB,areverse")
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", str(source), "-ac", "1", "-ar", "44100", "-af", trim, str(trimmed)],
                   check=True)
    gain = PEAK_DB - peak_db(trimmed) + 20 * math.log10(max(loudness, 0.05))
    seconds = float(subprocess.run(["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(trimmed)],
                                   capture_output=True, text=True, check=True).stdout.strip() or 0)
    fade = f"afade=t=out:st={max(0.0, seconds - FADE_SECONDS):.3f}:d={FADE_SECONDS}"
    if extra:
        fade = f"{extra},{fade}"
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", str(trimmed), "-af", f"volume={gain:.2f}dB,{fade}",
                    "-c:a", "pcm_s16le", str(out)], check=True)


def main():
    if shutil.which("ffmpeg") is None:
        raise SystemExit("ffmpeg is needed on the PATH.")
    root = Path(__file__).resolve().parents[2]
    audio = root / "Assets" / "_Project" / "Audio"
    audio.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory() as temp:
        cache = Path(temp)
        for sound, (clips, loudness, *extra) in SOUNDS.items():
            for index, (pack, member) in enumerate(clips):
                archive = download(PACKS[pack][0], cache)
                source = extract(archive, member, cache / f"{pack}_{member}") if member else archive
                suffix = "" if len(clips) == 1 else f"_{index + 1}"
                prepare(source, audio / f"{sound}{suffix}.wav", loudness, cache, extra[0] if extra else None)
            print(f"{sound}: {len(clips)} clip(s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
