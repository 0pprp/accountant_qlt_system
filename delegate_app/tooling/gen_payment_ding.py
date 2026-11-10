import math
import struct
import wave
from pathlib import Path

out = Path(__file__).resolve().parents[1] / 'assets' / 'sounds' / 'payment_ding.wav'
out.parent.mkdir(parents=True, exist_ok=True)

sr = 44100
# Apple Pay–style: short bright two-note rising chime (G5 -> C6) with soft decay.
notes = [
    (784.0, 0, 280, 0.42),  # G5
    (1046.5, 90, 420, 0.55),  # C6 slightly overlapping
]

total_ms = 700
n = int(sr * total_ms / 1000)
samples = [0.0] * n


def env(t: float, dur: float, attack: float = 0.008, release: float = 0.22) -> float:
    if t < 0 or t > dur:
        return 0.0
    a = min(attack, dur * 0.2)
    r = min(release, dur * 0.7)
    if t < a:
        return t / a
    if t > dur - r:
        return max(0.0, (dur - t) / r)
    return 1.0


for freq, start_ms, dur_ms, amp in notes:
    start = int(sr * start_ms / 1000)
    length = int(sr * dur_ms / 1000)
    for i in range(length):
        idx = start + i
        if idx >= n:
            break
        t = i / sr
        s = math.sin(2 * math.pi * freq * t)
        s += 0.22 * math.sin(2 * math.pi * freq * 2 * t)
        s += 0.08 * math.sin(2 * math.pi * freq * 3 * t)
        samples[idx] += amp * env(t, dur_ms / 1000.0) * s

peak = max(abs(x) for x in samples) or 1.0
gain = 0.92 / peak
pcm = [max(-32767, min(32767, int(x * gain * 32767))) for x in samples]

with wave.open(str(out), 'w') as w:
    w.setnchannels(1)
    w.setsampwidth(2)
    w.setframerate(sr)
    w.writeframes(b''.join(struct.pack('<h', v) for v in pcm))

print(f'wrote {out} bytes={out.stat().st_size} duration_ms={total_ms}')
