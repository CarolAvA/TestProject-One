from pathlib import Path
from PIL import Image


root = Path(__file__).resolve().parents[1]
art = root / "Assets" / "Resources" / "ReverseSurvivorArt" / "characters"
files = sorted(path for path in art.glob("hero_music_maniac_*_*.png") if "preview" not in path.name)
coverage = []
blank = []

for path in files:
    image = Image.open(path).convert("RGBA")
    alpha = image.getchannel("A")
    visible = sum(1 for value in alpha.getdata() if value > 0)
    ratio = visible / float(image.width * image.height)
    coverage.append(ratio)
    if visible < 80:
        blank.append(path.name)

print(f"frames={len(files)}")
print(f"coverage_min={min(coverage):.4f}")
print(f"coverage_max={max(coverage):.4f}")
print(f"blank={blank}")
