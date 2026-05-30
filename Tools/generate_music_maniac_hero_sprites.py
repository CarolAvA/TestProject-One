from pathlib import Path
from PIL import Image, ImageDraw
import math
import shutil


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt" / "characters"
SIZE = 96


PALETTE = {
    "outline": (16, 10, 24, 255),
    "outline2": (35, 19, 52, 255),
    "skin": (245, 196, 151, 255),
    "skin_shadow": (176, 105, 104, 255),
    "hair_cyan": (58, 241, 229, 255),
    "hair_blue": (39, 131, 232, 255),
    "hair_magenta": (255, 64, 179, 255),
    "coat": (85, 34, 151, 255),
    "coat_shadow": (43, 24, 83, 255),
    "coat_light": (157, 72, 255, 255),
    "shirt": (23, 214, 238, 255),
    "shirt_dark": (10, 89, 118, 255),
    "gold": (255, 208, 80, 255),
    "boot": (22, 22, 33, 255),
    "white": (236, 246, 255, 255),
    "red": (255, 70, 82, 255),
    "green": (75, 238, 122, 255),
}


def rgba(name):
    return PALETTE[name]


def poly(draw, points, fill, outline="outline"):
    draw.polygon(points, fill=rgba(fill), outline=rgba(outline))


def rect(draw, xy, fill, outline=None):
    draw.rectangle(xy, fill=rgba(fill), outline=rgba(outline) if outline else None)


def ellipse(draw, xy, fill, outline="outline"):
    draw.ellipse(xy, fill=rgba(fill), outline=rgba(outline) if outline else None)


def line(draw, xy, fill, width=1):
    draw.line(xy, fill=rgba(fill), width=width)


def note(draw, x, y, color="gold"):
    ellipse(draw, (x, y + 4, x + 4, y + 8), color, None)
    line(draw, (x + 4, y + 5, x + 4, y), color, 2)
    line(draw, (x + 4, y, x + 8, y + 2), color, 1)


def sparkle(draw, x, y, color="white"):
    line(draw, (x - 2, y, x + 2, y), color, 1)
    line(draw, (x, y - 2, x, y + 2), color, 1)


def tint_visible_pixels(image, color, strength):
    tint = Image.new("RGBA", image.size, color)
    alpha = image.getchannel("A")
    mixed = Image.blend(image, tint, strength)
    mixed.putalpha(alpha)
    return mixed


def draw_shadow(draw, cx, base_y, wide=18):
    draw.ellipse((cx - wide, base_y - 4, cx + wide, base_y + 3), fill=(0, 0, 0, 72))


def draw_keytar(draw, cx, cy, angle=0, scale=1, active=False):
    body = [
        (cx - int(20 * scale), cy + int(5 * scale)),
        (cx + int(11 * scale), cy - int(2 * scale)),
        (cx + int(20 * scale), cy + int(4 * scale)),
        (cx - int(11 * scale), cy + int(11 * scale)),
    ]
    poly(draw, body, "shirt_dark")
    poly(draw, [
        (cx - int(17 * scale), cy + int(6 * scale)),
        (cx + int(8 * scale), cy),
        (cx + int(14 * scale), cy + int(4 * scale)),
        (cx - int(10 * scale), cy + int(9 * scale)),
    ], "white", "outline2")
    for i in range(5):
        x = cx - int(12 * scale) + i * int(5 * scale)
        rect(draw, (x, cy + 4, x + 2, cy + 8), "outline2")
    if active:
        note(draw, cx + 21, cy - 12, "hair_cyan")
        note(draw, cx - 28, cy - 8, "hair_magenta")


def draw_body(draw, cx, base_y, bob=0, lean=0, step=0, arms="idle", expression="cool", tint=None):
    oy = int(bob)
    draw_shadow(draw, cx, base_y, 19)

    hip_y = base_y - 23 + oy
    shoulder_y = base_y - 48 + oy
    head_y = base_y - 70 + oy
    lean_i = int(lean)

    # legs and boots
    left_step = int(step)
    right_step = -int(step)
    line(draw, (cx - 7 + lean_i, hip_y, cx - 12 + left_step, base_y - 9), "outline", 5)
    line(draw, (cx + 7 + lean_i, hip_y, cx + 12 + right_step, base_y - 9), "outline", 5)
    line(draw, (cx - 7 + lean_i, hip_y, cx - 12 + left_step, base_y - 9), "coat_light", 3)
    line(draw, (cx + 7 + lean_i, hip_y, cx + 12 + right_step, base_y - 9), "hair_blue", 3)
    rect(draw, (cx - 19 + left_step, base_y - 10, cx - 7 + left_step, base_y - 5), "boot", "outline")
    rect(draw, (cx + 5 + right_step, base_y - 10, cx + 18 + right_step, base_y - 5), "boot", "outline")

    # coat tails
    poly(draw, [
        (cx - 18 + lean_i, shoulder_y + 20),
        (cx - 26 + lean_i - step // 2, base_y - 14),
        (cx - 7 + lean_i, hip_y + 4),
    ], "coat_shadow")
    poly(draw, [
        (cx + 18 + lean_i, shoulder_y + 20),
        (cx + 25 + lean_i + step // 2, base_y - 13),
        (cx + 6 + lean_i, hip_y + 4),
    ], "coat")

    # torso coat
    poly(draw, [
        (cx - 18 + lean_i, shoulder_y),
        (cx + 17 + lean_i, shoulder_y),
        (cx + 15 + lean_i, hip_y + 6),
        (cx - 14 + lean_i, hip_y + 6),
    ], "coat")
    poly(draw, [
        (cx - 8 + lean_i, shoulder_y + 4),
        (cx + 9 + lean_i, shoulder_y + 4),
        (cx + 7 + lean_i, hip_y + 1),
        (cx - 5 + lean_i, hip_y + 1),
    ], "shirt")
    rect(draw, (cx - 5 + lean_i, shoulder_y + 13, cx + 6 + lean_i, shoulder_y + 24), "outline2", "outline")
    ellipse(draw, (cx - 3 + lean_i, shoulder_y + 15, cx + 4 + lean_i, shoulder_y + 22), "gold", "outline2")
    for i, h in enumerate([4, 8, 6]):
        rect(draw, (cx - 12 + i * 5 + lean_i, hip_y - h, cx - 10 + i * 5 + lean_i, hip_y), "green")

    # arms
    if arms == "hit":
        line(draw, (cx - 17 + lean_i, shoulder_y + 6, cx - 31 + lean_i, shoulder_y - 7), "outline", 5)
        line(draw, (cx + 17 + lean_i, shoulder_y + 6, cx + 31 + lean_i, shoulder_y - 3), "outline", 5)
        line(draw, (cx - 17 + lean_i, shoulder_y + 6, cx - 31 + lean_i, shoulder_y - 7), "skin", 3)
        line(draw, (cx + 17 + lean_i, shoulder_y + 6, cx + 31 + lean_i, shoulder_y - 3), "skin", 3)
    elif arms == "cast":
        line(draw, (cx - 17 + lean_i, shoulder_y + 8, cx - 33 + lean_i, shoulder_y - 10), "outline", 5)
        line(draw, (cx - 17 + lean_i, shoulder_y + 8, cx - 33 + lean_i, shoulder_y - 10), "skin", 3)
        line(draw, (cx + 17 + lean_i, shoulder_y + 8, cx + 30 + lean_i, shoulder_y + 17), "outline", 5)
        line(draw, (cx + 17 + lean_i, shoulder_y + 8, cx + 30 + lean_i, shoulder_y + 17), "skin", 3)
        note(draw, cx - 43 + lean_i, shoulder_y - 23, "hair_cyan")
    else:
        line(draw, (cx - 17 + lean_i, shoulder_y + 7, cx - 28 + lean_i - step // 2, hip_y - 2), "outline", 5)
        line(draw, (cx + 17 + lean_i, shoulder_y + 7, cx + 28 + lean_i + step // 2, hip_y - 2), "outline", 5)
        line(draw, (cx - 17 + lean_i, shoulder_y + 7, cx - 28 + lean_i - step // 2, hip_y - 2), "skin", 3)
        line(draw, (cx + 17 + lean_i, shoulder_y + 7, cx + 28 + lean_i + step // 2, hip_y - 2), "skin", 3)

    draw_keytar(draw, cx + lean_i, shoulder_y + 21, active=arms in ("attack", "cast"))

    # neck and head
    rect(draw, (cx - 5 + lean_i, head_y + 18, cx + 5 + lean_i, head_y + 25), "skin_shadow", "outline")
    ellipse(draw, (cx - 14 + lean_i, head_y, cx + 14 + lean_i, head_y + 24), "skin", "outline")

    # hair spikes
    poly(draw, [
        (cx - 15 + lean_i, head_y + 5),
        (cx - 22 + lean_i, head_y - 6),
        (cx - 7 + lean_i, head_y),
    ], "hair_magenta")
    poly(draw, [
        (cx - 7 + lean_i, head_y + 1),
        (cx + 2 + lean_i, head_y - 12),
        (cx + 7 + lean_i, head_y + 1),
    ], "hair_cyan")
    poly(draw, [
        (cx + 4 + lean_i, head_y + 2),
        (cx + 19 + lean_i, head_y - 5),
        (cx + 12 + lean_i, head_y + 9),
    ], "hair_blue")

    # headphones and face
    line(draw, (cx - 11 + lean_i, head_y + 1, cx + 11 + lean_i, head_y + 1), "outline", 3)
    ellipse(draw, (cx - 18 + lean_i, head_y + 6, cx - 9 + lean_i, head_y + 17), "coat_light", "outline")
    ellipse(draw, (cx + 9 + lean_i, head_y + 6, cx + 18 + lean_i, head_y + 17), "coat_light", "outline")
    rect(draw, (cx - 9 + lean_i, head_y + 10, cx - 2 + lean_i, head_y + 14), "outline")
    rect(draw, (cx + 2 + lean_i, head_y + 10, cx + 9 + lean_i, head_y + 14), "outline")
    line(draw, (cx - 2 + lean_i, head_y + 12, cx + 2 + lean_i, head_y + 12), "outline", 1)
    if expression == "hurt":
        line(draw, (cx - 6 + lean_i, head_y + 18, cx + 5 + lean_i, head_y + 16), "red", 1)
    elif expression == "dead":
        line(draw, (cx - 7 + lean_i, head_y + 12, cx - 3 + lean_i, head_y + 16), "outline", 1)
        line(draw, (cx - 3 + lean_i, head_y + 12, cx - 7 + lean_i, head_y + 16), "outline", 1)
        line(draw, (cx + 3 + lean_i, head_y + 12, cx + 7 + lean_i, head_y + 16), "outline", 1)
        line(draw, (cx + 7 + lean_i, head_y + 12, cx + 3 + lean_i, head_y + 16), "outline", 1)
    else:
        rect(draw, (cx - 3 + lean_i, head_y + 17, cx + 6 + lean_i, head_y + 18), "white")

    # highlights
    if tint:
        overlay = Image.new("RGBA", (SIZE, SIZE), tint)
        return overlay
    return None


def draw_prone(draw, frame):
    base_y = 75
    x = 48 + min(frame, 3)
    draw_shadow(draw, x, base_y + 8, 24)
    poly(draw, [(x - 22, base_y - 14), (x + 16, base_y - 18), (x + 25, base_y - 4), (x - 16, base_y)], "coat")
    rect(draw, (x - 5, base_y - 22, x + 25, base_y - 14), "shirt", "outline")
    ellipse(draw, (x - 32, base_y - 24, x - 11, base_y - 4), "skin", "outline")
    poly(draw, [(x - 31, base_y - 19), (x - 43, base_y - 28), (x - 21, base_y - 23)], "hair_magenta")
    poly(draw, [(x - 25, base_y - 23), (x - 15, base_y - 34), (x - 12, base_y - 18)], "hair_cyan")
    rect(draw, (x - 30, base_y - 15, x - 24, base_y - 11), "outline")
    rect(draw, (x - 21, base_y - 16, x - 15, base_y - 12), "outline")
    line(draw, (x - 4, base_y - 10, x + 24, base_y - 6), "outline", 4)
    line(draw, (x - 4, base_y - 10, x + 24, base_y - 6), "boot", 2)
    for i in range(frame):
        sparkle(draw, 25 + i * 9, 38 - i % 2 * 5, "hair_blue")


def compose(action, frame, total):
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if action == "idle":
        bob = [0, -1, -2, -1, 0, 1, 0, -1][frame]
        draw_body(draw, 48, 84, bob=bob, lean=math.sin(frame / total * math.tau) * 1.3, step=0, arms="idle")
        if frame in (1, 5):
            note(draw, 24, 27, "hair_cyan")
        if frame in (3, 7):
            note(draw, 68, 25, "hair_magenta")
    elif action == "move":
        step = int(math.sin(frame / total * math.tau) * 8)
        bob = -1 if frame % 2 else 1
        draw_body(draw, 48, 84, bob=bob, lean=step * 0.18, step=step, arms="idle")
        line(draw, (30, 88, 22, 88), "hair_blue", 1)
        line(draw, (65, 88, 74, 88), "hair_magenta", 1)
    elif action == "hit":
        offsets = [0, 3, -2, 1]
        draw_body(draw, 48 + offsets[frame], 84, bob=1, lean=offsets[frame], step=0, arms="hit", expression="hurt")
        sparkle(draw, 26, 30, "red")
        sparkle(draw, 70, 28, "gold")
        img = tint_visible_pixels(img, (255, 82, 96, 255), 0.12 + frame * 0.04)
    elif action == "death":
        if frame < 3:
            draw_body(draw, 48, 85 + frame * 3, bob=frame * 2, lean=frame * 3, step=0, arms="hit", expression="dead")
            note(draw, 28, 20 + frame * 3, "hair_blue")
        else:
            draw_prone(draw, frame - 2)
            if frame > 5:
                img = tint_visible_pixels(img, (18, 12, 28, 255), min(0.32, (frame - 5) * 0.12))
    elif action == "attack":
        step = [0, -3, -5, -3, 0, 2][frame]
        draw_body(draw, 48, 84, bob=-1, lean=-step * 0.4, step=step, arms="attack")
        for i in range(3):
            note(draw, 68 + i * 5, 31 - i * 4, "gold")
    elif action == "cast":
        draw_body(draw, 48, 84, bob=-frame % 2, lean=0, step=0, arms="cast")
        r = 8 + frame * 3
        draw.ellipse((48 - r, 41 - r, 48 + r, 41 + r), outline=rgba("hair_cyan"), width=1)
        draw.ellipse((48 - r - 4, 41 - r - 4, 48 + r + 4, 41 + r + 4), outline=rgba("hair_magenta"), width=1)
    elif action == "spawn":
        scale_shift = max(0, 5 - frame)
        draw_body(draw, 48, 84 + scale_shift, bob=scale_shift, lean=0, step=0, arms="idle")
        for i in range(5 - frame if frame < 5 else 1):
            sparkle(draw, 24 + i * 10, 72 - i * 7, "hair_cyan")

    return img


def save_action(action, frames):
    paths = []
    for i in range(frames):
        img = compose(action, i, frames)
        path = OUT / f"hero_music_maniac_{action}_{i:02d}.png"
        img.save(path)
        paths.append(path)
    return paths


def make_preview(actions):
    cell = 96
    labels = list(actions.keys())
    sheet = Image.new("RGBA", (cell * max(actions.values()), cell * len(labels)), (18, 16, 24, 255))
    for row, action in enumerate(labels):
        for i in range(actions[action]):
            img = Image.open(OUT / f"hero_music_maniac_{action}_{i:02d}.png")
            sheet.alpha_composite(img, (i * cell, row * cell))
    sheet.save(OUT / "hero_music_maniac_animation_preview.png")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    actions = {
        "idle": 8,
        "move": 8,
        "hit": 4,
        "death": 8,
        "attack": 6,
        "cast": 6,
        "spawn": 6,
    }
    for action, count in actions.items():
        save_action(action, count)
    shutil.copyfile(OUT / "hero_music_maniac_idle_00.png", OUT / "hero_music_maniac.png")
    make_preview(actions)
    print(f"Generated {sum(actions.values())} hero frames in {OUT}")


if __name__ == "__main__":
    main()
