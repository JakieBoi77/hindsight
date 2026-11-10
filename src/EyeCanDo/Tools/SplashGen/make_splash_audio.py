"""Synthesises the studio-splash sting (original work, no samples).

    python Tools/SplashGen/make_splash_audio.py          (from src/EyeCanDo)

Writes Assets/_Project/Audio/Splash/hindsight_splash.wav (44.1 kHz, 16-bit stereo).
Standard library only. The clip starts with the animation, so the cue times below must match
the timeline in Scripts/UI/StudioSplash.cs.

Two sounds, nothing else: a light switch clicking on and the beam sweeping past (a whoosh).
"""

import math
import os
import random
import struct
import wave

SR = 44100
TAU = 2.0 * math.pi
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio", "Splash", "hindsight_splash.wav")

# Cues (seconds), as in StudioSplash.cs.
SWITCH = 0.75     # the click: the lens lights and the beam starts
REVEALED = 1.50   # the whole name is in the light
LENGTH = 2.2


def tone(freq, seconds, decay, attack=0.004, partials=((1.0, 1.0),), drop=None):
    """Sine partials [(ratio, level)] with an exponential decay. drop: (start_ratio, seconds) pitch fall."""
    n = int(seconds * SR)
    out = [0.0] * n
    for ratio, level in partials:
        phase = 0.0
        tc = decay / (ratio ** 0.7)  # higher partials die away sooner
        for k in range(n):
            t = k / SR
            f = freq * ratio
            if drop:
                g = min(1.0, t / drop[1])
                f *= drop[0] + (1.0 - drop[0]) * g
            phase += TAU * f / SR
            out[k] += math.sin(phase) * level * min(1.0, t / attack) * math.exp(-t / tc)
    return out


def sweep(seconds, seed, low, high, q=1.3):
    """White noise through a band-pass that slides from low to high."""
    rng = random.Random(seed)
    n = int(seconds * SR)
    lp = bp = 0.0
    out = [0.0] * n
    for k in range(n):
        x = k / n
        c = 2.0 * math.sin(math.pi * low * (high / low) ** x / SR)
        lp += c * bp
        bp += c * (rng.uniform(-1.0, 1.0) - lp - bp / q)
        out[k] = bp * math.sin(math.pi * x) ** 2  # swell and die away
    return out


def reverb(channel, seed):
    """A small Schroeder reverb: four damped combs into two all-passes."""
    rng = random.Random(seed)
    n = len(channel)
    out = [0.0] * n
    for base in (1557, 1617, 1491, 1422):
        length = base + rng.randrange(0, 23)
        buf, idx, store = [0.0] * length, 0, 0.0
        for k in range(n):
            y = buf[idx]
            store = y * 0.65 + store * 0.35
            buf[idx] = channel[k] + store * 0.74
            idx = idx + 1 if idx + 1 < length else 0
            out[k] += y
    for base in (225, 556):
        length = base + rng.randrange(0, 11)
        buf, idx = [0.0] * length, 0
        for k in range(n):
            delayed, x = buf[idx], out[k]
            buf[idx] = x + delayed * 0.5
            idx = idx + 1 if idx + 1 < length else 0
            out[k] = delayed - x * 0.5
    return [v * 0.25 for v in out]


def main():
    n = int(LENGTH * SR)
    dry = [[0.0] * n, [0.0] * n]
    wet = [[0.0] * n, [0.0] * n]

    def add(start, mono, gain, pan=0.0, send=0.3):
        """pan: -1 (left) to 1 (right), or a function of 0..1 over the sound."""
        i0 = int(start * SR)
        for k, v in enumerate(mono):
            i = i0 + k
            if i >= n:
                break
            angle = ((pan(k / len(mono)) if callable(pan) else pan) + 1.0) * math.pi / 4.0
            left, right = v * gain * math.cos(angle), v * gain * math.sin(angle)
            dry[0][i] += left
            dry[1][i] += right
            wet[0][i] += left * send
            wet[1][i] += right * send

    # 1. A light switch: the rocker's soft press, then the sharp snap of the contact a moment later.
    def snap(seed, bright, body):
        rng = random.Random(seed)
        tick = [rng.uniform(-1.0, 1.0) * math.exp(-k / (0.0016 * SR)) for k in range(int(0.02 * SR))]
        ring = tone(bright, 0.05, decay=0.007, attack=0.0003)
        knock = tone(body, 0.08, decay=0.018, attack=0.0008)
        return [(tick[k] * 0.7 if k < len(tick) else 0.0) + (ring[k] * 0.55 if k < len(ring) else 0.0) + knock[k] * 0.8 for k in range(len(knock))]

    add(SWITCH - 0.045, snap(1, 1900.0, 240.0), 0.2, pan=-0.3, send=0.12)
    add(SWITCH, snap(2, 2700.0, 170.0), 0.5, pan=-0.3, send=0.15)

    # 2. The beam: one breath of air, moving left to right with it.
    add(SWITCH + 0.03, sweep(REVEALED - SWITCH, 4, 350.0, 3200.0), 0.3, pan=lambda x: -0.7 + 1.4 * x, send=0.4)

    channels = [[d + w for d, w in zip(dry[c], reverb(wet[c], 100 + c))] for c in (0, 1)]
    scale = 0.8 / max(max(abs(v) for v in ch) for ch in channels)
    fade = int(0.4 * SR)
    frames = bytearray()
    for k in range(n):
        g = scale * min(1.0, (n - 1 - k) / fade)
        for ch in channels:
            frames += struct.pack("<h", int(max(-1.0, min(1.0, ch[k] * g)) * 32767))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with wave.open(OUT, "wb") as f:
        f.setnchannels(2)
        f.setsampwidth(2)
        f.setframerate(SR)
        f.writeframes(bytes(frames))
    print(f"{os.path.basename(OUT)}  {LENGTH:.2f}s")


if __name__ == "__main__":
    main()
