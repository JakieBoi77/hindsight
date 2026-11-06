#!/usr/bin/env python3
"""Generates EyeCanDo's original artwork as flat SVG.

The look follows the team's first 2D PoC (Dr. Iris and the seated child): flat shapes with a
navy ink outline, round heads and rosy cheeks. Characters are split into parts so the game can
animate them: a body per pose, a head, and shared face layers (eyes, mouth) drawn on a canvas
the same size as the head. Unity imports SVG natively (Vector Graphics module) as sprites at a
high resolution, so everything stays sharp on phones and tablets.

Unity's SVG importer does not support <text>, filters or masks, so none are used here.

Usage (from src/EyeCanDo):  python Tools/ArtGen/generate_art.py
"""

import math
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art")

# Palette ---------------------------------------------------------------------
INK = "#2E3A59"
WHITE = "#FFFFFF"
CHEEK = "#FF8E8E"
TEAL = "#2BB3A3"
TEAL_DARK = "#1F8C80"
BLUE = "#4A90E2"
BLUE_DARK = "#2F6DB5"
SKY = "#BFE6FF"
YELLOW = "#FFC845"
YELLOW_DARK = "#E0A526"
CORAL = "#FF6B6B"
PURPLE = "#9B6BE0"
GREEN = "#5CBF73"
GREEN_DARK = "#3E9C55"
PINK = "#FF9BC2"
ORANGE = "#FF9F43"
GREY = "#B8C2D3"
GREY_DARK = "#7C879C"
DROP = "#7CC8FF"
DROP_DARK = "#3E9BE0"
WOOD = "#D9A066"
WOOD_DARK = "#B07A45"
OUTLINE = 6


def svg(width, height, body):
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
        f'viewBox="0 0 {width} {height}">\n{body}\n</svg>\n'
    )


def write(rel_path, width, height, parts):
    body = "\n".join(p for p in parts if p)
    path = os.path.join(ART, rel_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(svg(width, height, body))
    WRITTEN.append(rel_path)


WRITTEN = []


def st(width=OUTLINE, color=INK):
    return f'stroke="{color}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'


def circle(cx, cy, r, fill, stroke=OUTLINE, extra=""):
    s = st(stroke) if stroke else ""
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {s} {extra}/>'


def ellipse(cx, cy, rx, ry, fill, stroke=OUTLINE, extra=""):
    s = st(stroke) if stroke else ""
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {s} {extra}/>'


def rect(x, y, w, h, fill, rx=0, stroke=OUTLINE, extra=""):
    s = st(stroke) if stroke else ""
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {s} {extra}/>'


def path(d, fill="none", stroke=OUTLINE, color=INK, extra=""):
    s = st(stroke, color) if stroke else ""
    return f'<path d="{d}" fill="{fill}" {s} {extra}/>'


def line(points, width, color=INK, opacity=None):
    pts = " ".join(f"{x},{y}" for x, y in points)
    o = f'stroke-opacity="{opacity}"' if opacity is not None else ""
    return (f'<polyline points="{pts}" fill="none" stroke="{color}" stroke-width="{width}" '
            f'stroke-linejoin="round" stroke-linecap="round" {o}/>')


def limb(points, fill, width=44):
    """A thick rounded limb: an ink stroke with a narrower coloured stroke on top."""
    return line(points, width + 2 * OUTLINE) + "\n" + line(points, width, fill)


def star_points(cx, cy, outer, inner, n=5, rotation=-90):
    pts = []
    for i in range(n * 2):
        a = math.radians(rotation + i * 180 / n)
        r = outer if i % 2 == 0 else inner
        pts.append(f"{cx + math.cos(a) * r:.1f},{cy + math.sin(a) * r:.1f}")
    return " ".join(pts)


def star(cx, cy, outer, inner, fill, stroke=OUTLINE):
    s = st(stroke) if stroke else ""
    return f'<polygon points="{star_points(cx, cy, outer, inner)}" fill="{fill}" {s}/>'


def drop_path(cx, cy, r):
    """A teardrop with its round end centred on (cx, cy) and the tip pointing up."""
    tip = cy - r * 2.1
    return (f"M{cx} {tip:.1f} C{cx + r * 0.35:.1f} {cy - r * 1.3:.1f} {cx + r:.1f} {cy - r * 0.6:.1f} "
            f"{cx + r:.1f} {cy:.1f} A{r} {r} 0 0 1 {cx - r:.1f} {cy:.1f} "
            f"C{cx - r:.1f} {cy - r * 0.6:.1f} {cx - r * 0.35:.1f} {cy - r * 1.3:.1f} {cx} {tip:.1f} Z")


def eye_shape(cx, cy, w, h):
    """An almond eye outline."""
    return (f"M{cx - w} {cy} Q{cx} {cy - h * 2} {cx + w} {cy} Q{cx} {cy + h * 2} {cx - w} {cy} Z")


# Characters ------------------------------------------------------------------
# The kids are built from recolourable parts in avatar_parts.py. Dr. Iris is drawn here.
def face_layers(prefix, size, eyes, eye_r, mouth_y, mouth_w, brow_y):
    """Shared face layers on a head-sized canvas: eyes (open, up, closed, happy, squeeze) and mouths."""
    (lx, ly), (rx, ry) = eyes
    ew, eh = eye_r, eye_r * 1.15

    def eye_pair(pupil_dy, brows="calm"):
        parts = []
        for x, y in ((lx, ly), (rx, ry)):
            parts.append(ellipse(x, y, ew, eh, WHITE, 5))
            parts.append(circle(x, y + pupil_dy, eye_r * 0.62, INK, 0))
            parts.append(circle(x + eye_r * 0.22, y + pupil_dy - eye_r * 0.25, eye_r * 0.22, WHITE, 0))
        parts.extend(brow_pair(brows))
        return parts

    def brow_pair(kind):
        out = []
        for x, side in ((lx, -1), (rx, 1)):
            if kind == "worried":
                out.append(path(f"M{x - ew} {brow_y + side * 0 - 2} Q{x} {brow_y - 10 + (6 if side < 0 else -6)} {x + ew} {brow_y + (8 if side < 0 else -12)}", "none", 5))
            elif kind == "up":
                out.append(path(f"M{x - ew} {brow_y - 6} Q{x} {brow_y - 22} {x + ew} {brow_y - 6}", "none", 5))
            else:
                out.append(path(f"M{x - ew} {brow_y} Q{x} {brow_y - 12} {x + ew} {brow_y}", "none", 5))
        return out

    write(f"Characters/{prefix}_eyes_open.svg", size, size, eye_pair(eye_r * 0.12))
    write(f"Characters/{prefix}_eyes_up.svg", size, size, eye_pair(-eye_r * 0.5, "up"))
    write(f"Characters/{prefix}_eyes_worried.svg", size, size, eye_pair(eye_r * 0.12, "worried"))
    write(f"Characters/{prefix}_eyes_closed.svg", size, size, [
        path(f"M{x - ew} {y} Q{x} {y + eh * 0.9} {x + ew} {y}", "none", 6) for x, y in ((lx, ly), (rx, ry))
    ] + brow_pair("calm"))
    write(f"Characters/{prefix}_eyes_happy.svg", size, size, [
        path(f"M{x - ew} {y + 4} Q{x} {y - eh * 1.1} {x + ew} {y + 4}", "none", 6) for x, y in ((lx, ly), (rx, ry))
    ] + brow_pair("up"))
    write(f"Characters/{prefix}_eyes_squeeze.svg", size, size, [
        path(f"M{lx - ew} {ly - 10} L{lx + ew * 0.7} {ly} L{lx - ew} {ly + 10}", "none", 6),
        path(f"M{rx + ew} {ry - 10} L{rx - ew * 0.7} {ry} L{rx + ew} {ry + 10}", "none", 6),
    ] + brow_pair("worried"))

    mx, my, mw = (lx + rx) / 2, mouth_y, mouth_w
    write(f"Characters/{prefix}_mouth_smile.svg", size, size, [
        path(f"M{mx - mw} {my} Q{mx} {my + mw * 0.9} {mx + mw} {my}", "none", 6)])
    write(f"Characters/{prefix}_mouth_talk.svg", size, size, [
        path(f"M{mx - mw * 0.7} {my} Q{mx} {my - 4} {mx + mw * 0.7} {my} Q{mx} {my + mw * 1.1} {mx - mw * 0.7} {my} Z", INK, 5),
        ellipse(mx, my + mw * 0.45, mw * 0.36, mw * 0.18, CORAL, 0)])
    write(f"Characters/{prefix}_mouth_grin.svg", size, size, [
        path(f"M{mx - mw * 1.1} {my - 4} Q{mx} {my - 2} {mx + mw * 1.1} {my - 4} Q{mx} {my + mw * 1.5} {mx - mw * 1.1} {my - 4} Z", INK, 5),
        path(f"M{mx - mw * 0.55} {my + mw * 0.75} Q{mx} {my + mw * 0.35} {mx + mw * 0.55} {my + mw * 0.75} Q{mx} {my + mw * 1.05} {mx - mw * 0.55} {my + mw * 0.75} Z", CORAL, 0)])
    write(f"Characters/{prefix}_mouth_oh.svg", size, size, [ellipse(mx, my + 6, mw * 0.35, mw * 0.45, INK, 5)])
    write(f"Characters/{prefix}_mouth_wobble.svg", size, size, [
        path(f"M{mx - mw} {my + 6} Q{mx - mw * 0.66} {my - 4} {mx - mw * 0.33} {my + 6} Q{mx} {my + 16} {mx + mw * 0.33} {my + 6} "
             f"Q{mx + mw * 0.66} {my - 4} {mx + mw} {my + 6}", "none", 6)])


# Doctor: head canvas 300x300 (face centre 150,160). The body is drawn on a 600x780 layout and
# shifted down 60 px on a 600x840 canvas, so raised hands are never clipped (neck at 300, 200).
DOC_HEAD = 300
DOC_SKIN = "#8D5A3B"
DOC_HAIR = "#2B1B17"
DOC_BODY = (600, 840)
DOC_SHIFT = 60


def doctor_head():
    cx, cy, r = 150, 160, 98
    write("Characters/doctor_head.svg", DOC_HEAD, DOC_HEAD, [
        circle(cx, cy - 104, 44, DOC_HAIR),
        circle(cx - r + 4, cy + 10, 18, DOC_SKIN),
        circle(cx + r - 4, cy + 10, 18, DOC_SKIN),
        circle(cx, cy, r, DOC_SKIN),
        path(f"M{cx - 95} {cy - 4} Q{cx - 100} {cy - 98} {cx} {cy - 98} Q{cx + 100} {cy - 98} {cx + 95} {cy - 4} "
             f"Q{cx + 70} {cy - 54} {cx} {cy - 52} Q{cx - 70} {cy - 54} {cx - 95} {cy - 4} Z", DOC_HAIR),
        ellipse(cx - 58, cy + 42, 15, 9, CHEEK, 0, 'fill-opacity="0.5"'),
        ellipse(cx + 58, cy + 42, 15, 9, CHEEK, 0, 'fill-opacity="0.5"'),
        path(f"M{cx - 7} {cy + 20} Q{cx} {cy + 27} {cx + 7} {cy + 20}", "none", 4),
    ])
    write("Characters/doctor_glasses.svg", DOC_HEAD, DOC_HEAD, [
        circle(cx - 38, cy + 4, 31, WHITE, 5, 'fill-opacity="0.12"'),
        circle(cx + 38, cy + 4, 31, WHITE, 5, 'fill-opacity="0.12"'),
        path(f"M{cx - 8} {cy} Q{cx} {cy - 8} {cx + 8} {cy}", "none", 5),
    ])


DOC_ARMS = {
    "idle": ([(222, 182), (196, 300), (196, 400)], [(378, 182), (404, 300), (404, 400)]),
    "wave": ([(222, 182), (196, 300), (196, 400)], [(378, 182), (470, 110), (486, 20)]),
    "point": ([(222, 182), (196, 300), (196, 400)], [(378, 182), (480, 170), (548, 120)]),
    "cheer": ([(222, 182), (130, 100), (112, 20)], [(378, 182), (470, 100), (488, 20)]),
    # Holds the drop bottle up and to the left, above the patient's eye.
    "hold": ([(222, 182), (120, 160), (52, 80)], [(378, 182), (404, 300), (404, 400)]),
}
# Hand centres the game uses to place props in a hand (body canvas pixels).
DOC_HANDS = {pose: ((arms[0][-1][0], arms[0][-1][1] + DOC_SHIFT), (arms[1][-1][0], arms[1][-1][1] + DOC_SHIFT)) for pose, arms in DOC_ARMS.items()}


def doctor_body(pose):
    w, h = DOC_BODY
    p = [
        rect(234, 560, 56, 170, "#3D4A6B", 20),
        rect(310, 560, 56, 170, "#3D4A6B", 20),
        ellipse(256, 738, 52, 24, INK, 0),
        ellipse(344, 738, 52, 24, INK, 0),
        rect(274, 110, 52, 70, DOC_SKIN, 10),
        # Coat over teal scrubs.
        path("M206 172 Q300 140 394 172 L438 610 Q300 640 162 610 Z", WHITE),
        path("M262 166 L300 260 L338 166 Z", TEAL, 4),
        path("M262 168 L300 300 L282 612", "none", 4),
        path("M338 168 L300 300 L318 612", "none", 4),
        # Stethoscope.
        path("M246 176 Q236 280 286 300", "none", 7, GREY_DARK),
        path("M354 176 Q364 280 314 300", "none", 7, GREY_DARK),
        line([(300, 300), (300, 352)], 7, GREY_DARK),
        circle(300, 364, 16, GREY, 5),
        # Pocket with pens and an eye name badge.
        rect(336, 410, 62, 56, WHITE, 8, 4),
        rect(346, 384, 10, 40, BLUE, 4, 0),
        rect(364, 390, 10, 34, CORAL, 4, 0),
        rect(198, 392, 74, 42, TEAL, 8, 4),
        ellipse(235, 413, 22, 12, WHITE, 0),
        circle(235, 413, 7, INK, 0),
    ]
    for arm in DOC_ARMS[pose]:
        p.append(limb(arm, WHITE, 52))
        p.append(circle(*arm[-1], 26, DOC_SKIN))
    if pose == "point":
        p.append(rect(556, 104, 26, 16, DOC_SKIN, 8, 4, 'transform="rotate(-30 556 112)"'))
    write(f"Characters/doctor_{pose}.svg", w, h, [f'<g transform="translate(0,{DOC_SHIFT})">'] + p + ["</g>"])


# Clinic props -------------------------------------------------------------------
def exam_chair():
    w, h = 600, 700
    write("Props/exam_chair.svg", w, h, [
        ellipse(300, 668, 180, 26, GREY_DARK),
        rect(272, 470, 56, 196, GREY),
        rect(190, 600, 220, 30, GREY_DARK, 14),
        rect(236, 40, 128, 92, BLUE_DARK, 40),
        rect(160, 110, 280, 350, BLUE, 64),
        path("M206 170 Q300 148 394 170", "none", 0, extra='stroke="#FFFFFF" stroke-opacity="0.35" stroke-width="16" stroke-linecap="round"'),
        rect(110, 328, 84, 44, BLUE_DARK, 20),
        rect(406, 328, 84, 44, BLUE_DARK, 20),
        rect(130, 404, 340, 84, BLUE, 36),
    ])


def bottle(name, face=False, golden=False):
    """Eye-drop bottle, nozzle pointing down, 200x300."""
    body = YELLOW if golden else WHITE
    p = [
        rect(40, 20, 120, 50, TEAL, 18),
        rect(30, 60, 140, 160, body, 34),
        rect(50, 104, 100, 70, TEAL if not face else SKY, 16, 4),
        path("M64 218 L136 218 L118 258 L82 258 Z", WHITE),
        rect(90, 256, 20, 26, WHITE, 8, 5),
    ]
    if face:
        p += [
            ellipse(78, 128, 11, 14, WHITE, 4), circle(79, 131, 7, INK, 0),
            ellipse(122, 128, 11, 14, WHITE, 4), circle(123, 131, 7, INK, 0),
            path("M86 154 Q100 166 114 154", "none", 5),
            ellipse(64, 152, 9, 6, CHEEK, 0, 'fill-opacity="0.6"'),
            ellipse(136, 152, 9, 6, CHEEK, 0, 'fill-opacity="0.6"'),
        ]
    else:
        p += [ellipse(100, 139, 26, 14, WHITE, 0), circle(100, 139, 9, INK, 0)]
    write(f"Props/{name}.svg", 200, 300, p)


def drops():
    write("Props/drop.svg", 120, 160, [
        path(drop_path(60, 100, 44), DROP, 6),
        ellipse(46, 104, 9, 15, WHITE, 0, 'fill-opacity="0.8" transform="rotate(20 46 104)"'),
    ])
    write("Props/drop_gold.svg", 120, 160, [
        path(drop_path(60, 100, 44), YELLOW, 6),
        ellipse(46, 104, 9, 15, WHITE, 0, 'fill-opacity="0.8" transform="rotate(20 46 104)"'),
        star(80, 52, 14, 6, WHITE, 0),
    ])
    rays = []
    for i in range(7):
        a = math.radians(-180 + i * 30)
        x, y = 100 + math.cos(a) * 62, 90 + math.sin(a) * 62
        rays.append(path(drop_path(round(x, 1), round(y, 1), 11), DROP, 4))
    write("Props/splash.svg", 200, 120, rays)


def tissue():
    write("Props/tissue.svg", 220, 200, [
        path("M30 60 L190 46 L196 180 L24 188 Z", WHITE),
        path("M30 60 Q70 20 110 54 Q150 16 190 46", WHITE, 6),
        path("M60 110 Q110 96 160 110", "none", 4, GREY),
        path("M56 144 Q110 132 164 146", "none", 4, GREY),
    ])


def sunglasses():
    write("Props/sunglasses.svg", 320, 130, [
        path("M20 40 L300 40", "none", 10),
        path("M30 40 L140 40 Q140 110 86 112 Q30 110 30 40 Z", INK, 6),
        path("M180 40 L290 40 Q290 110 236 112 Q180 110 180 40 Z", INK, 6),
        path("M140 50 Q160 36 180 50", "none", 8),
        path("M50 56 L80 56", "none", 0, extra='stroke="#FFFFFF" stroke-opacity="0.6" stroke-width="10" stroke-linecap="round"'),
        path("M200 56 L230 56", "none", 0, extra='stroke="#FFFFFF" stroke-opacity="0.6" stroke-width="10" stroke-linecap="round"'),
    ])


def ceiling_star():
    write("Props/ceiling_star.svg", 240, 240, [
        star(120, 128, 108, 50, YELLOW),
        ellipse(98, 128, 9, 12, INK, 0), ellipse(142, 128, 9, 12, INK, 0),
        path("M104 152 Q120 166 136 152", "none", 5),
        ellipse(84, 150, 10, 6, CHEEK, 0, 'fill-opacity="0.6"'),
        ellipse(156, 150, 10, 6, CHEEK, 0, 'fill-opacity="0.6"'),
    ])


def sticker():
    pts = []
    for i in range(24):
        a = math.radians(i * 15)
        r = 230 if i % 2 == 0 else 212
        pts.append(f"{250 + math.cos(a) * r:.1f},{250 + math.sin(a) * r:.1f}")
    write("Props/sticker_brave_eyes.svg", 500, 500, [
        f'<polygon points="{" ".join(pts)}" fill="{YELLOW}" {st(8)}/>',
        circle(250, 250, 170, WHITE, 6),
        path(eye_shape(250, 230, 120, 52), WHITE, 8),
        circle(250, 230, 50, BLUE, 6),
        circle(250, 230, 24, INK, 0),
        circle(264, 216, 10, WHITE, 0),
        star(150, 350, 30, 13, YELLOW, 5), star(250, 372, 30, 13, YELLOW, 5), star(350, 350, 30, 13, YELLOW, 5),
    ])


def closeup_eye():
    """Close-up eye parts on a 600x600 canvas: skin disc, white, iris, pupil, lid, lashes."""
    s = 600
    eye = eye_shape(300, 300, 230, 92)
    write("Eye/closeup_skin.svg", s, s, [circle(300, 300, 292, WHITE, 0)])
    write("Eye/closeup_white.svg", s, s, [path(eye, WHITE, 0)])
    write("Eye/closeup_iris.svg", s, s, [
        circle(300, 300, 96, WHITE, 0),
        *[line([(300 + math.cos(math.radians(a)) * 50, 300 + math.sin(math.radians(a)) * 50),
                (300 + math.cos(math.radians(a)) * 86, 300 + math.sin(math.radians(a)) * 86)], 6, "#000000", 0.12)
          for a in range(0, 360, 30)],
        circle(300, 300, 96, "none", 6),
    ])
    write("Eye/closeup_pupil.svg", s, s, [circle(300, 300, 44, INK, 0)])
    write("Eye/closeup_shine.svg", s, s, [circle(334, 262, 20, WHITE, 0), circle(272, 336, 9, WHITE, 0, 'fill-opacity="0.8"')])
    # Upper lid (600x300, skin-tinted): the game stretches it down from the top of the eye to blink.
    write("Eye/closeup_lid.svg", s, 300, [path("M20 0 L580 0 L580 200 Q300 310 20 200 Z", WHITE, 0)])
    lashes = []
    for t in (0.18, 0.32, 0.46, 0.6, 0.74, 0.86):
        # Points on the upper lid curve (70,300) -> (300,116) -> (530,300), pointing outward.
        x = 70 * (1 - t) ** 2 + 600 * t * (1 - t) + 530 * t * t
        y = 300 * (1 - t) ** 2 + 232 * t * (1 - t) + 300 * t * t
        dx, dy = x - 300, y - 360
        length = math.hypot(dx, dy)
        lashes.append(line([(x, y), (x + dx / length * 40, y + dy / length * 40)], 10))
    write("Eye/closeup_outline.svg", s, s, [path(eye, "none", 12)] + lashes)
    # Closed eye: a soft curve with lashes pointing down.
    write("Eye/closeup_closed.svg", s, s, [
        path("M70 300 Q300 400 530 300", "none", 12),
        *[line([(300 + (x - 300), 300 + 50 * (1 - ((x - 300) / 230) ** 2)),
                (300 + (x - 300) * 1.08, 300 + 50 * (1 - ((x - 300) / 230) ** 2) + 36)], 10)
          for x in (140, 200, 260, 340, 400, 460)],
    ])


# Backgrounds and room pieces (the game fills walls and floors with flat colour) --------
def room_pieces():
    write("Room/window.svg", 420, 340, [
        rect(10, 10, 400, 320, WHITE, 30),
        rect(34, 34, 352, 272, SKY, 16, 4),
        circle(310, 110, 44, YELLOW, 0),
        ellipse(140, 120, 70, 30, WHITE, 0), ellipse(110, 104, 40, 30, WHITE, 0), ellipse(170, 100, 44, 34, WHITE, 0),
        ellipse(260, 236, 60, 24, WHITE, 0), ellipse(290, 222, 36, 26, WHITE, 0),
        path("M210 34 L210 306", "none", 8, WHITE),
        path("M34 170 L386 170", "none", 8, WHITE),
    ])
    write("Room/eye_chart.svg", 260, 360, [
        rect(10, 10, 240, 340, WHITE, 24),
        # Picture chart rows: house, apple, star (big to small).
        path("M130 50 L180 92 L170 92 L170 132 L90 132 L90 92 L80 92 Z", CORAL, 5),
        circle(96, 190, 26, GREEN, 5), star(166, 190, 30, 13, YELLOW, 5),
        circle(80, 258, 16, BLUE, 4), path("M130 242 L148 274 L112 274 Z", PURPLE, 4), circle(180, 258, 16, CORAL, 4),
        circle(70, 312, 9, INK, 0), circle(110, 312, 9, INK, 0), circle(150, 312, 9, INK, 0), circle(190, 312, 9, INK, 0),
    ])
    write("Room/cabinet.svg", 300, 300, [
        rect(10, 60, 280, 230, WOOD, 20),
        path("M150 60 L150 290", "none", 6),
        circle(130, 170, 8, INK, 0), circle(170, 170, 8, INK, 0),
        rect(40, 30, 50, 40, TEAL, 10, 5), rect(110, 14, 40, 56, CORAL, 10, 5), rect(196, 22, 70, 48, YELLOW, 12, 5),
    ])
    write("Room/plant.svg", 220, 340, [
        ellipse(70, 120, 40, 84, GREEN, 6, 'transform="rotate(-24 70 120)"'),
        ellipse(150, 110, 40, 90, GREEN_DARK, 6, 'transform="rotate(22 150 110)"'),
        ellipse(110, 90, 38, 90, GREEN, 6),
        path("M50 220 L170 220 L156 330 L64 330 Z", CORAL),
        rect(40, 206, 140, 30, "#E85A5A", 12),
    ])
    write("Room/lamp.svg", 300, 200, [
        line([(150, 0), (150, 70)], 10),
        path("M60 160 Q60 70 150 70 Q240 70 240 160 Z", YELLOW),
        ellipse(150, 160, 90, 18, "#FFF2B8"),
    ])
    write("Room/rug.svg", 700, 160, [ellipse(350, 80, 340, 70, PINK), ellipse(350, 80, 280, 48, "none", 5, 'stroke-dasharray="18 14"')])


# Map and title -----------------------------------------------------------------
def map_pieces():
    write("Map/clinic.svg", 520, 460, [
        rect(40, 140, 440, 300, WHITE, 26),
        path("M20 160 L260 30 L500 160 Z", CORAL),
        rect(210, 300, 100, 140, BLUE, 18),
        circle(290, 372, 7, WHITE, 0),
        rect(80, 200, 100, 80, SKY, 14, 5), rect(340, 200, 100, 80, SKY, 14, 5),
        circle(260, 105, 40, WHITE, 5),
        path(eye_shape(260, 105, 28, 12), WHITE, 5), circle(260, 105, 10, BLUE, 0),
    ])
    write("Map/tree.svg", 200, 260, [
        rect(86, 150, 28, 100, WOOD_DARK, 10),
        circle(100, 100, 80, GREEN), circle(70, 80, 6, GREEN_DARK, 0), circle(130, 120, 6, GREEN_DARK, 0),
    ])
    write("Map/cloud.svg", 320, 160, [
        path("M50 130 Q10 130 14 96 Q18 62 60 66 Q70 20 120 24 Q160 0 200 30 Q250 20 266 66 Q310 70 306 104 Q302 130 270 130 Z", WHITE, 0),
    ])


# Icons (all 256x256, drawn in ink so the game can tint them) ------------------------
def icons():
    s = 256
    write("Icons/home.svg", s, s, [path("M128 34 L232 124 L202 124 L202 218 L152 218 L152 162 L104 162 L104 218 L54 218 L54 124 L24 124 Z", INK, 10)])
    write("Icons/restart.svg", s, s, [
        path("M196 128 A68 68 0 1 1 168 72", "none", 30),
        path("M150 40 L206 64 L170 112 Z", INK, 10),
    ])
    write("Icons/skip.svg", s, s, [path("M40 52 L128 128 L40 204 Z", INK, 12), path("M128 52 L216 128 L128 204 Z", INK, 12)])
    write("Icons/coin.svg", s, s, [circle(128, 128, 104, YELLOW_DARK, 10), circle(128, 128, 80, YELLOW, 0),
                                   star(128, 134, 50, 22, "#FFE38A", 0), path("M70 90 Q90 60 128 56", "none", 10, "#FFFFFF")])
    write("Icons/lock.svg", s, s, [path("M80 120 L80 84 Q80 36 128 36 Q176 36 176 84 L176 120", "none", 22), rect(52, 112, 152, 120, YELLOW, 24, 10),
                                   circle(128, 160, 16, INK, 0), rect(120, 166, 16, 34, INK, 6, 0)])
    write("Icons/bag.svg", s, s, [path("M92 96 Q92 40 128 40 Q164 40 164 96", "none", 14), path("M40 92 L216 92 L200 228 L56 228 Z", CORAL, 10),
                                  circle(128, 160, 26, YELLOW, 8)])
    write("Icons/shirt.svg", s, s, [path("M88 40 L48 64 L22 120 L64 140 L76 116 L76 226 L180 226 L180 116 L192 140 L234 120 L208 64 L168 40 Q128 76 88 40 Z", BLUE, 10)])
    write("Icons/palette.svg", s, s, [path("M128 28 Q236 28 236 120 Q236 168 196 168 L172 168 Q148 168 156 196 Q164 228 128 228 Q20 228 20 128 Q20 28 128 28 Z", "#F3D2A8", 10),
                                      circle(80, 92, 18, CORAL, 6), circle(128, 72, 18, YELLOW, 6), circle(178, 92, 18, GREEN, 6), circle(70, 150, 18, BLUE, 6)])
    write("Icons/smile.svg", s, s, [circle(128, 128, 104, YELLOW, 10), ellipse(92, 104, 12, 16, INK, 0), ellipse(164, 104, 12, 16, INK, 0),
                                    path("M78 152 Q128 204 178 152", "none", 12)])
    write("Icons/hair.svg", s, s, [path("M36 176 Q20 40 128 36 Q236 40 220 176 Q200 120 160 112 Q128 140 96 112 Q56 120 36 176 Z", WOOD_DARK, 10)])
    write("Icons/cap.svg", s, s, [path("M40 160 Q40 56 128 56 Q216 56 216 160 Z", CORAL, 10), path("M150 160 Q230 150 246 176 Q200 196 150 180 Z", "#E85A5A", 10)])
    write("Icons/hold.svg", s, s, [path("M128 240 Q114 200 132 172", "none", 6), ellipse(132, 100, 64, 74, PURPLE, 10), path("M120 172 L144 172 L132 158 Z", PURPLE, 6)])
    write("Icons/glasses.svg", s, s, [circle(76, 128, 46, SKY, 10), circle(180, 128, 46, SKY, 10), path("M118 120 Q128 108 138 120", "none", 10),
                                      path("M30 120 L14 100 M226 120 L242 100", "none", 10)])
    write("Icons/close.svg", s, s, [path("M64 64 L192 192 M192 64 L64 192", "none", 40)])
    write("Icons/play.svg", s, s, [path("M70 40 L210 128 L70 216 Z", INK, 14)])
    write("Icons/check.svg", s, s, [path("M48 134 L104 190 L212 70", "none", 40)])
    write("Icons/arrow_up.svg", s, s, [path("M128 28 L222 132 L162 132 L162 228 L94 228 L94 132 L34 132 Z", INK, 10)])
    write("Icons/arrow_right.svg", s, s, [path("M228 128 L124 222 L124 162 L28 162 L28 94 L124 94 L124 34 Z", INK, 10)])
    write("Icons/star.svg", s, s, [star(128, 136, 116, 52, YELLOW, 10)])
    write("Icons/star_empty.svg", s, s, [star(128, 136, 112, 50, "#E7ECF3", 10)])
    write("Icons/sparkle.svg", s, s, [path("M128 10 Q140 116 246 128 Q140 140 128 246 Q116 140 10 128 Q116 116 128 10 Z", WHITE, 0)])
    write("Icons/hand.svg", s, s, [
        path("M100 230 Q60 190 54 150 Q50 128 70 128 Q86 128 98 150 L98 50 Q98 26 120 26 Q142 26 142 50 L142 110 "
             "Q170 104 186 118 Q212 116 220 140 L220 182 Q218 222 186 236 Z", WHITE, 10),
    ])
    write("Icons/sound_on.svg", s, s, [
        path("M30 96 L80 96 L136 44 L136 212 L80 160 L30 160 Z", INK, 10),
        path("M170 92 Q194 128 170 164", "none", 16), path("M196 64 Q240 128 196 192", "none", 16),
    ])
    write("Icons/sound_off.svg", s, s, [
        path("M30 96 L80 96 L136 44 L136 212 L80 160 L30 160 Z", INK, 10),
        path("M168 96 L228 160 M228 96 L168 160", "none", 18),
    ])
    teeth = []
    for i in range(8):
        teeth.append(f'<rect x="108" y="14" width="40" height="52" rx="10" fill="{GREY}" {st(10)} transform="rotate({i * 45} 128 128)"/>')
    write("Icons/gear.svg", s, s, [*teeth, circle(128, 128, 84, GREY, 10), circle(128, 128, 34, WHITE, 10)])
    write("Icons/trash.svg", s, s, [
        rect(56, 82, 144, 150, CORAL, 18, 10),
        rect(36, 52, 184, 34, CORAL, 12, 10),
        path("M100 52 L100 30 L156 30 L156 52", "none", 10),
        line([(98, 116), (98, 200)], 12), line([(128, 116), (128, 200)], 12), line([(158, 116), (158, 200)], 12),
    ])
    write("Icons/plus.svg", s, s, [path("M128 40 L128 216 M40 128 L216 128", "none", 44)])
    write("Icons/text_size.svg", s, s, [
        path("M20 220 L84 40 L116 40 L180 220 L146 220 L132 178 L68 178 L54 220 Z M78 148 L122 148 L100 84 Z", INK, 0),
        path("M160 220 L196 120 L216 120 L252 220 L230 220 L222 196 L190 196 L182 220 Z M196 176 L216 176 L206 146 Z", INK, 0),
    ])
    write("Icons/warning.svg", s, s, [
        path("M128 22 Q140 22 148 36 L240 204 Q248 222 228 226 L28 226 Q8 222 16 204 L108 36 Q116 22 128 22 Z", YELLOW, 10),
        rect(114, 78, 28, 88, INK, 12, 0), circle(128, 194, 16, INK, 0),
    ])
    write("Icons/snowflake.svg", s, s, [
        *[line([(128 + math.cos(math.radians(a)) * 108, 128 + math.sin(math.radians(a)) * 108),
                (128 - math.cos(math.radians(a)) * 108, 128 - math.sin(math.radians(a)) * 108)], 18, DROP_DARK) for a in (90, 30, -30)],
        *[line([(128 + math.cos(math.radians(a)) * 70 + math.cos(math.radians(a + 40)) * 30,
                 128 + math.sin(math.radians(a)) * 70 + math.sin(math.radians(a + 40)) * 30),
                (128 + math.cos(math.radians(a)) * 70, 128 + math.sin(math.radians(a)) * 70),
                (128 + math.cos(math.radians(a)) * 70 + math.cos(math.radians(a - 40)) * 30,
                 128 + math.sin(math.radians(a)) * 70 + math.sin(math.radians(a - 40)) * 30)], 14, DROP_DARK)
          for a in range(0, 360, 60)],
    ])
    write("Icons/clock.svg", s, s, [circle(128, 128, 100, WHITE, 16), line([(128, 128), (128, 70)], 16), line([(128, 128), (170, 150)], 16)])
    write("Icons/sun.svg", s, s, [
        *[line([(128 + math.cos(math.radians(a)) * 82, 128 + math.sin(math.radians(a)) * 82),
                (128 + math.cos(math.radians(a)) * 118, 128 + math.sin(math.radians(a)) * 118)], 16, ORANGE) for a in range(0, 360, 45)],
        circle(128, 128, 62, YELLOW, 10),
    ])
    write("Icons/blurry.svg", s, s, [
        path(eye_shape(128, 128, 110, 46), WHITE, 10), circle(128, 128, 38, BLUE, 8),
        circle(128, 128, 16, INK, 0),
        path("M24 210 Q70 190 128 210 Q186 230 232 210", "none", 10, GREY_DARK),
        path("M24 46 Q70 26 128 46 Q186 66 232 46", "none", 10, GREY_DARK),
    ])
    write("Icons/heart.svg", s, s, [path("M128 226 Q30 160 30 96 Q30 40 82 40 Q114 40 128 74 Q142 40 174 40 Q226 40 226 96 Q226 160 128 226 Z", CORAL, 10)])
    write("Icons/speak.svg", s, s, [path("M40 50 L216 50 Q236 50 236 70 L236 160 Q236 180 216 180 L110 180 L60 226 L70 180 L40 180 Q20 180 20 160 L20 70 Q20 50 40 50 Z", WHITE, 10),
                                     circle(84, 116, 12, INK, 0), circle(128, 116, 12, INK, 0), circle(172, 116, 12, INK, 0)])
    write("Icons/drop.svg", s, s, [path(drop_path(128, 160, 72), DROP, 10),
                                    ellipse(104, 166, 14, 24, WHITE, 0, 'fill-opacity="0.8" transform="rotate(20 104 166)"')])
    write("Icons/eye.svg", s, s, [path(eye_shape(128, 128, 112, 48), WHITE, 10), circle(128, 128, 40, BLUE, 8), circle(128, 128, 18, INK, 0),
                                   circle(140, 116, 8, WHITE, 0)])
    write("Icons/eye_big_pupil.svg", s, s, [path(eye_shape(128, 128, 112, 48), WHITE, 10), circle(128, 128, 44, BLUE, 8), circle(128, 128, 34, INK, 0),
                                             circle(142, 114, 8, WHITE, 0)])
    write("Icons/eye_rainbow.svg", s, s, [path(eye_shape(128, 128, 112, 48), WHITE, 10),
                                           circle(128, 128, 44, CORAL, 0), circle(128, 128, 34, YELLOW, 0), circle(128, 128, 24, GREEN, 0),
                                           circle(128, 128, 44, "none", 8), circle(128, 128, 12, INK, 0)])
    write("Icons/sleepy.svg", s, s, [circle(110, 150, 80, YELLOW, 10),
                                      path("M66 150 Q82 166 98 150", "none", 9), path("M122 150 Q138 166 154 150", "none", 9),
                                      path("M96 196 Q110 204 124 196", "none", 8),
                                      path("M180 40 L226 40 L180 90 L226 90", "none", 12), path("M150 98 L176 98 L150 126 L176 126", "none", 9)])
    write("Icons/head_back.svg", s, s, [circle(118, 110, 62, "#F2C69B", 10),
                                         path("M76 90 Q118 66 160 90", "none", 0),
                                         ellipse(98, 92, 10, 12, INK, 0), ellipse(138, 92, 10, 12, INK, 0),
                                         path("M100 130 Q118 140 136 130", "none", 8),
                                         line([(210, 200), (210, 50)], 14, TEAL), path("M186 74 L210 40 L234 74", "none", 14, TEAL),
                                         rect(74, 178, 88, 60, YELLOW, 24, 10)])
    write("Icons/eyes_shut.svg", s, s, [circle(128, 128, 96, "#F2C69B", 10),
                                         path("M68 118 L102 128 L68 138", "none", 10), path("M188 118 L154 128 L188 138", "none", 10),
                                         path("M100 176 Q128 166 156 176", "none", 9)])
    write("Icons/wiggle.svg", s, s, [circle(128, 128, 84, "#F2C69B", 10),
                                      circle(100, 116, 12, INK, 0), circle(156, 116, 12, INK, 0),
                                      ellipse(128, 160, 18, 14, INK, 0),
                                      path("M20 84 Q30 128 20 172", "none", 10, TEAL), path("M236 84 Q226 128 236 172", "none", 10, TEAL)])
    write("Icons/sunglasses.svg", s, s, [
        path("M14 96 L242 96", "none", 10),
        path("M24 96 L118 96 Q118 170 70 172 Q24 170 24 96 Z", INK, 8),
        path("M138 96 L232 96 Q232 170 184 172 Q138 170 138 96 Z", INK, 8),
    ])


def stop_icons():
    s = 256
    write("Stops/visit.svg", s, s, [rect(50, 40, 156, 196, WHITE, 18, 10), rect(92, 22, 72, 40, TEAL, 12, 8),
                                     path("M84 110 L104 130 L140 92", "none", 10, GREEN), line([(150, 112), (180, 112)], 10),
                                     path("M84 176 L104 196 L140 158", "none", 10, GREEN), line([(150, 178), (180, 178)], 10)])
    write("Stops/waiting.svg", s, s, [rect(40, 60, 176, 110, CORAL, 30, 10), rect(30, 150, 196, 50, "#E85A5A", 20, 10),
                                       line([(60, 200), (60, 236)], 14), line([(196, 200), (196, 236)], 14)])
    write("Stops/match.svg", s, s, [rect(26, 50, 110, 150, YELLOW, 18, 10, 'transform="rotate(-10 81 125)"'),
                                     rect(120, 50, 110, 150, SKY, 18, 10, 'transform="rotate(10 175 125)"'),
                                     star(80, 126, 30, 13, WHITE, 6), circle(176, 126, 26, WHITE, 6)])
    write("Stops/light.svg", s, s, [rect(36, 150, 150, 40, GREY, 18, 10, 'transform="rotate(-30 111 170)"'),
                                     path("M170 100 L236 40 M184 128 L250 112 M146 82 L170 16", "none", 12, YELLOW),
                                     circle(170, 112, 18, YELLOW, 8)])
    write("Stops/drops.svg", s, s, [path(drop_path(128, 160, 72), DROP, 10),
                                     ellipse(104, 166, 14, 24, WHITE, 0, 'fill-opacity="0.8" transform="rotate(20 104 166)"')])
    write("Stops/wait.svg", s, s, [path("M64 30 L192 30 L192 50 Q192 110 140 128 Q192 146 192 206 L192 226 L64 226 L64 206 Q64 146 116 128 Q64 110 64 50 Z", WHITE, 10),
                                    path("M90 70 L166 70 Q160 104 128 116 Q96 104 90 70 Z", YELLOW, 0),
                                    path("M128 170 L170 210 L86 210 Z", YELLOW, 0)])
    write("Stops/microscope.svg", s, s, [rect(40, 200, 176, 30, GREY_DARK, 12, 10), rect(110, 120, 36, 90, GREY, 10, 10),
                                          rect(70, 70, 120, 64, BLUE, 26, 10), circle(70, 102, 30, SKY, 10)])
    write("Stops/biglight.svg", s, s, [line([(128, 236), (128, 140)], 16), path("M50 140 Q50 40 128 40 Q206 40 206 140 Z", YELLOW, 10),
                                        ellipse(128, 140, 78, 16, "#FFF2B8", 8)])
    write("Stops/done.svg", s, s, [path("M70 40 L186 40 L186 100 Q186 160 128 160 Q70 160 70 100 Z", YELLOW, 10),
                                    path("M70 60 Q30 60 34 96 Q38 122 74 120 M186 60 Q226 60 222 96 Q218 122 182 120", "none", 10),
                                    rect(110, 160, 36, 40, YELLOW_DARK, 4, 10), rect(70, 198, 116, 34, WOOD_DARK, 10, 10),
                                    star(128, 96, 30, 13, WHITE, 6)])


# Seek-and-find items for Blurry Spy (each 256x256) -----------------------------
def spy_items():
    s = 256
    items = {
        "duck": [ellipse(128, 160, 92, 60, YELLOW), circle(170, 96, 46, YELLOW), path("M206 96 L248 106 L206 118 Z", ORANGE, 6),
                 circle(180, 86, 8, INK, 0), path("M70 150 Q110 130 140 160", "none", 6)],
        "ball": [circle(128, 128, 100, CORAL), path("M28 128 Q128 80 228 128", "none", 8), path("M28 128 Q128 176 228 128", "none", 8),
                 path("M128 28 Q90 128 128 228", "none", 8, WHITE)],
        "kite": [path("M128 20 L210 110 L128 220 L46 110 Z", PURPLE), path("M128 20 L128 220 M46 110 L210 110", "none", 6),
                 path("M128 220 Q100 240 120 250 Q140 240 112 230", "none", 6, CORAL)],
        "teddy": [circle(76, 60, 30, WOOD), circle(180, 60, 30, WOOD), circle(128, 96, 66, WOOD), ellipse(128, 196, 72, 56, WOOD),
                  ellipse(128, 116, 28, 20, "#F3D2A8", 5), circle(128, 108, 8, INK, 0), circle(104, 84, 7, INK, 0), circle(152, 84, 7, INK, 0)],
        "apple": [circle(100, 144, 76, CORAL), circle(156, 144, 76, CORAL, 0), path("M100 70 Q128 80 156 70", "none", 0),
                  circle(156, 144, 76, "none", 6), path("M128 70 Q128 40 142 24", "none", 10, WOOD_DARK),
                  ellipse(166, 52, 28, 14, GREEN, 6, 'transform="rotate(-24 166 52)"')],
        "car": [path("M30 160 L40 110 Q50 92 70 92 L90 60 Q100 46 120 46 L172 46 Q190 46 200 62 L220 92 Q236 96 236 120 L236 160 Z", BLUE),
                rect(104, 60, 70, 34, SKY, 8, 5), circle(76, 170, 30, INK, 0), circle(76, 170, 12, GREY, 0),
                circle(190, 170, 30, INK, 0), circle(190, 170, 12, GREY, 0)],
        "flower": [line([(128, 130), (128, 240)], 14, GREEN_DARK),
                   *[circle(128 + math.cos(math.radians(a)) * 52, 100 + math.sin(math.radians(a)) * 52, 36, PINK) for a in range(0, 360, 72)],
                   circle(128, 100, 32, YELLOW)],
        "fish": [ellipse(116, 128, 84, 56, ORANGE), path("M190 128 L246 80 L246 176 Z", ORANGE),
                 circle(76, 116, 12, WHITE, 5), circle(78, 118, 6, INK, 0), path("M116 80 Q130 128 116 176", "none", 6)],
        "book": [path("M128 60 Q80 36 24 50 L24 210 Q80 196 128 220 Q176 196 232 210 L232 50 Q176 36 128 60 Z", GREEN),
                 path("M128 60 L128 220", "none", 6), line([(52, 96), (100, 104)], 6, WHITE), line([(156, 104), (204, 96)], 6, WHITE)],
        "rocket": [path("M128 14 Q190 70 180 180 L76 180 Q66 70 128 14 Z", WHITE), circle(128, 100, 26, SKY, 6),
                   path("M76 140 L36 196 L80 186 Z", CORAL), path("M180 140 L220 196 L176 186 Z", CORAL),
                   path("M100 182 L128 242 L156 182 Z", ORANGE)],
        "boat": [path("M24 150 L232 150 L196 216 L60 216 Z", CORAL), line([(128, 150), (128, 30)], 8),
                 path("M136 36 L210 130 L136 130 Z", WHITE), path("M120 56 L60 130 L120 130 Z", YELLOW)],
        "cat": [path("M60 100 L66 30 L110 70 L146 70 L190 30 L196 100 Z", GREY_DARK), circle(128, 130, 84, GREY_DARK),
                ellipse(96, 122, 12, 16, YELLOW, 4), ellipse(160, 122, 12, 16, YELLOW, 4), path("M116 156 L128 166 L140 156", "none", 6),
                path("M40 160 L90 156 M40 180 L90 168 M216 160 L166 156 M216 180 L166 168", "none", 4)],
        "butterfly": [ellipse(80, 90, 56, 50, PINK), ellipse(176, 90, 56, 50, PINK), ellipse(88, 176, 40, 38, PURPLE), ellipse(168, 176, 40, 38, PURPLE),
                      rect(116, 60, 24, 160, INK, 12, 0), path("M120 64 Q100 20 84 20 M136 64 Q156 20 172 20", "none", 6)],
        "balloon": [path("M128 240 Q140 210 128 196", "none", 6), ellipse(128, 104, 80, 92, GREEN), path("M116 196 L140 196 L128 182 Z", GREEN, 5),
                    ellipse(100, 72, 14, 24, WHITE, 0, 'fill-opacity="0.6" transform="rotate(24 100 72)"')],
        "shoe": [path("M30 150 L30 80 Q60 70 90 80 L110 120 Q170 120 210 150 Q236 166 230 200 L30 200 Z", BLUE),
                 rect(24, 196, 212, 24, WHITE, 10), line([(70, 100), (96, 92)], 6, WHITE), line([(80, 124), (106, 116)], 6, WHITE)],
        "umbrella": [path("M24 132 Q24 30 128 30 Q232 30 232 132 Q206 112 180 132 Q154 112 128 132 Q102 112 76 132 Q50 112 24 132 Z", TEAL),
                     path("M128 132 L128 214 Q128 236 106 236 Q88 236 88 218", "none", 10), circle(128, 26, 8, INK, 0)],
        "drum": [ellipse(128, 96, 96, 34, "#FFFFFF"), path("M32 96 L32 186 Q128 226 224 186 L224 96 Q128 136 32 96 Z", CORAL),
                 path("M32 120 L80 186 L128 130 L176 186 L224 120", "none", 6, YELLOW), line([(150, 20), (110, 80)], 12, WOOD_DARK),
                 circle(150, 20, 12, YELLOW, 6)],
        "cupcake": [path("M56 140 L200 140 L180 236 L76 236 Z", PINK), path("M48 140 Q40 90 90 84 Q100 40 140 50 Q190 50 190 92 Q220 100 208 140 Z", WHITE),
                    circle(128, 40, 18, CORAL)],
    }
    for name, parts in items.items():
        write(f"Spy/{name}.svg", s, s, parts)
    write("Spy/sofa.svg", 700, 340, [rect(60, 60, 580, 200, CORAL, 60), rect(20, 150, 100, 160, "#E85A5A", 40), rect(580, 150, 100, 160, "#E85A5A", 40),
                                     rect(100, 190, 500, 110, "#FF8585", 30), line([(350, 190), (350, 300)], 6)])
    write("Spy/shelf.svg", 420, 480, [rect(10, 10, 400, 460, WOOD, 20), rect(36, 36, 348, 190, WOOD_DARK, 10, 0),
                                      rect(36, 246, 348, 196, WOOD_DARK, 10, 0)])


def main():
    import sys
    import avatar_parts
    avatar_parts.generate(sys.modules[__name__])
    doctor_head()
    face_layers("doctor", DOC_HEAD, ((112, 164), (188, 164)), 15, 206, 22, 124)
    for pose in DOC_ARMS:
        doctor_body(pose)
    exam_chair()
    bottle("bottle")
    bottle("bottle_buddy", face=True)
    drops()
    tissue()
    sunglasses()
    ceiling_star()
    sticker()
    closeup_eye()
    room_pieces()
    map_pieces()
    icons()
    stop_icons()
    spy_items()
    print(f"wrote {len(WRITTEN)} SVG files")
    print("doctor hands:", DOC_HANDS)


if __name__ == "__main__":
    main()
