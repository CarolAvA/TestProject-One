from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt"


def rgba(hex_color, alpha=255):
    hex_color = hex_color.lstrip("#")
    return tuple(int(hex_color[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


PAL = {
    "outline": rgba("#141018"),
    "black": rgba("#07070a"),
    "white": rgba("#f7f2e8"),
    "gray": rgba("#57515f"),
    "dark": rgba("#242230"),
    "purple": rgba("#6d2db5"),
    "magenta": rgba("#f230a6"),
    "cyan": rgba("#22d6ff"),
    "blue": rgba("#3c75ff"),
    "red": rgba("#ef3650"),
    "orange": rgba("#ff7b22"),
    "yellow": rgba("#ffd447"),
    "green": rgba("#5cff58"),
    "toxic": rgba("#baff24"),
    "ice": rgba("#9be8ff"),
}


def image(size):
    return Image.new("RGBA", (size, size), (0, 0, 0, 0))


def save(img, folder, name):
    path = OUT / folder / f"{name}.png"
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)


def rect(d, xy, fill, outline=None, w=1):
    d.rectangle(xy, fill=fill)
    if outline:
        for i in range(w):
            d.rectangle((xy[0] - i, xy[1] - i, xy[2] + i, xy[3] + i), outline=outline)


def ellipse(d, xy, fill, outline=None, w=1):
    d.ellipse(xy, fill=fill)
    if outline:
        for i in range(w):
            d.ellipse((xy[0] - i, xy[1] - i, xy[2] + i, xy[3] + i), outline=outline)


def line(d, points, fill, width=2):
    d.line(points, fill=fill, width=width)


def note(d, x, y, c, scale=1):
    ellipse(d, (x, y + 11 * scale, x + 10 * scale, y + 20 * scale), c, PAL["outline"], max(1, scale))
    rect(d, (x + 8 * scale, y, x + 11 * scale, y + 15 * scale), c)
    line(d, [(x + 10 * scale, y), (x + 23 * scale, y + 4 * scale)], c, max(2, 2 * scale))


def spark(d, cx, cy, color, r=18):
    for dx, dy in [(1, 0), (0, 1), (1, 1), (1, -1)]:
        line(d, [(cx - dx * r, cy - dy * r), (cx + dx * r, cy + dy * r)], color, 3)
    ellipse(d, (cx - 5, cy - 5, cx + 5, cy + 5), PAL["white"], color, 2)


def draw_hero():
    img = image(96)
    d = ImageDraw.Draw(img)
    line(d, [(26, 72), (34, 86)], PAL["cyan"], 5)
    line(d, [(62, 72), (70, 86)], PAL["magenta"], 5)
    rect(d, (30, 38, 66, 74), PAL["purple"], PAL["outline"], 3)
    rect(d, (25, 45, 36, 71), PAL["black"], PAL["outline"], 2)
    rect(d, (60, 45, 71, 71), PAL["black"], PAL["outline"], 2)
    ellipse(d, (29, 12, 67, 48), rgba("#eadac8"), PAL["outline"], 3)
    for p in [(38, 10), (44, 3), (50, 9), (57, 2), (62, 13)]:
        line(d, [(48, 18), p], PAL["magenta"], 4)
    ellipse(d, (35, 23, 43, 31), PAL["white"], PAL["outline"], 1)
    ellipse(d, (53, 23, 61, 31), PAL["white"], PAL["outline"], 1)
    rect(d, (38, 26, 41, 29), PAL["black"])
    rect(d, (56, 26, 59, 29), PAL["black"])
    rect(d, (38, 35, 59, 42), PAL["black"], PAL["outline"], 1)
    rect(d, (42, 36, 56, 39), PAL["white"])
    ellipse(d, (22, 22, 33, 37), PAL["cyan"], PAL["outline"], 2)
    ellipse(d, (63, 22, 74, 37), PAL["cyan"], PAL["outline"], 2)
    line(d, [(30, 27), (66, 27)], PAL["cyan"], 3)
    rect(d, (66, 47, 84, 67), PAL["dark"], PAL["outline"], 2)
    ellipse(d, (70, 51, 81, 62), PAL["magenta"], PAL["cyan"], 2)
    line(d, [(21, 42), (8, 25)], PAL["white"], 3)
    rect(d, (4, 19, 14, 25), PAL["cyan"], PAL["outline"], 1)
    note(d, 9, 8, PAL["magenta"])
    save(img, "characters", "hero_music_maniac")


def draw_monster(name, size, colors, mode):
    img = image(size)
    d = ImageDraw.Draw(img)
    c0, c1, c2 = colors
    cx = size // 2
    if mode == "blob":
        ellipse(d, (24, 34, size - 24, size - 18), c0, PAL["outline"], 4)
        for x, y in [(25, 47), (19, 61), (72, 43), (78, 59)]:
            ellipse(d, (x, y, x + 12, y + 12), c1, PAL["outline"], 2)
        ellipse(d, (36, 48, 48, 60), PAL["white"], PAL["outline"], 1)
        ellipse(d, (55, 46, 67, 58), PAL["white"], PAL["outline"], 1)
        rect(d, (42, 68, 62, 75), PAL["black"], PAL["outline"], 1)
    elif mode == "bubble":
        ellipse(d, (28, 22, size - 28, size - 18), rgba("#4fff75", 155), PAL["outline"], 4)
        ellipse(d, (42, 44, 63, 65), c1, PAL["outline"], 2)
        for x, y in [(31, 33), (70, 39), (45, 77)]:
            ellipse(d, (x, y, x + 9, y + 9), c2, PAL["outline"], 1)
        rect(d, (39, 22, 62, 33), PAL["dark"], PAL["outline"], 2)
    elif mode == "cassette":
        rect(d, (24, 31, size - 24, size - 24), c0, PAL["outline"], 4)
        rect(d, (32, 42, size - 32, 52), c1, PAL["outline"], 1)
        ellipse(d, (34, 58, 48, 72), PAL["dark"], PAL["white"], 2)
        ellipse(d, (size - 48, 58, size - 34, 72), PAL["dark"], PAL["white"], 2)
        line(d, [(21, 42), (8, 32)], c2, 4)
        note(d, size - 24, 18, c2)
    elif mode == "speaker":
        rect(d, (24, 16, size - 24, size - 12), c0, PAL["outline"], 5)
        ellipse(d, (39, 28, size - 39, size - 64), c2, PAL["outline"], 3)
        ellipse(d, (31, size - 61, size - 31, size - 19), c1, PAL["outline"], 4)
        ellipse(d, (44, size - 48, size - 44, size - 32), PAL["black"], c2, 2)
    elif mode == "wizard":
        rect(d, (42, 16, size - 42, size - 20), c0, PAL["outline"], 4)
        line(d, [(cx, 26), (cx - 18, 70)], c2, 3)
        ellipse(d, (33, 70, size - 33, size - 15), c1, PAL["outline"], 3)
        line(d, [(73, 45), (93, 35)], PAL["white"], 3)
    elif mode == "fork":
        rect(d, (38, 38, size - 38, size - 20), c0, PAL["outline"], 4)
        line(d, [(35, 25), (35, 72), (45, 72), (45, 25)], c2, 4)
        line(d, [(size - 35, 25), (size - 35, 72), (size - 45, 72), (size - 45, 25)], c2, 4)
        ellipse(d, (45, 45, size - 45, 65), PAL["black"], c1, 2)
    elif mode == "assassin":
        ellipse(d, (42, 16, size - 42, 42), c1, PAL["outline"], 3)
        rect(d, (36, 40, size - 36, size - 16), c0, PAL["outline"], 3)
        for side in [-1, 1]:
            line(d, [(cx, 54), (cx + side * 34, 28), (cx + side * 44, 20)], c2, 3)
            line(d, [(cx, 72), (cx + side * 31, 89)], c2, 3)
        rect(d, (43, 26, 56, 31), PAL["green"])
    elif mode == "boss":
        rect(d, (32, 28, size - 32, size - 24), c0, PAL["outline"], 6)
        ellipse(d, (48, 53, size - 48, size - 49), PAL["black"], c2, 5)
        rect(d, (56, 28, size - 56, 43), c1, PAL["outline"], 3)
        for i in range(6):
            h = 18 + (i % 3) * 12
            rect(d, (45 + i * 15, size - 42 - h, 52 + i * 15, size - 42), c2)
        for p in [(30, 30), (20, 54), (36, 91), (size - 30, 30), (size - 20, 54), (size - 36, 91)]:
            line(d, [(cx, 65), p], PAL["magenta"], 4)
    save(img, "characters", name)


def draw_projectiles():
    data = {
        "projectile_basic_note": (PAL["white"], PAL["magenta"]),
        "projectile_lightning_note": (PAL["cyan"], PAL["blue"]),
        "projectile_ice_note": (PAL["ice"], PAL["white"]),
        "projectile_fire_note": (PAL["orange"], PAL["yellow"]),
        "projectile_poison_note": (PAL["toxic"], PAL["green"]),
        "projectile_sonic_ring": (PAL["purple"], PAL["magenta"]),
    }
    for name, (a, b) in data.items():
        img = image(64)
        d = ImageDraw.Draw(img)
        if "sonic" in name:
            for r in [10, 18, 26]:
                ellipse(d, (32 - r, 32 - r, 32 + r, 32 + r), (0, 0, 0, 0), b, 2)
        else:
            ellipse(d, (13, 13, 51, 51), (*a[:3], 90), b, 3)
            note(d, 22, 16, a, 1)
            if "lightning" in name:
                line(d, [(41, 8), (28, 31), (39, 31), (25, 57)], PAL["white"], 3)
            if "ice" in name:
                spark(d, 32, 33, PAL["ice"], 16)
            if "fire" in name:
                line(d, [(23, 51), (31, 35), (38, 51), (44, 27), (50, 53)], PAL["orange"], 4)
            if "poison" in name:
                ellipse(d, (9, 40, 20, 51), PAL["toxic"], PAL["outline"], 1)
        save(img, "projectiles", name)


def draw_icon(folder, name, base, symbol):
    img = image(64)
    d = ImageDraw.Draw(img)
    rect(d, (5, 5, 58, 58), PAL["dark"], PAL["outline"], 3)
    rect(d, (8, 8, 55, 55), (*base[:3], 55))
    if symbol == "bolt":
        line(d, [(38, 10), (23, 34), (36, 34), (24, 55)], base, 6)
        line(d, [(38, 10), (23, 34), (36, 34), (24, 55)], PAL["white"], 2)
    elif symbol == "ice":
        spark(d, 32, 32, base, 22)
    elif symbol == "cross":
        line(d, [(20, 20), (44, 44)], base, 6)
        line(d, [(44, 20), (20, 44)], base, 6)
        rect(d, (26, 15, 38, 49), PAL["white"], None)
        rect(d, (15, 26, 49, 38), PAL["white"], None)
    elif symbol == "shield":
        line(d, [(32, 12), (49, 21), (44, 47), (32, 56), (20, 47), (15, 21), (32, 12)], base, 4)
        line(d, [(18, 45), (46, 22)], PAL["white"], 4)
    elif symbol == "wall":
        for y in [17, 31, 45]:
            rect(d, (14, y, 50, y + 10), base, PAL["outline"], 1)
    elif symbol == "hand":
        ellipse(d, (21, 26, 44, 54), base, PAL["outline"], 2)
        for x in [17, 25, 33, 41]:
            rect(d, (x, 10, x + 7, 34), base, PAL["outline"], 1)
    elif symbol == "fire":
        line(d, [(22, 53), (31, 28), (38, 53), (46, 18), (52, 54)], base, 6)
    elif symbol == "poison":
        ellipse(d, (20, 18, 44, 49), base, PAL["outline"], 2)
        ellipse(d, (27, 25, 34, 32), PAL["outline"])
    elif symbol == "note":
        note(d, 22, 16, base)
    elif symbol == "danger":
        line(d, [(32, 11), (54, 51), (10, 51), (32, 11)], base, 4)
        rect(d, (30, 25, 34, 40), PAL["white"])
        rect(d, (30, 44, 34, 48), PAL["white"])
    else:
        spark(d, 32, 32, base, 20)
    save(img, folder, name)


def draw_tiles():
    tile_defs = {
        "tile_wood_floor": (rgba("#5b3929"), rgba("#9b6547")),
        "tile_dark_brick": (rgba("#252638"), PAL["purple"]),
        "tile_concrete_roof": (rgba("#4e5561"), PAL["cyan"]),
        "tile_asphalt_wave": (rgba("#25252a"), PAL["magenta"]),
        "tile_stage_floor": (rgba("#1d1b28"), PAL["cyan"]),
        "tile_soundwave_floor": (rgba("#211535"), PAL["magenta"]),
        "tile_ice": (rgba("#326e8f"), PAL["ice"]),
        "tile_fire": (rgba("#4b2017"), PAL["orange"]),
        "tile_poison": (rgba("#24321d"), PAL["toxic"]),
        "tile_lightning": (rgba("#17264c"), PAL["cyan"]),
    }
    for name, (base, accent) in tile_defs.items():
        img = image(32)
        d = ImageDraw.Draw(img)
        rect(d, (0, 0, 31, 31), base)
        if "wood" in name:
            for y in [7, 15, 24]:
                line(d, [(0, y), (31, y + 1)], accent, 1)
        elif "brick" in name:
            for y in [0, 10, 20, 31]:
                line(d, [(0, y), (31, y)], rgba("#0f1018"), 1)
            for x in [8, 21]:
                line(d, [(x, 0), (x, 10)], rgba("#0f1018"), 1)
            line(d, [(3, 25), (28, 6)], accent, 1)
        else:
            for i in range(3):
                x = 5 + i * 9
                line(d, [(x, 7), (x + 5, 16), (x - 1, 24)], accent, 1)
        save(img, "tiles", name)


def draw_decor():
    decor = ["poster", "speaker", "light_rig", "cable", "mic_stand", "drum_kit", "dj_table", "crate", "barrier", "road_sign", "neon_sign"]
    for name in decor:
        img = image(64)
        d = ImageDraw.Draw(img)
        if name == "speaker":
            rect(d, (16, 8, 48, 58), PAL["dark"], PAL["outline"], 3)
            ellipse(d, (23, 15, 41, 33), PAL["black"], PAL["cyan"], 2)
            ellipse(d, (20, 34, 44, 58), PAL["black"], PAL["magenta"], 2)
        elif name == "cable":
            d.arc((5, 20, 60, 56), 180, 360, PAL["cyan"], 4)
            d.arc((12, 7, 48, 46), 20, 240, PAL["magenta"], 3)
        elif name == "mic_stand":
            rect(d, (29, 18, 33, 56), PAL["white"], PAL["outline"], 1)
            rect(d, (19, 55, 43, 59), PAL["white"], PAL["outline"], 1)
            rect(d, (24, 10, 42, 18), PAL["cyan"], PAL["outline"], 1)
        elif name == "light_rig":
            rect(d, (10, 14, 54, 20), PAL["gray"], PAL["outline"], 1)
            for x, c in [(15, PAL["magenta"]), (29, PAL["cyan"]), (43, PAL["yellow"])]:
                ellipse(d, (x, 22, x + 9, 35), c, PAL["outline"], 1)
                line(d, [(x + 4, 35), (x - 3, 57)], (*c[:3], 120), 4)
        elif name == "poster":
            rect(d, (13, 8, 51, 56), rgba("#30273d"), PAL["outline"], 2)
            spark(d, 32, 28, PAL["magenta"], 15)
            note(d, 19, 35, PAL["cyan"])
        elif name == "drum_kit":
            ellipse(d, (20, 26, 44, 50), PAL["red"], PAL["outline"], 2)
            ellipse(d, (8, 17, 25, 31), PAL["cyan"], PAL["outline"], 1)
            ellipse(d, (39, 17, 56, 31), PAL["yellow"], PAL["outline"], 1)
        elif name == "dj_table":
            rect(d, (8, 25, 56, 50), PAL["dark"], PAL["outline"], 2)
            ellipse(d, (14, 29, 30, 45), PAL["black"], PAL["magenta"], 2)
            ellipse(d, (35, 29, 51, 45), PAL["black"], PAL["cyan"], 2)
        else:
            rect(d, (10, 20, 54, 48), PAL["dark"], PAL["outline"], 2)
            line(d, [(13, 24), (51, 44)], PAL["magenta"], 3)
            line(d, [(13, 44), (51, 24)], PAL["cyan"], 3)
        save(img, "decor", name)


def draw_ui():
    for name, color in [("ui_panel", PAL["purple"]), ("ui_button", PAL["cyan"]), ("ui_rhythm_bar", PAL["orange"]), ("ui_bd_card", PAL["magenta"]), ("ui_portrait_frame", PAL["yellow"]), ("ui_danger_frame", PAL["red"]), ("ui_bubble", PAL["white"])]:
        img = image(64)
        d = ImageDraw.Draw(img)
        fill = rgba("#191923", 230) if name != "ui_bubble" else rgba("#f7f2e8", 235)
        rect(d, (3, 3, 60, 60), fill, PAL["outline"], 3)
        rect(d, (7, 7, 56, 56), (0, 0, 0, 0), color, 2)
        for x in [13, 31, 49]:
            rect(d, (x, 53, x + 5, 56), color)
        save(img, "ui", name)


def draw_vfx():
    for name, color in [("vfx_spawn_smoke", PAL["cyan"]), ("vfx_death_noise", PAL["magenta"]), ("vfx_hit_spark", PAL["yellow"]), ("vfx_sonic_ring", PAL["purple"]), ("vfx_warning_ring", PAL["red"])]:
        img = image(96)
        d = ImageDraw.Draw(img)
        for r in [12, 24, 36]:
            ellipse(d, (48 - r, 48 - r, 48 + r, 48 + r), (0, 0, 0, 0), (*color[:3], 220 - r * 3), 3)
        spark(d, 48, 48, color, 32)
        save(img, "vfx", name)


def main():
    draw_hero()
    monsters = [
        ("monster_noise_blob", 96, (rgba("#35303b"), PAL["magenta"], PAL["cyan"]), "blob"),
        ("monster_venom_singer", 96, (PAL["green"], PAL["toxic"], PAL["purple"]), "bubble"),
        ("monster_cassette_thrower", 96, (rgba("#8f5c36"), PAL["yellow"], PAL["orange"]), "cassette"),
        ("monster_speaker_brute", 128, (rgba("#313443"), PAL["magenta"], PAL["cyan"]), "speaker"),
        ("monster_metronome_wizard", 128, (PAL["purple"], PAL["magenta"], PAL["cyan"]), "wizard"),
        ("monster_tuning_fork_breaker", 128, (rgba("#22394a"), PAL["cyan"], PAL["white"]), "fork"),
        ("monster_cable_assassin", 96, (PAL["black"], PAL["magenta"], PAL["cyan"]), "assassin"),
        ("monster_distortion_king", 192, (rgba("#27202e"), PAL["magenta"], PAL["cyan"]), "boss"),
        ("monster_drumstick_imp", 96, (rgba("#7b3030"), PAL["yellow"], PAL["orange"]), "blob"),
        ("monster_bomb_kid", 96, (rgba("#422222"), PAL["red"], PAL["yellow"]), "speaker"),
        ("monster_subwoofer_elite", 160, (rgba("#252535"), PAL["orange"], PAL["magenta"]), "speaker"),
        ("monster_vinyl_witch", 128, (PAL["purple"], PAL["cyan"], PAL["magenta"]), "wizard"),
        ("monster_lighting_executor", 128, (rgba("#2b2f36"), PAL["yellow"], PAL["cyan"]), "fork"),
        ("monster_ultimate_dj_tyrant", 192, (rgba("#20202a"), PAL["yellow"], PAL["magenta"]), "boss"),
        ("monster_bad_beat_puppet", 192, (rgba("#42303f"), PAL["cyan"], PAL["yellow"]), "boss"),
    ]
    for item in monsters:
        draw_monster(*item)
    draw_projectiles()
    draw_tiles()
    draw_decor()
    draw_ui()
    draw_vfx()

    skill_icons = [
        ("skill_lightning", PAL["cyan"], "bolt"),
        ("skill_frost_field", PAL["ice"], "ice"),
        ("skill_anti_heal", PAL["green"], "cross"),
        ("skill_shield_brand", PAL["cyan"], "shield"),
        ("skill_bone_wall", rgba("#d5d0aa"), "wall"),
        ("skill_demon_hand", PAL["red"], "hand"),
        ("skill_poison_circle", PAL["toxic"], "poison"),
        ("skill_fire_burst", PAL["orange"], "fire"),
        ("skill_spikes", PAL["white"], "danger"),
        ("skill_sonic_lock", PAL["magenta"], "note"),
        ("skill_monster_buff", PAL["red"], "spark"),
        ("skill_boss_summon", PAL["purple"], "hand"),
    ]
    for name, color, symbol in skill_icons:
        draw_icon("icons", name, color, symbol)

    buff_icons = [
        ("buff_freeze", PAL["ice"], "ice"),
        ("buff_poison", PAL["toxic"], "poison"),
        ("buff_burn", PAL["orange"], "fire"),
        ("buff_slow", PAL["cyan"], "danger"),
        ("buff_stun", PAL["yellow"], "spark"),
        ("buff_anti_heal", PAL["red"], "cross"),
        ("buff_shield_break", PAL["cyan"], "shield"),
        ("buff_vulnerable", PAL["magenta"], "danger"),
        ("buff_shield", PAL["cyan"], "shield"),
        ("buff_berserk", PAL["red"], "spark"),
    ]
    for name, color, symbol in buff_icons:
        draw_icon("icons", name, color, symbol)

    element_icons = [
        ("element_fire", PAL["orange"], "fire"),
        ("element_ice", PAL["ice"], "ice"),
        ("element_lightning", PAL["cyan"], "bolt"),
        ("element_poison", PAL["toxic"], "poison"),
        ("element_sonic", PAL["magenta"], "note"),
        ("resource_energy", PAL["yellow"], "spark"),
        ("danger_warning", PAL["red"], "danger"),
        ("track_low", PAL["orange"], "note"),
        ("track_mid", PAL["purple"], "note"),
        ("track_high", PAL["cyan"], "note"),
    ]
    for name, color, symbol in element_icons:
        draw_icon("icons", name, color, symbol)


if __name__ == "__main__":
    main()
