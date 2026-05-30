from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import math


ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt"
PROJECTILES = ART / "projectiles"
VFX = ART / "vfx"
TILES = ART / "tiles"
SIZE = 128
SMALL = 96


COLORS = {
    "white": (242, 250, 255, 255),
    "cyan": (64, 235, 245, 255),
    "blue": (64, 128, 255, 255),
    "ice": (136, 224, 255, 255),
    "fire": (255, 88, 38, 255),
    "orange": (255, 171, 57, 255),
    "poison": (90, 238, 92, 255),
    "poison_dark": (33, 128, 58, 255),
    "purple": (178, 88, 255, 255),
    "magenta": (255, 70, 184, 255),
    "red": (255, 57, 77, 255),
    "yellow": (255, 226, 92, 255),
    "bone": (228, 218, 170, 255),
    "dark": (18, 12, 28, 255),
    "shadow": (0, 0, 0, 120),
}


def c(name, alpha=None):
    r, g, b, a = COLORS[name]
    return (r, g, b, a if alpha is None else alpha)


def line(draw, xy, color, width=1):
    draw.line(xy, fill=c(color), width=width)


def ellipse(draw, xy, fill, outline=None, width=1):
    draw.ellipse(xy, fill=c(fill) if isinstance(fill, str) else fill, outline=c(outline) if outline else None, width=width)


def poly(draw, points, fill, outline=None):
    draw.polygon(points, fill=c(fill), outline=c(outline) if outline else None)


def rect(draw, xy, fill, outline=None):
    draw.rectangle(xy, fill=c(fill), outline=c(outline) if outline else None)


def glow_layer(size, color, radius=4):
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    draw.ellipse((size * 0.3, size * 0.3, size * 0.7, size * 0.7), fill=c(color, 100))
    return layer.filter(ImageFilter.GaussianBlur(radius))


def draw_note_projectile(color, frame, frames, size=SMALL):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    img.alpha_composite(glow_layer(size, color, 7))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    pulse = math.sin(frame / frames * math.tau) * 2
    ellipse(draw, (cx - 16, cy + 6 + pulse, cx + 4, cy + 26 + pulse), color, "white", 2)
    line(draw, (cx + 2, cy + 15 + pulse, cx + 2, cy - 24 + pulse), color, 5)
    line(draw, (cx + 2, cy - 24 + pulse, cx + 27, cy - 14 + pulse), color, 4)
    line(draw, (cx - 28, cy + 1 + pulse, cx + 24, cy - 9 + pulse), "white", 2)
    for i in range(3):
        x = cx - 37 + i * 14
        line(draw, (x, cy + 22 + i, x - 12, cy + 27 + i), color, 2)
    return img


def draw_lightning_projectile(frame, frames):
    img = Image.new("RGBA", (SMALL, SMALL), (0, 0, 0, 0))
    img.alpha_composite(glow_layer(SMALL, "blue", 8))
    draw = ImageDraw.Draw(img)
    cx, cy = SMALL // 2, SMALL // 2
    jitter = int(math.sin(frame * 1.7) * 3)
    points = [(cx - 28, cy - 8), (cx - 2, cy - 22 + jitter), (cx - 10, cy - 3), (cx + 26, cy - 8 + jitter), (cx - 3, cy + 24), (cx + 4, cy + 2)]
    poly(draw, points, "yellow", "white")
    line(draw, (cx - 35, cy + 14, cx + 32, cy - 18), "cyan", 2)
    return img


def draw_ring_projectile(frame, frames):
    img = Image.new("RGBA", (SMALL, SMALL), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = SMALL // 2, SMALL // 2
    r = 20 + math.sin(frame / frames * math.tau) * 3
    for i, color in enumerate(["purple", "magenta", "cyan"]):
        rr = r + i * 4
        draw.ellipse((cx - rr, cy - rr, cx + rr, cy + rr), outline=c(color, 210 - i * 45), width=3)
    line(draw, (cx - 36, cy, cx + 36, cy), "white", 2)
    return img


def draw_hit_spark(frame, frames, color="yellow", size=SIZE):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    progress = frame / max(1, frames - 1)
    radius = 10 + progress * 45
    for i in range(14):
        ang = i / 14 * math.tau + progress * 0.45
        inner = radius * 0.25
        outer = radius * (0.75 + 0.25 * math.sin(i))
        line(draw, (cx + math.cos(ang) * inner, cy + math.sin(ang) * inner, cx + math.cos(ang) * outer, cy + math.sin(ang) * outer), color, max(1, int(4 - progress * 3)))
    draw.ellipse((cx - radius * 0.35, cy - radius * 0.35, cx + radius * 0.35, cy + radius * 0.35), outline=c("white", int(220 * (1 - progress))), width=3)
    return img


def draw_ring_vfx(frame, frames, color="cyan", size=SIZE):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    progress = frame / max(1, frames - 1)
    for i in range(3):
        r = 16 + progress * 45 + i * 8
        alpha = int(max(0, 210 * (1 - progress) - i * 35))
        draw.ellipse((cx - r, cy - r, cx + r, cy + r), outline=c(color, alpha), width=4)
    for i in range(8):
        a = i / 8 * math.tau + progress * math.tau
        line(draw, (cx + math.cos(a) * 16, cy + math.sin(a) * 16, cx + math.cos(a) * 46, cy + math.sin(a) * 46), color, 2)
    return img


def draw_smoke(frame, frames, color="purple", size=SIZE):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    progress = frame / max(1, frames - 1)
    cx, cy = size // 2, size // 2
    for i in range(9):
        a = i / 9 * math.tau
        dist = 8 + progress * (16 + i * 2)
        r = 8 + progress * 13 + i % 3 * 3
        alpha = max(24, int(170 * (1 - progress)))
        draw.ellipse((cx + math.cos(a) * dist - r, cy + math.sin(a) * dist - r, cx + math.cos(a) * dist + r, cy + math.sin(a) * dist + r), fill=c(color, alpha))
    return img.filter(ImageFilter.GaussianBlur(0.3))


def draw_aoe(frame, frames, color, motif="ring", size=SIZE):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    progress = frame / frames
    base = 38 + math.sin(progress * math.tau) * 4
    draw.ellipse((cx - base, cy - base, cx + base, cy + base), fill=c(color, 44), outline=c(color, 170), width=3)
    for i in range(12):
        a = i / 12 * math.tau + progress * math.tau * 0.45
        r1 = 12
        r2 = base - 6
        line(draw, (cx + math.cos(a) * r1, cy + math.sin(a) * r1, cx + math.cos(a) * r2, cy + math.sin(a) * r2), color, 1)
    if motif == "ice":
        for i in range(6):
            a = i / 6 * math.tau
            line(draw, (cx, cy, cx + math.cos(a) * 34, cy + math.sin(a) * 34), "white", 2)
    elif motif == "fire":
        for i in range(8):
            a = i / 8 * math.tau + progress
            poly(draw, [(cx + math.cos(a) * 18, cy + math.sin(a) * 18), (cx + math.cos(a + 0.12) * 36, cy + math.sin(a + 0.12) * 36), (cx + math.cos(a - 0.12) * 28, cy + math.sin(a - 0.12) * 28)], "orange")
    elif motif == "poison":
        for i in range(6):
            a = i / 6 * math.tau + progress
            ellipse(draw, (cx + math.cos(a) * 26 - 5, cy + math.sin(a) * 26 - 5, cx + math.cos(a) * 26 + 5, cy + math.sin(a) * 26 + 5), "poison_dark", None)
    return img


def draw_skill(frame, frames, key):
    palette = {
        "lightning": ("yellow", "blue"),
        "frost_field": ("ice", "cyan"),
        "anti_heal": ("purple", "magenta"),
        "shield_brand": ("cyan", "blue"),
        "bone_wall": ("bone", "yellow"),
        "demon_hand": ("red", "purple"),
    }
    primary, secondary = palette[key]
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = SIZE // 2, SIZE // 2
    progress = frame / max(1, frames - 1)
    r = 12 + progress * 47
    draw.ellipse((cx - r, cy - r, cx + r, cy + r), outline=c(primary, int(230 * (1 - progress * 0.45))), width=4)
    if key == "lightning":
        for i in range(5):
            x = cx - 26 + i * 13
            poly(draw, [(x, cy - 45), (x + 9, cy - 5), (x + 1, cy - 8), (x + 16, cy + 38)], primary, "white")
    elif key == "frost_field":
        for i in range(8):
            a = i / 8 * math.tau
            line(draw, (cx, cy, cx + math.cos(a) * (28 + progress * 20), cy + math.sin(a) * (28 + progress * 20)), secondary, 2)
    elif key == "anti_heal":
        line(draw, (cx - 30, cy - 30, cx + 30, cy + 30), primary, 6)
        line(draw, (cx + 30, cy - 30, cx - 30, cy + 30), primary, 6)
        draw.ellipse((cx - 25, cy - 25, cx + 25, cy + 25), outline=c(secondary), width=4)
    elif key == "shield_brand":
        poly(draw, [(cx, cy - 38), (cx + 34, cy - 20), (cx + 24, cy + 28), (cx, cy + 42), (cx - 24, cy + 28), (cx - 34, cy - 20)], secondary, "white")
        line(draw, (cx - 28, cy, cx + 28, cy), primary, 5)
    elif key == "bone_wall":
        for i in range(5):
            rect(draw, (cx - 42 + i * 18, cy - 38, cx - 30 + i * 18, cy + 38), primary, "dark")
    elif key == "demon_hand":
        for i in range(5):
            a = -0.85 + i * 0.42
            line(draw, (cx, cy + 36, cx + math.sin(a) * 42, cy - 30 - abs(i - 2) * 5), primary, 8)
        ellipse(draw, (cx - 22, cy + 10, cx + 22, cy + 48), secondary, "dark")
    return img


def save_sequence(folder, prefix, frames, drawer):
    folder.mkdir(parents=True, exist_ok=True)
    paths = []
    for i in range(frames):
        image = drawer(i, frames)
        path = folder / f"{prefix}_anim_{i:02d}.png"
        image.save(path)
        paths.append(path)
    paths[0].replace(folder / f"{prefix}.png")
    for i, path in enumerate(paths):
        if i == 0:
            image = drawer(0, frames)
            image.save(folder / f"{prefix}_anim_00.png")
            image.save(folder / f"{prefix}.png")
    return paths


def make_preview(entries, out_path, cell=128):
    cols = 8
    rows = len(entries)
    sheet = Image.new("RGBA", (cols * cell, rows * cell), (18, 16, 24, 255))
    for row, (folder, prefix, frames) in enumerate(entries):
        for i in range(min(cols, frames)):
            img = Image.open(folder / f"{prefix}_anim_{i:02d}.png").resize((cell, cell), Image.Resampling.NEAREST)
            sheet.alpha_composite(img, (i * cell, row * cell))
    sheet.save(out_path)


def main():
    entries = []
    projectile_specs = [
        ("projectile_basic_note", lambda i, n: draw_note_projectile("white", i, n)),
        ("projectile_lightning_note", lambda i, n: draw_lightning_projectile(i, n)),
        ("projectile_ice_note", lambda i, n: draw_note_projectile("ice", i, n)),
        ("projectile_fire_note", lambda i, n: draw_note_projectile("fire", i, n)),
        ("projectile_poison_note", lambda i, n: draw_note_projectile("poison", i, n)),
        ("projectile_sonic_ring", lambda i, n: draw_ring_projectile(i, n)),
    ]
    for prefix, drawer in projectile_specs:
        save_sequence(PROJECTILES, prefix, 8, drawer)
        entries.append((PROJECTILES, prefix, 8))

    vfx_specs = [
        ("vfx_hit_spark", 8, lambda i, n: draw_hit_spark(i, n, "yellow")),
        ("vfx_sonic_ring", 8, lambda i, n: draw_ring_vfx(i, n, "purple")),
        ("vfx_spawn_smoke", 8, lambda i, n: draw_smoke(i, n, "cyan")),
        ("vfx_death_noise", 8, lambda i, n: draw_smoke(i, n, "purple")),
        ("vfx_warning_ring", 8, lambda i, n: draw_ring_vfx(i, n, "red")),
    ]
    for prefix, frames, drawer in vfx_specs:
        save_sequence(VFX, prefix, frames, drawer)
        entries.append((VFX, prefix, frames))

    aoe_specs = [
        ("vfx_aoe_fire", lambda i, n: draw_aoe(i, n, "fire", "fire")),
        ("vfx_aoe_ice", lambda i, n: draw_aoe(i, n, "ice", "ice")),
        ("vfx_aoe_poison", lambda i, n: draw_aoe(i, n, "poison", "poison")),
        ("vfx_aoe_sonic", lambda i, n: draw_aoe(i, n, "purple", "ring")),
    ]
    for prefix, drawer in aoe_specs:
        save_sequence(VFX, prefix, 8, drawer)
        entries.append((VFX, prefix, 8))

    skill_specs = ["lightning", "frost_field", "anti_heal", "shield_brand", "bone_wall", "demon_hand"]
    for key in skill_specs:
        prefix = f"vfx_skill_{key}"
        save_sequence(VFX, prefix, 8, lambda i, n, k=key: draw_skill(i, n, k))
        entries.append((VFX, prefix, 8))

    # Keep tile fallbacks visually upgraded too.
    draw_aoe(0, 8, "fire", "fire").save(TILES / "tile_fire.png")
    draw_aoe(0, 8, "ice", "ice").save(TILES / "tile_ice.png")
    draw_aoe(0, 8, "poison", "poison").save(TILES / "tile_poison.png")
    draw_aoe(0, 8, "blue", "ring").save(TILES / "tile_lightning.png")

    make_preview(entries, VFX / "fx_animation_preview.png")
    print(f"Generated {sum(frames for _, _, frames in entries)} animated fx frames.")
    print(f"Preview: {VFX / 'fx_animation_preview.png'}")


if __name__ == "__main__":
    main()
