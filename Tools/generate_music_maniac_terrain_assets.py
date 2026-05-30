from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "ReverseSurvivorArt"
TILES = OUT / "tiles"
DECOR = OUT / "decor"


def rgba(hex_color, alpha=255):
    hex_color = hex_color.lstrip("#")
    return tuple(int(hex_color[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


PAL = {
    "ink": rgba("#070910"),
    "line": rgba("#222638"),
    "floor": rgba("#151722"),
    "floor2": rgba("#1d2030"),
    "steel": rgba("#43495a"),
    "steel2": rgba("#626a7f"),
    "cyan": rgba("#28d7ff"),
    "pink": rgba("#ff2b9b"),
    "purple": rgba("#8748ff"),
    "orange": rgba("#ff8a2a"),
    "yellow": rgba("#ffd94f"),
    "green": rgba("#68ff73"),
    "red": rgba("#ff3856"),
    "white": rgba("#f2f0ff"),
}


def save(img, folder, name):
    path = folder / f"{name}.png"
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)


def add_noise_rects(d, seed, count=9):
    for i in range(count):
        x = (seed * 17 + i * 23) % 58
        y = (seed * 31 + i * 19) % 58
        c = (28 + (i * 7) % 34, 30 + (seed * 5 + i * 3) % 28, 45 + (i * 11) % 30, 70)
        d.rectangle((x, y, min(63, x + 2 + i % 4), min(63, y + 1 + seed % 3)), fill=c)


def draw_floor_tile(name, base, accent, mode, seed):
    img = Image.new("RGBA", (64, 64), base)
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, 63, 63), outline=(9, 10, 15, 255))
    for x in (0, 32, 63):
        d.line((x, 0, x, 63), fill=(*PAL["line"][:3], 120), width=1)
    for y in (0, 32, 63):
        d.line((0, y, 63, y), fill=(*PAL["line"][:3], 120), width=1)
    add_noise_rects(d, seed)
    if mode == "stage":
        d.line((6, 10, 58, 10), fill=(*accent[:3], 100), width=2)
        d.line((8, 54, 56, 54), fill=(*accent[:3], 80), width=2)
        for x in (14, 31, 48):
            d.rectangle((x, 20, x + 3, 44), fill=(*accent[:3], 58))
    elif mode == "wave":
        for y in (14, 27, 41, 53):
            points = []
            for x in range(-4, 69, 8):
                points.append((x, y + ((x + seed) % 16 - 8) // 4))
            d.line(points, fill=(*accent[:3], 120), width=2)
    elif mode == "brick":
        for y in (16, 32, 48):
            d.line((0, y, 63, y), fill=(8, 9, 14, 185), width=2)
        for row, off in enumerate((0, 16, 0, 16)):
            y0 = row * 16
            for x in range(-off, 64, 32):
                d.line((x, y0, x, y0 + 16), fill=(8, 9, 14, 160), width=2)
        d.line((9, 51, 55, 14), fill=(*accent[:3], 80), width=1)
    elif mode == "cable":
        d.arc((5, 18, 59, 73), 196, 344, fill=(*accent[:3], 120), width=3)
        d.arc((17, -8, 53, 44), 15, 240, fill=(*PAL["pink"][:3], 95), width=2)
        d.rectangle((8, 8, 18, 13), fill=(*accent[:3], 160))
    save(img, TILES, name)


def draw_edge_tile(name, side):
    img = Image.new("RGBA", (64, 64), rgba("#11131c"))
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, 63, 63), fill=rgba("#151722"))
    add_noise_rects(d, len(name), 6)
    metal = rgba("#333846")
    light = PAL["cyan"] if side in ("top", "bottom") else PAL["pink"]
    if side == "top":
        d.rectangle((0, 0, 63, 25), fill=metal)
        d.rectangle((0, 26, 63, 31), fill=PAL["ink"])
        d.line((0, 34, 63, 34), fill=light, width=3)
    elif side == "bottom":
        d.rectangle((0, 38, 63, 63), fill=metal)
        d.rectangle((0, 32, 63, 37), fill=PAL["ink"])
        d.line((0, 29, 63, 29), fill=light, width=3)
    elif side == "left":
        d.rectangle((0, 0, 25, 63), fill=metal)
        d.rectangle((26, 0, 31, 63), fill=PAL["ink"])
        d.line((34, 0, 34, 63), fill=light, width=3)
    elif side == "right":
        d.rectangle((38, 0, 63, 63), fill=metal)
        d.rectangle((32, 0, 37, 63), fill=PAL["ink"])
        d.line((29, 0, 29, 63), fill=light, width=3)
    for p in range(10, 63, 18):
        if side in ("top", "bottom"):
            d.rectangle((p, 8 if side == "top" else 48, p + 7, 15 if side == "top" else 55), fill=PAL["yellow"])
        else:
            d.rectangle((8 if side == "left" else 48, p, 15 if side == "left" else 55, p + 7), fill=PAL["yellow"])
    save(img, TILES, name)


def draw_corner_tile(name, sx, sy):
    img = Image.new("RGBA", (64, 64), rgba("#151722"))
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, 63, 63), fill=rgba("#12151e"))
    x0, x1 = (0, 31) if sx < 0 else (32, 63)
    y0, y1 = (0, 31) if sy > 0 else (32, 63)
    d.rectangle((x0, 0, x1, 63), fill=rgba("#333846"))
    d.rectangle((0, y0, 63, y1), fill=rgba("#3c4150"))
    d.line((32, 0, 32, 63), fill=PAL["ink"], width=3)
    d.line((0, 32, 63, 32), fill=PAL["ink"], width=3)
    d.ellipse((18, 18, 46, 46), fill=rgba("#10131c"), outline=PAL["orange"], width=3)
    d.ellipse((27, 27, 37, 37), fill=PAL["yellow"])
    save(img, TILES, name)


def glow(size, draw_fn, blur=4):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(img))
    return img.filter(ImageFilter.GaussianBlur(blur))


def decor_canvas(size=128):
    return Image.new("RGBA", (size, size), (0, 0, 0, 0))


def draw_speaker():
    img = decor_canvas()
    img.alpha_composite(glow((128, 128), lambda d: d.rectangle((29, 10, 99, 118), outline=PAL["cyan"], width=8), 6))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((28, 10, 100, 118), radius=5, fill=rgba("#222632"), outline=PAL["ink"], width=5)
    d.ellipse((44, 23, 84, 63), fill=rgba("#080910"), outline=PAL["cyan"], width=4)
    d.ellipse((36, 66, 92, 112), fill=rgba("#080910"), outline=PAL["pink"], width=5)
    d.ellipse((57, 83, 71, 97), fill=PAL["ink"], outline=PAL["purple"], width=3)
    save(img, DECOR, "speaker")


def draw_crate_like(name, accent):
    img = decor_canvas()
    d = ImageDraw.Draw(img)
    img.alpha_composite(glow((128, 128), lambda gd: gd.rounded_rectangle((20, 30, 108, 106), radius=6, outline=accent, width=7), 5))
    d.rounded_rectangle((18, 28, 110, 108), radius=6, fill=rgba("#2a2630"), outline=PAL["ink"], width=5)
    d.rectangle((26, 37, 102, 48), fill=(*accent[:3], 145))
    d.line((30, 54, 98, 98), fill=(*accent[:3], 180), width=5)
    d.line((98, 54, 30, 98), fill=(*accent[:3], 150), width=5)
    d.rectangle((25, 82, 103, 92), fill=rgba("#12141c"))
    save(img, DECOR, name)


def draw_barrier():
    img = decor_canvas(160)
    d = ImageDraw.Draw(img)
    img.alpha_composite(glow((160, 160), lambda gd: gd.rounded_rectangle((14, 53, 146, 105), radius=7, outline=PAL["orange"], width=8), 6))
    d.rounded_rectangle((12, 50, 148, 108), radius=7, fill=rgba("#292c35"), outline=PAL["ink"], width=5)
    for x in range(22, 140, 28):
        d.polygon([(x, 54), (x + 18, 54), (x + 4, 104), (x - 14, 104)], fill=PAL["orange"], outline=PAL["ink"])
    d.rectangle((20, 43, 34, 116), fill=rgba("#404655"), outline=PAL["ink"])
    d.rectangle((126, 43, 140, 116), fill=rgba("#404655"), outline=PAL["ink"])
    save(img, DECOR, "barrier")


def draw_dj_table():
    img = decor_canvas(160)
    d = ImageDraw.Draw(img)
    img.alpha_composite(glow((160, 160), lambda gd: gd.rounded_rectangle((18, 54, 142, 116), radius=8, outline=PAL["pink"], width=7), 5))
    d.rounded_rectangle((16, 52, 144, 118), radius=8, fill=rgba("#242735"), outline=PAL["ink"], width=5)
    d.ellipse((32, 64, 70, 102), fill=rgba("#080910"), outline=PAL["pink"], width=4)
    d.ellipse((90, 64, 128, 102), fill=rgba("#080910"), outline=PAL["cyan"], width=4)
    for x in (74, 80, 86):
        d.rectangle((x, 62, x + 3, 108), fill=PAL["yellow"])
    save(img, DECOR, "dj_table")


def draw_light_rig():
    img = decor_canvas(160)
    d = ImageDraw.Draw(img)
    d.rectangle((20, 24, 140, 38), fill=rgba("#454b5a"), outline=PAL["ink"], width=3)
    for x, c in ((38, PAL["pink"]), (75, PAL["cyan"]), (112, PAL["yellow"])):
        img.alpha_composite(glow((160, 160), lambda gd, xx=x, cc=c: gd.polygon([(xx, 52), (xx - 22, 142), (xx + 22, 142)], fill=(*cc[:3], 60)), 4))
        d.rounded_rectangle((x - 12, 40, x + 12, 62), radius=4, fill=rgba("#242735"), outline=PAL["ink"], width=2)
        d.ellipse((x - 8, 47, x + 8, 63), fill=c, outline=PAL["ink"], width=2)
    save(img, DECOR, "light_rig")


def draw_small_decor():
    draw_speaker()
    draw_crate_like("crate", PAL["cyan"])
    draw_crate_like("neon_sign", PAL["pink"])
    draw_crate_like("road_sign", PAL["yellow"])
    draw_crate_like("poster", PAL["purple"])
    draw_crate_like("cable", PAL["cyan"])
    draw_crate_like("mic_stand", PAL["white"])
    draw_crate_like("drum_kit", PAL["red"])
    draw_barrier()
    draw_dj_table()
    draw_light_rig()


def main():
    floor_specs = [
        ("tile_stage_floor", rgba("#161923"), PAL["cyan"], "stage"),
        ("tile_stage_floor_a", rgba("#181a25"), PAL["pink"], "stage"),
        ("tile_stage_floor_b", rgba("#141923"), PAL["orange"], "wave"),
        ("tile_stage_floor_c", rgba("#1b1628"), PAL["purple"], "cable"),
        ("tile_dark_brick", rgba("#171925"), PAL["purple"], "brick"),
        ("tile_asphalt_wave", rgba("#151721"), PAL["pink"], "wave"),
        ("tile_soundwave_floor", rgba("#18142a"), PAL["cyan"], "wave"),
        ("tile_concrete_roof", rgba("#202633"), PAL["cyan"], "stage"),
        ("tile_wood_floor", rgba("#25202a"), PAL["orange"], "stage"),
    ]
    for i, (name, base, accent, mode) in enumerate(floor_specs):
        draw_floor_tile(name, base, accent, mode, i + 3)
    draw_edge_tile("tile_border_top", "top")
    draw_edge_tile("tile_border_bottom", "bottom")
    draw_edge_tile("tile_border_left", "left")
    draw_edge_tile("tile_border_right", "right")
    draw_corner_tile("tile_border_corner_tl", -1, 1)
    draw_corner_tile("tile_border_corner_tr", 1, 1)
    draw_corner_tile("tile_border_corner_bl", -1, -1)
    draw_corner_tile("tile_border_corner_br", 1, -1)
    draw_small_decor()
    print("Generated block terrain, border, and obstacle art.")


if __name__ == "__main__":
    main()
