from pathlib import Path
from PIL import Image


root = Path(__file__).resolve().parents[1]
art = root / "Assets" / "Resources" / "ReverseSurvivorArt" / "characters"
prefixes = [
    "monster_noise_blob",
    "monster_venom_singer",
    "monster_cassette_thrower",
    "monster_speaker_brute",
    "monster_metronome_wizard",
    "monster_tuning_fork_breaker",
    "monster_cable_assassin",
    "monster_distortion_king",
]
actions = {
    "idle": 8,
    "move": 8,
    "attack": 6,
    "hit": 4,
    "death": 8,
    "spawn": 6,
}

missing = []
blank = []
coverage = []

for prefix in prefixes:
    for action, count in actions.items():
        for index in range(count):
            path = art / f"{prefix}_{action}_{index:02d}.png"
            if not path.exists():
                missing.append(path.name)
                continue
            image = Image.open(path).convert("RGBA")
            alpha = image.getchannel("A")
            visible = sum(1 for value in alpha.getdata() if value > 0)
            ratio = visible / float(image.width * image.height)
            coverage.append(ratio)
            if visible < 80:
                blank.append(path.name)

print(f"frames={len(coverage)}")
print(f"missing={missing}")
print(f"blank={blank}")
print(f"coverage_min={min(coverage):.4f}")
print(f"coverage_max={max(coverage):.4f}")
