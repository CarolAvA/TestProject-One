from pathlib import Path
from PIL import Image


root = Path(__file__).resolve().parents[1]
art = root / "Assets" / "Resources" / "ReverseSurvivorArt"
prefixes = [
    ("projectiles", "projectile_basic_note"),
    ("projectiles", "projectile_lightning_note"),
    ("projectiles", "projectile_ice_note"),
    ("projectiles", "projectile_fire_note"),
    ("projectiles", "projectile_poison_note"),
    ("projectiles", "projectile_sonic_ring"),
    ("vfx", "vfx_hit_spark"),
    ("vfx", "vfx_sonic_ring"),
    ("vfx", "vfx_spawn_smoke"),
    ("vfx", "vfx_death_noise"),
    ("vfx", "vfx_warning_ring"),
    ("vfx", "vfx_aoe_fire"),
    ("vfx", "vfx_aoe_ice"),
    ("vfx", "vfx_aoe_poison"),
    ("vfx", "vfx_aoe_sonic"),
    ("vfx", "vfx_warning_lightning", 12),
    ("vfx", "vfx_warning_frost_field", 12),
    ("vfx", "vfx_warning_anti_heal", 12),
    ("vfx", "vfx_warning_shield_brand", 12),
    ("vfx", "vfx_warning_bone_wall", 12),
    ("vfx", "vfx_warning_demon_hand", 12),
    ("vfx", "vfx_skill_lightning", 12),
    ("vfx", "vfx_skill_frost_field", 12),
    ("vfx", "vfx_skill_anti_heal", 12),
    ("vfx", "vfx_skill_shield_brand", 12),
    ("vfx", "vfx_skill_bone_wall", 12),
    ("vfx", "vfx_skill_demon_hand", 12),
]

missing = []
blank = []
coverage = []

for item in prefixes:
    folder, prefix = item[0], item[1]
    frame_count = item[2] if len(item) > 2 else 8
    for index in range(frame_count):
        path = art / folder / f"{prefix}_anim_{index:02d}.png"
        if not path.exists():
            missing.append(str(path.relative_to(art)))
            continue
        image = Image.open(path).convert("RGBA")
        visible = sum(1 for value in image.getchannel("A").getdata() if value > 0)
        coverage.append(visible / float(image.width * image.height))
        if visible < 80:
            blank.append(str(path.relative_to(art)))

print(f"frames={len(coverage)}")
print(f"missing={missing}")
print(f"blank={blank}")
print(f"coverage_min={min(coverage):.4f}")
print(f"coverage_max={max(coverage):.4f}")
