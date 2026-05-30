from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt"


def rgba(hex_color, alpha=255):
    hex_color = hex_color.lstrip("#")
    return tuple(int(hex_color[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


PAL = {
    "ink": rgba("#06070b"),
    "panel": rgba("#141420", 238),
    "panel2": rgba("#1e1730", 232),
    "line": rgba("#2f2a3d", 255),
    "white": rgba("#f4f0ff"),
    "muted": rgba("#807b95"),
    "cyan": rgba("#29d8ff"),
    "blue": rgba("#4778ff"),
    "pink": rgba("#ff2c9b"),
    "purple": rgba("#8a43ff"),
    "orange": rgba("#ff8a2c"),
    "yellow": rgba("#ffd84a"),
    "green": rgba("#61ff74"),
    "toxic": rgba("#c4ff2d"),
    "ice": rgba("#9feaff"),
    "red": rgba("#ff3656"),
}


def save(img, folder, name):
    path = OUT / folder / f"{name}.png"
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)
    return path


def glow_layer(size, draw_fn, blur=5):
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(layer))
    return layer.filter(ImageFilter.GaussianBlur(blur))


def panel_asset(name, size, accent, fill=None, strong=False):
    fill = fill or PAL["panel"]
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    w, h = size

    def glow(d):
        d.rounded_rectangle((10, 10, w - 10, h - 10), radius=8, outline=accent, width=5)

    img.alpha_composite(glow_layer(size, glow, 7))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((7, 7, w - 8, h - 8), radius=7, fill=fill, outline=PAL["ink"], width=4)
    d.rounded_rectangle((12, 12, w - 13, h - 13), radius=5, outline=(*accent[:3], 180), width=2)
    d.rectangle((18, h - 19, w - 18, h - 15), fill=(*accent[:3], 90))
    d.rectangle((18, 16, min(w - 18, 74), 20), fill=accent)
    d.rectangle((w - 74, 16, w - 18, 20), fill=(*accent[:3], 150))
    if strong:
        for i in range(5):
            x = 22 + i * 18
            d.rectangle((x, h - 30, x + 8, h - 22), fill=(*accent[:3], 145))
    return save(img, "ui", name)


def button_asset(name, size, accent, fill=rgba("#191626", 244)):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    w, h = size

    def glow(d):
        d.rounded_rectangle((9, 9, w - 9, h - 9), radius=8, outline=accent, width=5)

    img.alpha_composite(glow_layer(size, glow, 5))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((6, 6, w - 7, h - 7), radius=8, fill=fill, outline=PAL["ink"], width=4)
    d.rounded_rectangle((12, 12, w - 13, h - 13), radius=5, outline=(*accent[:3], 170), width=2)
    d.rectangle((15, 14, w - 15, 20), fill=(*accent[:3], 60))
    d.rectangle((15, h - 22, w - 15, h - 16), fill=(*accent[:3], 95))
    d.line((20, 30, w - 20, 30), fill=(*PAL["white"][:3], 24), width=1)
    return save(img, "ui", name)


def icon_slot(name, accent):
    size = (128, 128)
    img = Image.new("RGBA", size, (0, 0, 0, 0))

    def glow(d):
        d.rounded_rectangle((20, 18, 108, 106), radius=12, outline=accent, width=6)

    img.alpha_composite(glow_layer(size, glow, 8))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((18, 16, 110, 108), radius=12, fill=rgba("#0b0d16", 226), outline=PAL["ink"], width=4)
    d.rounded_rectangle((25, 23, 103, 101), radius=8, outline=(*accent[:3], 190), width=3)
    d.ellipse((43, 41, 85, 83), outline=(*accent[:3], 80), width=2)
    d.rectangle((48, 98, 80, 104), fill=(*accent[:3], 145))
    return save(img, "ui", name)


def badge_asset(name, accent):
    size = (96, 40)
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    img.alpha_composite(glow_layer(size, lambda d: d.rounded_rectangle((7, 7, 88, 32), radius=8, outline=accent, width=4), 4))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((5, 5, 90, 34), radius=8, fill=rgba("#12131c", 232), outline=PAL["ink"], width=3)
    d.rounded_rectangle((10, 10, 85, 29), radius=5, outline=(*accent[:3], 190), width=2)
    d.rectangle((15, 25, 80, 28), fill=(*accent[:3], 110))
    return save(img, "ui", name)


def icon_canvas():
    return Image.new("RGBA", (96, 96), (0, 0, 0, 0))


def draw_icon_back(d, accent):
    d.rounded_rectangle((5, 5, 90, 90), radius=10, fill=rgba("#11131d", 238), outline=PAL["ink"], width=4)
    d.rounded_rectangle((11, 11, 84, 84), radius=7, outline=(*accent[:3], 180), width=3)
    d.ellipse((22, 22, 74, 74), fill=(*accent[:3], 38), outline=(*accent[:3], 95), width=2)


def spark_lines(d, cx, cy, color, r=28, width=4):
    for dx, dy in ((1, 0), (0, 1), (1, 1), (1, -1)):
        d.line((cx - dx * r, cy - dy * r, cx + dx * r, cy + dy * r), fill=(*color[:3], 230), width=width)


def note(d, x, y, color, scale=2):
    d.ellipse((x, y + 20 * scale, x + 11 * scale, y + 31 * scale), fill=color, outline=PAL["ink"], width=2)
    d.rectangle((x + 10 * scale, y, x + 14 * scale, y + 25 * scale), fill=color)
    d.line((x + 12 * scale, y, x + 30 * scale, y + 7 * scale), fill=color, width=4)


def skill_icon(name, accent, symbol):
    img = icon_canvas()
    img.alpha_composite(glow_layer((96, 96), lambda d: d.rounded_rectangle((10, 10, 86, 86), radius=11, outline=accent, width=6), 6))
    d = ImageDraw.Draw(img)
    draw_icon_back(d, accent)
    if symbol == "bolt":
        d.line((58, 16, 33, 47, 50, 47, 35, 80), fill=PAL["white"], width=8)
        d.line((58, 16, 33, 47, 50, 47, 35, 80), fill=accent, width=4)
    elif symbol == "ice":
        spark_lines(d, 48, 48, PAL["white"], 30, 6)
        spark_lines(d, 48, 48, accent, 24, 3)
    elif symbol == "cross":
        d.line((25, 25, 71, 71), fill=PAL["red"], width=10)
        d.line((71, 25, 25, 71), fill=PAL["red"], width=10)
        d.rounded_rectangle((40, 17, 56, 79), radius=3, fill=PAL["white"])
        d.rounded_rectangle((17, 40, 79, 56), radius=3, fill=PAL["white"])
    elif symbol == "shield":
        d.polygon([(48, 15), (75, 27), (68, 68), (48, 82), (28, 68), (21, 27)], fill=(*accent[:3], 190), outline=PAL["ink"])
        d.line((28, 66, 68, 30), fill=PAL["white"], width=6)
    elif symbol == "wall":
        for y in (23, 42, 61):
            d.rounded_rectangle((21, y, 75, y + 14), radius=3, fill=(*accent[:3], 215), outline=PAL["ink"], width=2)
            d.line((48, y + 1, 48, y + 13), fill=PAL["ink"], width=2)
    elif symbol == "hand":
        d.ellipse((31, 38, 67, 78), fill=accent, outline=PAL["ink"], width=3)
        for x, h in ((22, 28), (34, 18), (46, 15), (58, 21), (70, 34)):
            d.rounded_rectangle((x, h, x + 11, 52), radius=5, fill=accent, outline=PAL["ink"], width=2)
    elif symbol == "fire":
        d.polygon([(31, 78), (43, 45), (50, 64), (65, 21), (72, 79)], fill=PAL["orange"], outline=PAL["ink"])
        d.polygon([(41, 77), (50, 55), (58, 78)], fill=PAL["yellow"])
    elif symbol == "poison":
        d.ellipse((30, 23, 66, 72), fill=accent, outline=PAL["ink"], width=3)
        d.ellipse((39, 36, 47, 44), fill=PAL["ink"])
        d.ellipse((53, 36, 61, 44), fill=PAL["ink"])
        d.rectangle((42, 55, 57, 60), fill=PAL["ink"])
    elif symbol == "note":
        note(d, 31, 15, accent, 1)
        for r in (17, 27):
            d.arc((48 - r, 48 - r, 48 + r, 48 + r), start=300, end=55, fill=PAL["white"], width=3)
    else:
        spark_lines(d, 48, 48, accent, 27, 5)
        d.ellipse((40, 40, 56, 56), fill=PAL["white"], outline=accent, width=3)
    save(img, "icons", name)


def hud_icons():
    skill_icon("skill_lightning", PAL["cyan"], "bolt")
    skill_icon("skill_frost_field", PAL["ice"], "ice")
    skill_icon("skill_anti_heal", PAL["green"], "cross")
    skill_icon("skill_shield_brand", PAL["cyan"], "shield")
    skill_icon("skill_bone_wall", rgba("#ddd7ad"), "wall")
    skill_icon("skill_demon_hand", PAL["red"], "hand")
    skill_icon("skill_poison_circle", PAL["toxic"], "poison")
    skill_icon("skill_fire_burst", PAL["orange"], "fire")
    skill_icon("skill_spikes", PAL["white"], "spark")
    skill_icon("skill_sonic_lock", PAL["pink"], "note")
    skill_icon("skill_monster_buff", PAL["red"], "spark")
    skill_icon("skill_boss_summon", PAL["purple"], "hand")
    skill_icon("resource_energy", PAL["yellow"], "spark")
    skill_icon("danger_warning", PAL["red"], "spark")
    skill_icon("track_low", PAL["orange"], "note")
    skill_icon("track_mid", PAL["purple"], "note")
    skill_icon("track_high", PAL["cyan"], "note")


def ui_assets():
    panel_asset("ui_panel", (128, 128), PAL["purple"])
    panel_asset("ui_bd_card", (128, 128), PAL["pink"], rgba("#191424", 236))
    panel_asset("ui_danger_frame", (128, 128), PAL["red"], rgba("#191118", 238), True)
    panel_asset("ui_rhythm_bar", (256, 96), PAL["orange"], rgba("#15131c", 242), True)
    panel_asset("ui_bubble", (128, 96), PAL["cyan"], rgba("#f2eef6", 236))
    panel_asset("ui_portrait_frame", (128, 128), PAL["yellow"], rgba("#16141d", 236), True)
    button_asset("ui_button", (128, 64), PAL["cyan"])
    button_asset("ui_icon_button", (160, 128), PAL["pink"], rgba("#171421", 246))
    icon_slot("ui_icon_slot", PAL["cyan"])
    badge_asset("ui_status_ready", PAL["green"])
    badge_asset("ui_status_low", PAL["red"])
    badge_asset("ui_status_cd", PAL["orange"])
    badge_asset("ui_status_armed", PAL["yellow"])
    button_asset("ui_section_header", (192, 48), PAL["purple"], rgba("#171421", 236))


def main():
    ui_assets()
    hud_icons()
    print("Generated improved Music Maniac UI assets.")


if __name__ == "__main__":
    main()
