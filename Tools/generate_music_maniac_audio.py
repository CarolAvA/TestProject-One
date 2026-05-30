from __future__ import annotations

import json
import math
import re
import wave
from dataclasses import dataclass
from pathlib import Path

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / "Docs" / "defeat_music_maniac_audio_sfx_bgm_design.md"
OUT = ROOT / "Assets" / "Resources" / "Audio" / "MusicManiac"
SAMPLE_RATE = 44100


@dataclass(frozen=True)
class AudioSpec:
    design_name: str
    output_name: str
    category: str
    duration: float
    loop: bool
    stereo: bool
    bpm: int
    family: str


def extract_design_files() -> list[str]:
    text = DOC.read_text(encoding="utf-8")
    files = sorted(set(re.findall(r"\b(?:sfx|bgm|amb)_[a-z0-9_]+\.(?:wav|ogg)", text)))
    return files


def categorize(name: str) -> tuple[str, str]:
    stem = Path(name).stem
    if stem.startswith("bgm_"):
        return "BGM", "bgm"
    if stem.startswith("amb_"):
        return "Ambience", "ambience"
    if stem.startswith("sfx_ui_"):
        return "SFX/UI", "ui"
    if stem.startswith("sfx_voice_"):
        return "SFX/Voice", "voice"
    if stem.startswith("sfx_creator_"):
        return "SFX/Creator", "creator"
    if stem.startswith("sfx_ai_") or stem.startswith("sfx_rhythm_"):
        return "SFX/AI", "ai"
    if stem.startswith("sfx_mon_"):
        return "SFX/Monster", "monster"
    if stem.startswith("sfx_elite_") or stem.startswith("sfx_boss_"):
        return "SFX/Boss", "boss"
    if stem.startswith("sfx_hit_") or stem.startswith("sfx_dmgnum_"):
        return "SFX/Hit", "hit"
    if stem.startswith("sfx_buff_"):
        return "SFX/Buff", "buff"
    if stem.startswith("sfx_env_"):
        return "SFX/Environment", "environment"
    if stem.startswith("sfx_result_"):
        return "SFX/Result", "result"
    return "SFX/Misc", "misc"


def duration_for(name: str, family: str) -> tuple[float, bool, bool]:
    stem = Path(name).stem
    loop = "loop" in stem or family in {"bgm", "ambience"}
    if family == "bgm":
        if "result" in stem:
            return 10.0, False, True
        if "pause" in stem:
            return 18.0, True, True
        return 24.0, True, True
    if family == "ambience":
        return 12.0, True, True
    if loop:
        return 4.0, True, True
    if any(k in stem for k in ["intro", "phase2", "drop"]):
        return 2.8, False, True
    if any(k in stem for k in ["warning", "charge", "spawn", "death", "victory", "defeat"]):
        return 1.25, False, True
    if any(k in stem for k in ["cast", "release", "explode", "slam", "aoe", "sonic"]):
        return 0.95, False, True
    if family in {"ui", "hit", "buff"}:
        return 0.22, False, False
    if family == "voice":
        return 0.55, False, False
    return 0.45, False, False


def bpm_for(name: str) -> int:
    stem = Path(name).stem
    if "menu" in stem:
        return 96
    if "prepare" in stem:
        return 92
    if "low" in stem:
        return 110
    if "mid" in stem:
        return 128
    if "high" in stem:
        return 152
    if "boss" in stem:
        return 142
    if "pause" in stem:
        return 75
    return 120


def spec_for(design_name: str) -> AudioSpec:
    category, family = categorize(design_name)
    duration, loop, stereo = duration_for(design_name, family)
    output_name = f"{Path(design_name).stem}.wav"
    return AudioSpec(
        design_name=design_name,
        output_name=output_name,
        category=category,
        duration=duration,
        loop=loop,
        stereo=stereo,
        bpm=bpm_for(design_name),
        family=family,
    )


def sine(freq: float, t: np.ndarray, phase: float = 0.0) -> np.ndarray:
    return np.sin((math.tau * freq * t) + phase)


def square(freq: float, t: np.ndarray, phase: float = 0.0) -> np.ndarray:
    return np.sign(sine(freq, t, phase))


def tri(freq: float, t: np.ndarray) -> np.ndarray:
    return 2.0 * np.abs(2.0 * ((t * freq) % 1.0) - 1.0) - 1.0


def noise(n: int, seed: int) -> np.ndarray:
    rng = np.random.default_rng(seed)
    return rng.uniform(-1.0, 1.0, n)


def envelope(n: int, attack: float = 0.015, release: float = 0.18) -> np.ndarray:
    env = np.ones(n, dtype=np.float32)
    if n <= 2:
        return env

    a = min(max(1, int(SAMPLE_RATE * attack)), max(1, n // 2))
    r = min(max(1, int(SAMPLE_RATE * release)), max(1, n - a))
    env[:a] *= np.linspace(0.0, 1.0, a, dtype=np.float32)
    env[-r:] *= np.linspace(1.0, 0.0, r, dtype=np.float32)
    return env


def tone(freq: float, dur: float, wave: str = "square", amp: float = 0.4, seed: int = 0) -> np.ndarray:
    n = max(1, int(SAMPLE_RATE * dur))
    t = np.arange(n, dtype=np.float32) / SAMPLE_RATE
    if wave == "sine":
        y = sine(freq, t)
    elif wave == "tri":
        y = tri(freq, t)
    elif wave == "noise":
        y = noise(n, seed)
    else:
        y = square(freq, t)
    return (y * envelope(n) * amp).astype(np.float32)


def mix_at(dst: np.ndarray, src: np.ndarray, start: int) -> None:
    if start >= len(dst):
        return
    end = min(len(dst), start + len(src))
    dst[start:end] += src[: end - start]


def low_pass(x: np.ndarray, alpha: float = 0.08) -> np.ndarray:
    y = np.zeros_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc += (float(v) - acc) * alpha
        y[i] = acc
    return y


def make_hit(spec: AudioSpec, seed: int) -> np.ndarray:
    n = int(spec.duration * SAMPLE_RATE)
    t = np.arange(n, dtype=np.float32) / SAMPLE_RATE
    stem = Path(spec.design_name).stem
    base = 260.0
    if "fire" in stem:
        base = 130.0
    elif "ice" in stem or "frozen" in stem:
        base = 760.0
    elif "lightning" in stem or "thunder" in stem:
        base = 1180.0
    elif "poison" in stem:
        base = 190.0
    elif "sonic" in stem:
        base = 90.0
    elif "shield" in stem or "glass" in stem:
        base = 610.0
    elif "crit" in stem or "heavy" in stem:
        base = 95.0
    y = sine(base, t) * 0.35 + square(base * 1.5, t) * 0.18 + noise(n, seed) * 0.22
    if "lightning" in stem or "glitch" in stem:
        y *= (np.mod(t * 55.0, 1.0) > 0.35).astype(np.float32)
    if "ice" in stem or "glass" in stem or "shieldbreak" in stem:
        y += sine(base * 2.3, t) * np.exp(-t * 9.0) * 0.35
    if "poison" in stem:
        y = low_pass(y, 0.045)
    return (y * envelope(n, 0.004, 0.16)).astype(np.float32)


def make_ui(spec: AudioSpec, seed: int) -> np.ndarray:
    stem = Path(spec.design_name).stem
    freq = 660.0
    if "error" in stem or "invalid" in stem:
        freq = 150.0
    elif "confirm" in stem or "save" in stem or "ready" in stem:
        freq = 880.0
    elif "cancel" in stem or "close" in stem or "back" in stem:
        freq = 420.0
    elif "hover" in stem or "tooltip" in stem:
        freq = 980.0
    n = int(spec.duration * SAMPLE_RATE)
    y = np.zeros(n, dtype=np.float32)
    mix_at(y, tone(freq, spec.duration * 0.45, "square", 0.35, seed), 0)
    if "confirm" in stem or "open" in stem or "upgrade" in stem:
        mix_at(y, tone(freq * 1.5, spec.duration * 0.42, "tri", 0.26, seed), int(n * 0.35))
    if "cancel" in stem or "back" in stem or "close" in stem:
        mix_at(y, tone(freq * 0.66, spec.duration * 0.42, "tri", 0.23, seed), int(n * 0.35))
    return y


def make_voice(spec: AudioSpec, seed: int) -> np.ndarray:
    n = int(spec.duration * SAMPLE_RATE)
    t = np.arange(n, dtype=np.float32) / SAMPLE_RATE
    stem = Path(spec.design_name).stem
    base = 220.0
    if "angry" in stem:
        base = 170.0
    elif "panic" in stem:
        base = 360.0
    elif "taunt" in stem or "levelup" in stem:
        base = 290.0
    elif "death" in stem or "lowhp" in stem:
        base = 135.0
    wobble = 1.0 + 0.18 * sine(7.0 + (seed % 5), t)
    carrier = square(base * wobble, t) * 0.35 + sine(base * 2.0 * wobble, t) * 0.16
    syllables = (np.mod(t * (7 + seed % 4), 1.0) > 0.25).astype(np.float32)
    y = carrier * syllables + noise(n, seed) * 0.07
    return low_pass(y * envelope(n, 0.015, 0.18), 0.18)


def make_sfx(spec: AudioSpec, seed: int) -> np.ndarray:
    stem = Path(spec.design_name).stem
    if spec.family == "ui":
        return make_ui(spec, seed)
    if spec.family == "voice" or "mumble" in stem or "taunt" in stem:
        return make_voice(spec, seed)
    if spec.family in {"hit", "buff"} or any(k in stem for k in ["hit", "break", "crit", "kill", "block", "dodge", "immune"]):
        return make_hit(spec, seed)

    n = int(spec.duration * SAMPLE_RATE)
    y = np.zeros(n, dtype=np.float32)
    base = 330.0
    wave = "square"
    if any(k in stem for k in ["low", "boom", "slam", "boss", "speaker", "subwoofer"]):
        base = 90.0
        wave = "sine"
    elif any(k in stem for k in ["high", "lightning", "tick", "ice"]):
        base = 920.0
        wave = "tri"
    elif any(k in stem for k in ["poison", "bubble"]):
        base = 180.0
        wave = "sine"
    elif any(k in stem for k in ["fire", "blast", "explode"]):
        base = 120.0
        wave = "noise"
    elif any(k in stem for k in ["metronome", "metro"]):
        base = 620.0
        wave = "square"
    elif any(k in stem for k in ["cassette", "scratch", "record"]):
        base = 470.0
        wave = "tri"

    if "warning" in stem or "charge" in stem:
        segments = 5
        for i in range(segments):
            f = base * (1.0 + i * 0.18)
            mix_at(y, tone(f, spec.duration / segments * 0.85, wave, 0.22, seed + i), int(i * n / segments))
        y += noise(n, seed) * np.linspace(0.02, 0.15, n, dtype=np.float32)
    elif "loop" in stem:
        beat = max(0.18, 60.0 / max(60, spec.bpm))
        steps = int(spec.duration / beat) + 1
        for i in range(steps):
            amp = 0.22 if i % 4 else 0.38
            mix_at(y, tone(base * (1.0 + (i % 3) * 0.12), min(0.16, beat * 0.45), wave, amp, seed + i), int(i * beat * SAMPLE_RATE))
        y += low_pass(noise(n, seed + 91), 0.01) * 0.12
    else:
        mix_at(y, tone(base, min(spec.duration, 0.42), wave, 0.42, seed), 0)
        if any(k in stem for k in ["spawn", "cast", "attack", "fire", "release"]):
            mix_at(y, tone(base * 1.5, min(0.22, spec.duration * 0.45), "square", 0.25, seed + 1), int(n * 0.18))
        if any(k in stem for k in ["death", "defeat", "end"]):
            mix_at(y, tone(base * 0.5, min(0.38, spec.duration * 0.7), "tri", 0.25, seed + 2), int(n * 0.35))
        y += noise(n, seed + 7) * 0.08

    return (y * envelope(n, 0.008, min(0.35, spec.duration * 0.35))).astype(np.float32)


def make_bgm(spec: AudioSpec, seed: int) -> np.ndarray:
    n = int(spec.duration * SAMPLE_RATE)
    y = np.zeros(n, dtype=np.float32)
    beat = 60.0 / spec.bpm
    stem = Path(spec.design_name).stem
    bass_root = 55.0
    if "boss" in stem:
        bass_root = 49.0
    elif "high" in stem:
        bass_root = 65.0
    elif "menu" in stem:
        bass_root = 82.0

    scale = [1.0, 1.125, 1.2, 1.333, 1.5, 1.6, 1.8]
    steps = int(spec.duration / (beat / 2.0)) + 2
    for i in range(steps):
        start = int(i * beat * 0.5 * SAMPLE_RATE)
        bar = i // 8
        degree = scale[(i + bar * 2 + seed) % len(scale)]
        if i % 2 == 0:
            mix_at(y, tone(bass_root * degree, beat * 0.46, "square", 0.18, seed + i), start)
        if i % 4 in {1, 3}:
            mix_at(y, tone(220.0 * degree, beat * 0.28, "tri", 0.12, seed + i), start)
        if i % 8 == 0:
            mix_at(y, tone(70.0, 0.18, "sine", 0.36, seed + i), start)
        if i % 4 == 2:
            mix_at(y, tone(180.0, 0.08, "noise", 0.18, seed + i), start)
        if i % 2 == 1:
            mix_at(y, tone(2200.0, 0.035, "noise", 0.10, seed + i), start)

    if any(k in stem for k in ["high", "riot", "collapse", "boss"]):
        y += low_pass(noise(n, seed + 101), 0.02) * 0.11
    if "broken_radio" in stem or "lofi" in stem:
        y = low_pass(y, 0.09) + noise(n, seed + 102) * 0.035
    if "bad_metronome" in stem:
        for i in range(0, steps, 7):
            mix_at(y, tone(740.0, 0.055, "square", 0.22, seed + i), int((i * beat * 0.5 + 0.09) * SAMPLE_RATE))

    return y.astype(np.float32)


def make_ambience(spec: AudioSpec, seed: int) -> np.ndarray:
    n = int(spec.duration * SAMPLE_RATE)
    t = np.arange(n, dtype=np.float32) / SAMPLE_RATE
    stem = Path(spec.design_name).stem
    y = low_pass(noise(n, seed), 0.006) * 0.25
    hum = 55.0
    if "rooftop" in stem:
        hum = 110.0
    elif "sonic" in stem:
        hum = 70.0
    elif "livehouse" in stem or "mainstage" in stem:
        hum = 48.0
    y += sine(hum, t) * 0.08 + sine(hum * 2.02, t) * 0.035
    for i in range(0, int(spec.duration), 3):
        mix_at(y, tone(900.0 + (seed % 5) * 70, 0.08, "tri", 0.08, seed + i), int((i + 0.7) * SAMPLE_RATE))
    return y.astype(np.float32)


def normalize(y: np.ndarray, target_peak: float) -> np.ndarray:
    peak = float(np.max(np.abs(y))) if y.size else 0.0
    if peak <= 0.0001:
        return y
    return (y / peak * target_peak).astype(np.float32)


def stereoize(y: np.ndarray, spec: AudioSpec, seed: int) -> np.ndarray:
    if not spec.stereo:
        return y[:, None]
    delay = 101 + seed % 157
    right = np.roll(y, delay) * 0.92
    right[:delay] = 0.0
    n = len(y)
    t = np.arange(n, dtype=np.float32) / SAMPLE_RATE
    right += sine(0.22 + (seed % 5) * 0.03, t) * y * 0.04
    return np.stack([y, right], axis=1)


def write_wav(path: Path, audio: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    audio = np.clip(audio, -1.0, 1.0)
    pcm = (audio * 32767.0).astype("<i2")
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1 if pcm.ndim == 1 else pcm.shape[1])
        wav.setsampwidth(2)
        wav.setframerate(SAMPLE_RATE)
        wav.writeframes(pcm.tobytes())


def output_path(spec: AudioSpec) -> Path:
    return OUT / spec.category / spec.output_name


def make_audio(spec: AudioSpec, seed: int) -> np.ndarray:
    if spec.family == "bgm":
        y = make_bgm(spec, seed)
        target = 0.42
    elif spec.family == "ambience":
        y = make_ambience(spec, seed)
        target = 0.28
    else:
        y = make_sfx(spec, seed)
        target = 0.72 if any(k in spec.design_name for k in ["boss", "heavy", "crit", "explode", "slam"]) else 0.56
    y = normalize(y, target)
    return stereoize(y, spec, seed)


def main() -> None:
    files = extract_design_files()
    specs = [spec_for(name) for name in files]
    manifest = []
    for index, spec in enumerate(specs):
        audio = make_audio(spec, index * 31 + len(spec.design_name))
        path = output_path(spec)
        write_wav(path, audio)
        manifest.append(
            {
                "designName": spec.design_name,
                "actualFile": str(path.relative_to(ROOT)).replace("\\", "/"),
                "resourcesPath": "Audio/MusicManiac/" + str(path.relative_to(OUT).with_suffix("")).replace("\\", "/"),
                "category": spec.category,
                "durationSeconds": spec.duration,
                "loop": spec.loop,
                "stereo": spec.stereo,
                "bpm": spec.bpm,
                "note": "Original .ogg design entries are generated as .wav drafts because ffmpeg/OGG encoding is not available in this workspace.",
            }
        )

    manifest_path = OUT / "music_maniac_audio_manifest.json"
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"generated {len(specs)} audio files")
    print(manifest_path)


if __name__ == "__main__":
    main()
