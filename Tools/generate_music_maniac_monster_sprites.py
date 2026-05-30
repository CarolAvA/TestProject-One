from pathlib import Path
from PIL import Image, ImageDraw
import math


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt" / "characters"
SIZE = 96


PALETTE = {
    "outline": (14, 10, 22, 255),
    "deep": (31, 20, 48, 255),
    "bone": (232, 224, 185, 255),
    "poison": (80, 238, 108, 255),
    "poison_dark": (32, 124, 61, 255),
    "cyan": (62, 235, 238, 255),
    "blue": (54, 118, 238, 255),
    "magenta": (255, 65, 184, 255),
    "purple": (132, 68, 232, 255),
    "orange": (255, 157, 55, 255),
    "yellow": (255, 220, 86, 255),
    "red": (255, 72, 86, 255),
    "white": (238, 248, 255, 255),
    "gray": (94, 100, 122, 255),
    "dark_gray": (43, 46, 64, 255),
}


MONSTERS = [
    ("monster_noise_blob", "Skeleton", "blob", 1.0, "magenta"),
    ("monster_venom_singer", "VenomBug", "singer", 1.0, "poison"),
    ("monster_cassette_thrower", "Archer", "cassette", 1.0, "orange"),
    ("monster_speaker_brute", "Stoneguard", "speaker", 1.16, "cyan"),
    ("monster_metronome_wizard", "HexPriest", "wizard", 1.08, "purple"),
    ("monster_tuning_fork_breaker", "Shieldbreaker", "fork", 1.08, "yellow"),
    ("monster_cable_assassin", "Assassin", "cable", 1.0, "blue"),
    ("monster_distortion_king", "BoneKing", "king", 1.42, "red"),
]

ACTIONS = {
    "idle": (8, 8, True, 0.11),
    "move": (8, 10, True, 0.085),
    "attack": (6, 12, False, 0.075),
    "hit": (4, 12, False, 0.07),
    "death": (8, 8, False, 0.13),
    "spawn": (6, 10, False, 0.08),
}


def c(name):
    return PALETTE[name]


def ellipse(draw, xy, fill, outline="outline", width=1):
    draw.ellipse(xy, fill=c(fill), outline=c(outline) if outline else None, width=width)


def rect(draw, xy, fill, outline="outline", width=1):
    draw.rectangle(xy, fill=c(fill), outline=c(outline) if outline else None, width=width)


def poly(draw, points, fill, outline="outline"):
    draw.polygon(points, fill=c(fill), outline=c(outline) if outline else None)


def line(draw, xy, fill, width=1):
    draw.line(xy, fill=c(fill), width=width)


def shadow(draw, cx, y, radius, alpha=70):
    draw.ellipse((cx - radius, y - 4, cx + radius, y + 3), fill=(0, 0, 0, alpha))


def sparkle(draw, x, y, color="white"):
    line(draw, (x - 2, y, x + 2, y), color)
    line(draw, (x, y - 2, x, y + 2), color)


def note(draw, x, y, color="cyan"):
    ellipse(draw, (x, y + 5, x + 5, y + 10), color, None)
    line(draw, (x + 5, y + 7, x + 5, y), color, 2)
    line(draw, (x + 5, y, x + 10, y + 2), color)


def tint_visible(image, color, strength):
    tint = Image.new("RGBA", image.size, c(color))
    alpha = image.getchannel("A")
    out = Image.blend(image, tint, strength)
    out.putalpha(alpha)
    return out


def pose(action, frame, total):
    phase = frame / max(1, total)
    wave = math.sin(phase * math.tau)
    if action == "idle":
        return int(wave * 1.4), 0, 0, 1.0
    if action == "move":
        return int(abs(wave) * -2), int(wave * 5), int(wave * 2), 1.0
    if action == "attack":
        thrust = [0, 2, 6, 3, 0, -1][frame]
        return -1, thrust, thrust, 1.0
    if action == "hit":
        shake = [0, 4, -3, 1][frame]
        return 1, shake, -shake, 1.0
    if action == "death":
        return min(frame * 2, 8), frame * 2, frame * 9, max(0.28, 1.0 - frame * 0.095)
    if action == "spawn":
        return max(0, 5 - frame), 0, 0, min(1.0, 0.45 + frame * 0.12)
    return 0, 0, 0, 1.0


def transform(points, cx, cy, scale, dx=0, dy=0):
    return [(cx + int(x * scale) + dx, cy + int(y * scale) + dy) for x, y in points]


def draw_blob(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(19 * scale))
    body = (cx - int(19 * scale), base - int(38 * scale) + bob, cx + int(19 * scale), base - int(4 * scale) + bob)
    ellipse(draw, body, "deep")
    ellipse(draw, (body[0] + 5, body[1] + 5, body[2] - 4, body[3] - 3), accent, "outline")
    for i, off in enumerate([-12, 0, 12]):
        ellipse(draw, (cx + int((off + step * 0.3) * scale) - 4, base - 10 + bob, cx + int((off + step * 0.3) * scale) + 4, base - 3 + bob), "deep")
    rect(draw, (cx - 8, base - 26 + bob, cx - 3, base - 20 + bob), "white")
    rect(draw, (cx + 4, base - 26 + bob, cx + 9, base - 20 + bob), "white")
    line(draw, (cx - 8, base - 16 + bob, cx + 8, base - 14 + bob), "outline", 2)
    for i in range(3):
        note(draw, cx - 28 + i * 24, base - 50 - i * 2 + bob, "magenta" if i % 2 else "cyan")


def draw_singer(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(18 * scale))
    ellipse(draw, (cx - 17, base - 43 + bob, cx + 17, base - 10 + bob), "poison", "outline")
    poly(draw, [(cx - 11, base - 37 + bob), (cx - 18, base - 55 + bob), (cx - 4, base - 43 + bob)], "poison_dark")
    poly(draw, [(cx + 8, base - 38 + bob), (cx + 20, base - 53 + bob), (cx + 14, base - 39 + bob)], "poison_dark")
    rect(draw, (cx - 10, base - 14 + bob, cx - 2, base - 6), "dark_gray")
    rect(draw, (cx + 3, base - 14 + bob, cx + 11, base - 6), "dark_gray")
    ellipse(draw, (cx - 7, base - 32 + bob, cx - 2, base - 27 + bob), "outline", None)
    ellipse(draw, (cx + 4, base - 32 + bob, cx + 9, base - 27 + bob), "outline", None)
    ellipse(draw, (cx - 7, base - 24 + bob, cx + 8, base - 14 + bob), "deep")
    line(draw, (cx - 22, base - 24 + bob, cx - 33 - step, base - 18 + bob), "outline", 4)
    line(draw, (cx + 22, base - 24 + bob, cx + 32 + step, base - 18 + bob), "outline", 4)
    note(draw, cx + 24 + step, base - 47 + bob, "poison")


def draw_cassette(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(19 * scale))
    rect(draw, (cx - 21, base - 42 + bob, cx + 21, base - 13 + bob), "orange")
    rect(draw, (cx - 15, base - 36 + bob, cx + 15, base - 25 + bob), "white", "outline")
    ellipse(draw, (cx - 13, base - 24 + bob, cx - 4, base - 15 + bob), "deep")
    ellipse(draw, (cx + 4, base - 24 + bob, cx + 13, base - 15 + bob), "deep")
    line(draw, (cx - 4, base - 20 + bob, cx + 4, base - 20 + bob), "outline", 2)
    rect(draw, (cx - 25 - step, base - 34 + bob, cx - 16 - step, base - 24 + bob), "dark_gray")
    line(draw, (cx + 21, base - 30 + bob, cx + 34 + step, base - 37 + bob), "outline", 4)
    rect(draw, (cx + 31 + step, base - 42 + bob, cx + 42 + step, base - 34 + bob), "orange")
    rect(draw, (cx - 13, base - 13 + bob, cx - 6, base - 5), "dark_gray")
    rect(draw, (cx + 6, base - 13 + bob, cx + 13, base - 5), "dark_gray")


def draw_speaker(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 3, int(24 * scale))
    rect(draw, (cx - 24, base - 54 + bob, cx + 24, base - 6 + bob), "dark_gray")
    rect(draw, (cx - 19, base - 49 + bob, cx + 19, base - 10 + bob), "deep", "cyan", 2)
    ellipse(draw, (cx - 14, base - 43 + bob, cx + 14, base - 15 + bob), "cyan")
    ellipse(draw, (cx - 7, base - 36 + bob, cx + 7, base - 22 + bob), "deep")
    rect(draw, (cx - 17, base - 7, cx - 8, base - 1), "gray")
    rect(draw, (cx + 8, base - 7, cx + 17, base - 1), "gray")
    line(draw, (cx - 25, base - 36 + bob, cx - 35 - step, base - 23 + bob), "outline", 6)
    line(draw, (cx + 25, base - 36 + bob, cx + 35 + step, base - 23 + bob), "outline", 6)
    if action == "attack":
        for r in (20, 28, 36):
            draw.arc((cx - r, base - 45 + bob - r, cx + r, base - 45 + bob + r), 25, 155, fill=c("cyan"), width=1)


def draw_wizard(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(18 * scale))
    poly(draw, [(cx, base - 62 + bob), (cx - 20, base - 40 + bob), (cx + 20, base - 40 + bob)], "purple")
    ellipse(draw, (cx - 14, base - 45 + bob, cx + 14, base - 20 + bob), "bone")
    poly(draw, [(cx - 18, base - 23 + bob), (cx + 18, base - 23 + bob), (cx + 13, base - 5), (cx - 13, base - 5)], "purple")
    line(draw, (cx - 5, base - 36 + bob, cx + 5, base - 36 + bob), "outline", 2)
    line(draw, (cx, base - 55 + bob, cx, base - 66 + bob), "yellow", 2)
    line(draw, (cx - 22, base - 25 + bob, cx - 33 - step, base - 36 + bob), "outline", 4)
    line(draw, (cx + 20, base - 25 + bob, cx + 34 + step, base - 37 + bob), "outline", 4)
    line(draw, (cx + 34 + step, base - 37 + bob, cx + 34 + step, base - 62 + bob), "yellow", 2)
    sparkle(draw, cx + 34 + step, base - 64 + bob, "magenta")


def draw_fork(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(18 * scale))
    poly(draw, [(cx - 17, base - 44 + bob), (cx + 17, base - 44 + bob), (cx + 19, base - 13 + bob), (cx, base - 4), (cx - 19, base - 13 + bob)], "yellow")
    rect(draw, (cx - 8, base - 34 + bob, cx + 8, base - 21 + bob), "deep")
    line(draw, (cx - 20, base - 36 + bob, cx - 35 - step, base - 45 + bob), "outline", 5)
    line(draw, (cx + 20, base - 36 + bob, cx + 35 + step, base - 45 + bob), "outline", 5)
    line(draw, (cx + 35 + step, base - 48 + bob, cx + 35 + step, base - 66 + bob), "cyan", 3)
    line(draw, (cx + 29 + step, base - 66 + bob, cx + 41 + step, base - 66 + bob), "cyan", 2)
    line(draw, (cx + 31 + step, base - 72 + bob, cx + 31 + step, base - 66 + bob), "cyan", 2)
    line(draw, (cx + 39 + step, base - 72 + bob, cx + 39 + step, base - 66 + bob), "cyan", 2)
    rect(draw, (cx - 13, base - 13, cx - 6, base - 5), "dark_gray")
    rect(draw, (cx + 6, base - 13, cx + 13, base - 5), "dark_gray")


def draw_cable(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 2, int(16 * scale))
    poly(draw, [(cx - 13, base - 44 + bob), (cx + 13, base - 44 + bob), (cx + 16, base - 12 + bob), (cx, base - 4), (cx - 16, base - 12 + bob)], "blue")
    ellipse(draw, (cx - 12, base - 58 + bob, cx + 12, base - 36 + bob), "deep")
    rect(draw, (cx - 8, base - 50 + bob, cx + 8, base - 45 + bob), "cyan")
    line(draw, (cx - 13, base - 33 + bob, cx - 31 - step, base - 18 + bob), "outline", 4)
    line(draw, (cx + 13, base - 33 + bob, cx + 31 + step, base - 19 + bob), "outline", 4)
    line(draw, (cx - 20, base - 7, cx - 7, base - 4), "outline", 4)
    line(draw, (cx + 7, base - 4, cx + 22, base - 7), "outline", 4)
    for i in range(2):
        line(draw, (cx - 33 + i * 66 + (-step if i == 0 else step), base - 18 + bob, cx - 41 + i * 82, base - 12 + bob), "cyan", 2)


def draw_king(draw, cx, base, scale, bob, step, action, accent):
    shadow(draw, cx, base + 3, int(27 * scale))
    poly(draw, [(cx - 25, base - 57 + bob), (cx + 25, base - 57 + bob), (cx + 22, base - 8), (cx - 22, base - 8)], "deep")
    rect(draw, (cx - 18, base - 49 + bob, cx + 18, base - 15 + bob), "red")
    ellipse(draw, (cx - 13, base - 44 + bob, cx + 13, base - 18 + bob), "dark_gray")
    poly(draw, [(cx - 18, base - 58 + bob), (cx - 12, base - 72 + bob), (cx - 4, base - 58 + bob), (cx + 5, base - 72 + bob), (cx + 13, base - 58 + bob), (cx + 20, base - 70 + bob), (cx + 21, base - 56 + bob)], "yellow")
    ellipse(draw, (cx - 5, base - 35 + bob, cx + 5, base - 25 + bob), "cyan")
    line(draw, (cx - 26, base - 41 + bob, cx - 42 - step, base - 24 + bob), "outline", 7)
    line(draw, (cx + 26, base - 41 + bob, cx + 42 + step, base - 24 + bob), "outline", 7)
    rect(draw, (cx - 18, base - 10, cx - 7, base - 2), "dark_gray")
    rect(draw, (cx + 7, base - 10, cx + 18, base - 2), "dark_gray")
    if action == "attack":
        for i in range(4):
            line(draw, (cx + 28 + i * 5, base - 54 + bob - i * 4, cx + 39 + i * 5, base - 54 + bob - i * 4), "red", 2)


DRAWERS = {
    "blob": draw_blob,
    "singer": draw_singer,
    "cassette": draw_cassette,
    "speaker": draw_speaker,
    "wizard": draw_wizard,
    "fork": draw_fork,
    "cable": draw_cable,
    "king": draw_king,
}


def compose(prefix, shape, scale, accent, action, frame, total):
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    bob, step, lean, alpha = pose(action, frame, total)
    base = 84 + bob
    cx = 48 + (step if action in ("hit", "death") else 0)

    if action == "spawn":
        for i in range(max(1, 5 - frame)):
            sparkle(draw, 18 + i * 14, 72 - i * 8, accent)

    DRAWERS[shape](draw, cx, base, scale * alpha, bob, step, action, accent)

    if action == "hit":
        img = tint_visible(img, "red", 0.12 + frame * 0.05)
        sparkle(draw, 25, 31, "red")
    elif action == "death" and frame > 4:
        img = tint_visible(img, "deep", min(0.35, (frame - 4) * 0.11))
        for i in range(frame - 3):
            sparkle(draw, 22 + i * 9, 34 + i % 2 * 8, "gray")

    return img


def save_monster(prefix, display, shape, scale, accent):
    count = 0
    for action, (frames, fps, loop, duration) in ACTIONS.items():
        for frame in range(frames):
            image = compose(prefix, shape, scale, accent, action, frame, frames)
            image.save(OUT / f"{prefix}_{action}_{frame:02d}.png")
            count += 1
    Image.open(OUT / f"{prefix}_idle_00.png").save(OUT / f"{prefix}.png")
    return count


def make_preview():
    cols = max(frames for frames, _, _, _ in ACTIONS.values())
    rows = len(MONSTERS) * len(ACTIONS)
    sheet = Image.new("RGBA", (cols * SIZE, rows * SIZE), (18, 16, 24, 255))
    row = 0
    for prefix, _, _, _, _ in MONSTERS:
        for action, (frames, _, _, _) in ACTIONS.items():
            for frame in range(frames):
                image = Image.open(OUT / f"{prefix}_{action}_{frame:02d}.png")
                sheet.alpha_composite(image, (frame * SIZE, row * SIZE))
            row += 1
    sheet.save(OUT / "monster_animation_preview.png")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    total = 0
    for prefix, display, shape, scale, accent in MONSTERS:
        generated = save_monster(prefix, display, shape, scale, accent)
        total += generated
        print(f"{display}: {generated} frames -> {prefix}")
    make_preview()
    print(f"Generated {total} monster frames in enum order.")


if __name__ == "__main__":
    main()
