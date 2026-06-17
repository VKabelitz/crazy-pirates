#!/usr/bin/env python3
"""Generate CV-Tag preview image and fast-forward slide for
'Dieses andere Adventure' (Team Richards Haus)."""
import os
import base64
import io
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageOps, ImageChops

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GFX  = os.path.join(ROOT, "Assets", "TextMesh Pro", "Graphics")
CV   = os.path.join(ROOT, "cvtag")

SHOT_ROW  = os.path.join(CV, "Screenshot from 2026-06-17 09-40-23.png")  # row of helitoads
SHOT_FIRE = os.path.join(CV, "Screenshot from 2026-06-17 09-38-13.png")  # towers firing
TOAD      = os.path.join(GFX, "Toad2.png")    # heli-toad with propeller
TOAD_FLAT = os.path.join(GFX, "toadd.png")    # plain toad
GEAR      = os.path.join(GFX, "menuArt.png")  # steampunk gear corner
HINTER    = os.path.join(GFX, "Hintergrund.png")  # painted sky backdrop
RENDER    = os.path.join(CV,  "media", "Toad.png")  # hand-drawn HeliToad mascot
CARDS     = [os.path.join(GFX, n) for n in ("canon1.png", "canon2.png", "canon3.png")]  # tower cards
# Blender renders of the game models (transparent PNGs in media/)
MODELS    = {n: os.path.join(CV, "media", n + ".png")
             for n in ("floattoad", "helitoad", "iceball", "cannon", "crossbow", "tesla")}

# ---- palette (warm steampunk) ----
CREAM   = (244, 230, 200)
PARCH   = (236, 216, 178)
COPPER  = (196, 110, 52)
COPPER2 = (150, 78, 34)
DARK    = (38, 24, 14)
DARK2   = (24, 15, 8)
TOADGRN = (122, 158, 78)

FS = "/usr/share/fonts/truetype/freefont/FreeSerifBold.ttf"
LB = "/usr/share/fonts/truetype/lato/Lato-Bold.ttf"
LK = "/usr/share/fonts/truetype/lato/Lato-Black.ttf"
LR = "/usr/share/fonts/truetype/lato/Lato-Regular.ttf"

def font(path, size): return ImageFont.truetype(path, size)

def cover(img, w, h):
    """Scale + center-crop to exactly w x h."""
    img = img.convert("RGBA")
    s = max(w / img.width, h / img.height)
    img = img.resize((round(img.width * s), round(img.height * s)), Image.LANCZOS)
    x = (img.width - w) // 2
    y = (img.height - h) // 2
    return img.crop((x, y, x + w, y + h))

def vgrad(w, h, top, bot):
    base = Image.new("RGB", (1, h))
    for y in range(h):
        t = y / max(1, h - 1)
        base.putpixel((0, y), tuple(round(top[i] + (bot[i] - top[i]) * t) for i in range(3)))
    return base.resize((w, h)).convert("RGBA")

def drop_shadow(rgba, blur=18, alpha=150, grow=6):
    a = rgba.split()[3]
    sh = Image.new("RGBA", rgba.size, (0, 0, 0, 0))
    solid = Image.new("RGBA", rgba.size, (0, 0, 0, alpha))
    sh.paste(solid, (0, 0), a.point(lambda p: 255 if p > 20 else 0))
    if grow:
        sh.putalpha(sh.split()[3].filter(ImageFilter.MaxFilter(grow * 2 + 1)))
    return sh.filter(ImageFilter.GaussianBlur(blur))

def paste_with_shadow(canvas, rgba, x, y, blur=18, alpha=150, dx=10, dy=14, grow=4):
    sh = drop_shadow(rgba, blur, alpha, grow)
    canvas.alpha_composite(sh, (x + dx, y + dy))
    canvas.alpha_composite(rgba, (x, y))

def text(draw, xy, s, fnt, fill, anchor="la", outline=None, ow=0, shadow=None, sh_off=(2, 3)):
    x, y = xy
    if shadow:
        draw.text((x + sh_off[0], y + sh_off[1]), s, font=fnt, fill=shadow, anchor=anchor)
    if outline and ow:
        for ox in range(-ow, ow + 1):
            for oy in range(-ow, ow + 1):
                if ox or oy:
                    draw.text((x + ox, y + oy), s, font=fnt, fill=outline, anchor=anchor)
    draw.text((x, y), s, font=fnt, fill=fill, anchor=anchor)

def fit_toad(scale_h):
    t = Image.open(TOAD).convert("RGBA")
    s = scale_h / t.height
    return t.resize((round(t.width * s), round(t.height * s)), Image.LANCZOS)

def fit_h(img, scale_h):
    """Resize an RGBA image to a target height, preserving aspect."""
    s = scale_h / img.height
    return img.resize((round(img.width * s), round(img.height * s)), Image.LANCZOS)

def trim(img):
    """Autocrop transparent margins."""
    img = img.convert("RGBA")
    bb = img.split()[3].getbbox()
    return img.crop(bb) if bb else img

def defringe(img, thr=252, radius=12):
    """Kill the dark matte halo on the Blender renders.

    Their antialiased edge pixels were rendered over a black background, so
    semi-transparent edges carry near-black colour and show up as a black
    fringe when composited over a bright backdrop. We bleed the solid object
    colour outward (alpha-weighted blur) and replace the colour of every
    not-fully-opaque pixel with it, keeping the original alpha so the silhouette
    and antialiasing are untouched."""
    import numpy as np
    img = img.convert("RGBA")
    arr = np.asarray(img, dtype=np.float32)
    rgb, a = arr[..., :3], arr[..., 3]
    af = a / 255.0  # opacity weight: dark transparent edges contribute little

    blur = ImageFilter.GaussianBlur(radius)
    # blurred alpha-weighted colour (premultiplied) and blurred alpha
    pm = (rgb * af[..., None]).clip(0, 255).astype(np.uint8)
    pm_b = np.asarray(Image.fromarray(pm, "RGB").filter(blur), dtype=np.float32)
    a_b = np.asarray(Image.fromarray(a.astype(np.uint8), "L").filter(blur),
                     dtype=np.float32) / 255.0

    safe = a_b > 1e-3
    bled = np.where(safe[..., None], pm_b / np.where(safe, a_b, 1.0)[..., None], rgb)

    edge = (a < thr)[..., None]
    out_rgb = np.where(edge, bled, rgb)
    out = np.dstack([np.clip(out_rgb, 0, 255), a]).astype(np.uint8)
    return Image.fromarray(out, "RGBA")

def load_render(scale_h):
    return fit_h(trim(Image.open(RENDER)), scale_h)

def model_img(name, h, rot=0):
    """Trimmed Blender render of a game model, sized to height h, optionally rotated."""
    img = fit_h(defringe(trim(Image.open(MODELS[name]))), h)
    if rot:
        img = img.rotate(rot, expand=True, resample=Image.BICUBIC)
    return img

def paste_centered(canvas, img, cx, cy, **kw):
    paste_with_shadow(canvas, img, cx - img.width // 2, cy - img.height // 2, **kw)

def tower_card(scale_h):
    """Load a tower card cropped tight to its wooden frame, at target height."""
    return [fit_h(trim(Image.open(p)), scale_h) for p in CARDS]

def rounded_panel(w, h, radius, fill, border=None, bw=0):
    p = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(p)
    d.rounded_rectangle([0, 0, w - 1, h - 1], radius=radius, fill=fill,
                        outline=border, width=bw)
    return p

# ============================================================ PREVIEW 1280x720
def build_preview():
    W, H = 1280, 720
    canvas = cover(Image.open(SHOT_ROW), W, H)

    # warm color grade + vignette
    overlay = Image.new("RGBA", (W, H), (60, 30, 10, 40))
    canvas.alpha_composite(overlay)
    # left + bottom darkening for text legibility
    left = vgrad(W, H, (0, 0, 0), (0, 0, 0))
    lmask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(lmask)
    for x in range(W):
        md.line([(x, 0), (x, H)], fill=max(0, int(190 * (1 - x / (W * 0.62)))))
    canvas.paste((0, 0, 0), (0, 0), lmask)
    bot = Image.new("L", (W, H), 0)
    bd = ImageDraw.Draw(bot)
    for y in range(H):
        bd.line([(0, y), (W, y)], fill=max(0, int(200 * ((y - H * 0.45) / (H * 0.55)))) if y > H * 0.45 else 0)
    canvas.paste((0, 0, 0), (0, 0), bot)

    # hand-drawn HeliToad mascot, bottom-right (fully visible)
    toad = load_render(620).rotate(-6, expand=True, resample=Image.BICUBIC)
    paste_with_shadow(canvas, toad, W - toad.width + 18, H - toad.height + 10,
                      blur=24, alpha=180, dx=14, dy=16)

    # tower-card trio, bottom-left (the arsenal, shown not told)
    cards = tower_card(132)
    cw = cards[0].width
    cxs, cys = 60, H - cards[0].height - 26
    for i, card in enumerate(cards):
        paste_with_shadow(canvas, card, cxs + i * (cw + 18), cys,
                          blur=12, alpha=140, dx=6, dy=8, grow=3)

    d = ImageDraw.Draw(canvas)
    # title
    text(d, (60, 300), "Das etwas andere", font(FS, 78), CREAM,
         outline=DARK2, ow=3, shadow=(0, 0, 0))
    text(d, (60, 378), "Adventure", font(FS, 78), COPPER,
         outline=DARK2, ow=3, shadow=(0, 0, 0))

    out = os.path.join(CV, "preview", "preview.png")
    canvas.convert("RGB").save(out, "PNG")
    print("wrote", out, canvas.size)

# ====================================================== FAST FORWARD 1920x1080
def vignette(w, h, edge=170, top=150, bot=150):
    """Dark mask: strong at top + bottom + corners, clear in the middle.
    Keeps a painted backdrop legible behind cream text and renders."""
    m = Image.new("L", (w, h), 0)
    d = ImageDraw.Draw(m)
    for y in range(h):
        t = y / max(1, h - 1)
        v = 0
        if t < 0.34:                       # darken the top band (title)
            v = max(v, int(top * (1 - t / 0.34)))
        if t > 0.62:                       # darken the bottom (grounding)
            v = max(v, int(bot * ((t - 0.62) / 0.38)))
        d.line([(0, y), (w, y)], fill=v)
    # side darkening for the gear corners + cast
    side = Image.new("L", (w, h), 0)
    sd = ImageDraw.Draw(side)
    for x in range(w):
        t = x / max(1, w - 1)
        v = 0
        if t < 0.22:
            v = int(edge * (1 - t / 0.22))
        elif t > 0.78:
            v = int(edge * ((t - 0.78) / 0.22))
        sd.line([(x, 0), (x, h)], fill=v)
    return ImageChops.lighter(m, side)

def build_fast_forward():
    W, H = 1920, 1080
    # painted sky backdrop (fits the floating pirate ship), warmed + vignetted
    bg = cover(Image.open(HINTER), W, H)
    bg.alpha_composite(Image.new("RGBA", (W, H), (40, 22, 8, 70)))   # warm wash
    dark = Image.new("RGBA", (W, H), (10, 6, 3, 255))
    dark.putalpha(vignette(W, H))
    bg.alpha_composite(dark)
    canvas = bg

    d = ImageDraw.Draw(canvas)

    # ---- centered gameplay screenshot (the hero in the middle) ----
    sw, sh = 1040, 585
    shot = cover(Image.open(SHOT_FIRE), sw, sh)
    frame = rounded_panel(sw + 24, sh + 24, 28, (44, 27, 15, 255), border=COPPER, bw=6)
    inner = rounded_panel(sw, sh, 18, (0, 0, 0, 255))
    mask = inner.split()[3]
    fx, fy = (W - sw) // 2, 268
    paste_with_shadow(canvas, frame, fx - 12, fy - 12, blur=28, alpha=170, dx=0, dy=20)
    shot_r = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    shot_r.paste(shot, (0, 0), mask)
    canvas.alpha_composite(shot_r, (fx, fy))

    # ---- the cast, spread around the screenshot (3D Blender renders) ----
    # (name, height, rotation, center-x, center-y)
    spread = [
        ("helitoad",  322,  -7,  256,  352),   # top-left    enemy
        ("floattoad", 332,   7,  220,  662),   # mid-left    enemy
        ("cannon",    300,  -4,  268,  922),   # bottom-left tower
        ("iceball",   318,   6, 1664,  350),   # top-right   enemy
        ("tesla",     366,   0, 1706,  668),   # mid-right   tower
        ("crossbow",  300,   5, 1664,  922),   # bottom-right tower
    ]
    for name, h, rot, cx, cy in spread:
        paste_centered(canvas, model_img(name, h, rot), cx, cy,
                       blur=26, alpha=175, dx=12, dy=16, grow=4)

    # ---- the only text: the title ----
    text(d, (W // 2, 46), "Das etwas andere Adventure", font(FS, 100), CREAM,
         anchor="ma", outline=DARK2, ow=4, shadow=(0, 0, 0))

    # The CV-Tag fast-forward slide must be delivered as .svg. The artwork is
    # built from photographic screenshots, painted backdrop and 3D renders, so
    # the composed frame is embedded losslessly as a single image inside an SVG
    # canvas (valid SVG, scales cleanly, imports into PowerPoint).
    rgb = canvas.convert("RGB")
    buf = io.BytesIO()
    rgb.save(buf, "PNG")
    b64 = base64.b64encode(buf.getvalue()).decode("ascii")
    svg = (
        f'<?xml version="1.0" encoding="UTF-8"?>\n'
        f'<svg xmlns="http://www.w3.org/2000/svg" '
        f'xmlns:xlink="http://www.w3.org/1999/xlink" '
        f'width="{W}" height="{H}" viewBox="0 0 {W} {H}">\n'
        f'  <title>Dieses andere Adventure – Fast Forward</title>\n'
        f'  <image x="0" y="0" width="{W}" height="{H}" '
        f'xlink:href="data:image/png;base64,{b64}"/>\n'
        f'</svg>\n'
    )
    out = os.path.join(CV, "fast_forward", "fast_forward.svg")
    with open(out, "w", encoding="utf-8") as f:
        f.write(svg)
    print("wrote", out, canvas.size)

if __name__ == "__main__":
    build_preview()
    build_fast_forward()
