"""The kids' avatar parts, so the player can build and dress their own character.

Parts the game recolours (skin, hair, clothes) are drawn in black and white: white areas take
the chosen colour and black lines become the ink outline (the "EyeCanDo/UI Recolor" shader
does this). Light grey becomes a slightly darker shade of the chosen colour. Everything else
(eyes, mouths, hats, glasses, held items) is drawn in full colour.

Canvases:
- Head parts: 340x360. The face centre is at (170, 220) and the chin joins the neck at (170, 316).
  Parts are drawn on the first PoC's 260x260 layout and shifted by (40, 80), leaving room for hats.
- Standing bodies (stand, wave): 340x620, neck at (170, 96). Drawn shifted down 60 px so a
  raised hand is never clipped.
- Seated body: 340x430, neck at (170, 36).
"""

import math

import shop_items

BLACK = "#000000"
WHITE = "#FFFFFF"
SHADE = "#D6D6D6"
OUT = 6

g = None  # the generate_art module, for its palette, write() and shapes


def st(width=OUT):
    return f'stroke="{BLACK}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'


def c(cx, cy, r, fill=WHITE, w=OUT, extra=""):
    s = st(w) if w else ""
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {s} {extra}/>'


def e(cx, cy, rx, ry, fill=WHITE, w=OUT, extra=""):
    s = st(w) if w else ""
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {s} {extra}/>'


def p(d, fill=WHITE, w=OUT, extra=""):
    s = st(w) if w else ""
    return f'<path d="{d}" fill="{fill}" {s} {extra}/>'


def limb(points, width=40):
    pts = " ".join(f"{x},{y}" for x, y in points)
    return (f'<polyline points="{pts}" fill="none" stroke="{BLACK}" stroke-width="{width + 2 * OUT}" stroke-linejoin="round" stroke-linecap="round"/>\n'
            f'<polyline points="{pts}" fill="none" stroke="{WHITE}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"/>')


def head(name, parts):
    """Writes a head part on the 340x340 canvas, drawn in the 260x260 layout."""
    g.write(f"Avatar/{name}.svg", 340, 360, ['<g transform="translate(40,80)">'] + [x for x in parts if x] + ["</g>"])


def body(name, pose, parts):
    if pose == "seated":
        g.write(f"Avatar/{name}_seated.svg", 340, 430, parts)
    else:
        g.write(f"Avatar/{name}_{pose}.svg", 340, 620, ['<g transform="translate(0,60)">'] + parts + ["</g>"])


CX, CY, R = 130, 140, 88
EYES = ((100, 150), (160, 150))


# Head and ears (skin) -------------------------------------------------------------------

def heads():
    head("head", [c(CX, CY, R)])
    ears = {
        "round": lambda x, s: c(x, CY + 8, 17),
        "small": lambda x, s: c(x + s * 2, CY + 8, 12),
        "big": lambda x, s: e(x - s * 4, CY + 6, 23, 27),
        "pointy": lambda x, s: p(f"M{x + s * 8} {CY - 8} L{x - s * 22} {CY - 26} L{x - s * 6} {CY + 26} Z"),
    }
    for name, draw in ears.items():
        head(f"ears_{name}", [draw(CX - R + 2, -1), draw(CX + R - 2, 1)])
    head("cheeks", [e(CX - 50, CY + 36, 14, 9, "#FF8E8E", 0, 'fill-opacity="0.55"'),
                    e(CX + 50, CY + 36, 14, 9, "#FF8E8E", 0, 'fill-opacity="0.55"')])


# Hair (hair colour). Each style has a back layer (behind the head) and a front layer. --------

def hairs():
    styles = {
        "tuft": ("", [p(f"M{CX - 84} {CY - 6} Q{CX - 82} {CY - 92} {CX} {CY - 90} Q{CX + 84} {CY - 92} {CX + 86} {CY - 6} "
                        f"Q{CX + 64} {CY - 46} {CX + 32} {CY - 42} Q{CX} {CY - 64} {CX - 30} {CY - 42} Q{CX - 62} {CY - 46} {CX - 84} {CY - 6} Z"),
                      p(f"M{CX} {CY - 88} Q{CX + 12} {CY - 118} {CX + 36} {CY - 108}", "none", 12)]),
        "curly": ([c(CX - 62, CY - 62, 48), c(CX + 62, CY - 62, 48), c(CX, CY - 84, 56), c(CX - 92, CY - 10, 34), c(CX + 92, CY - 10, 34)],
                  [p(f"M{CX - 82} {CY - 8} Q{CX - 70} {CY - 70} {CX} {CY - 74} Q{CX + 70} {CY - 70} {CX + 82} {CY - 8} "
                     f"Q{CX + 50} {CY - 46} {CX} {CY - 44} Q{CX - 50} {CY - 46} {CX - 82} {CY - 8} Z")]),
        "bowl": ("", [p(f"M{CX - 92} {CY + 6} Q{CX - 96} {CY - 96} {CX} {CY - 96} Q{CX + 96} {CY - 96} {CX + 92} {CY + 6} "
                        f"L{CX + 70} {CY - 30} L{CX - 70} {CY - 30} Z")]),
        "pigtails": ([e(CX - 104, CY + 30, 30, 46, extra=f'transform="rotate(20 {CX - 104} {CY + 30})"'),
                      e(CX + 104, CY + 30, 30, 46, extra=f'transform="rotate(-20 {CX + 104} {CY + 30})"')],
                     [p(f"M{CX - 86} {CY} Q{CX - 84} {CY - 92} {CX} {CY - 90} Q{CX + 84} {CY - 92} {CX + 86} {CY} "
                        f"Q{CX + 40} {CY - 60} {CX + 6} {CY - 40} Q{CX - 30} {CY - 66} {CX - 86} {CY} Z")]),
        "puffs": ([c(CX - 92, CY - 70, 40), c(CX + 92, CY - 70, 40)],
                  [p(f"M{CX - 84} {CY - 4} Q{CX - 80} {CY - 90} {CX} {CY - 90} Q{CX + 80} {CY - 90} {CX + 84} {CY - 4} "
                     f"Q{CX + 54} {CY - 52} {CX} {CY - 50} Q{CX - 54} {CY - 52} {CX - 84} {CY - 4} Z")]),
        "long": ([p(f"M{CX - 96} {CY} Q{CX - 104} {CY - 100} {CX} {CY - 100} Q{CX + 104} {CY - 100} {CX + 96} {CY} "
                    f"L{CX + 100} {CY + 96} Q{CX + 70} {CY + 108} {CX + 56} {CY + 90} L{CX - 56} {CY + 90} Q{CX - 70} {CY + 108} {CX - 100} {CY + 96} Z")],
                 [p(f"M{CX - 86} {CY + 10} Q{CX - 86} {CY - 92} {CX} {CY - 92} Q{CX + 86} {CY - 92} {CX + 86} {CY + 10} "
                    f"Q{CX + 70} {CY - 40} {CX + 10} {CY - 44} Q{CX - 20} {CY - 30} {CX - 40} {CY - 50} Q{CX - 70} {CY - 30} {CX - 86} {CY + 10} Z")]),
        "spiky": ("", [p(f"M{CX - 86} {CY - 4} L{CX - 96} {CY - 70} L{CX - 60} {CY - 64} L{CX - 52} {CY - 112} L{CX - 18} {CY - 84} "
                         f"L{CX + 4} {CY - 124} L{CX + 28} {CY - 86} L{CX + 62} {CY - 110} L{CX + 66} {CY - 62} L{CX + 100} {CY - 66} "
                         f"L{CX + 86} {CY - 4} Q{CX + 50} {CY - 44} {CX} {CY - 42} Q{CX - 50} {CY - 44} {CX - 86} {CY - 4} Z")]),
        "bun": ([c(CX, CY - 106, 40)],
                [p(f"M{CX - 88} {CY - 2} Q{CX - 92} {CY - 94} {CX} {CY - 94} Q{CX + 92} {CY - 94} {CX + 88} {CY - 2} "
                   f"Q{CX + 60} {CY - 54} {CX} {CY - 54} Q{CX - 60} {CY - 54} {CX - 88} {CY - 2} Z")]),
        "buzz": ("", [p(f"M{CX - 86} {CY - 14} Q{CX - 84} {CY - 92} {CX} {CY - 92} Q{CX + 84} {CY - 92} {CX + 86} {CY - 14} "
                        f"Q{CX + 60} {CY - 64} {CX} {CY - 64} Q{CX - 60} {CY - 64} {CX - 86} {CY - 14} Z")]),
    }
    for name, (back, front) in styles.items():
        if back:
            head(f"hair_{name}_back", back)
        head(f"hair_{name}_front", front)


# Eyes (full colour). Iris layers are white so the game can tint them with the eye colour. ----

EYE_STYLES = {"round": (18, 11, 6), "big": (23, 15, 8), "lashes": (18, 11, 6), "dots": (0, 0, 9)}


def eyes():
    ink = g.INK
    for style, (r, iris, pupil) in EYE_STYLES.items():
        for state, dy in (("open", 2), ("up", -8)):
            whites, irises, pupils = [], [], []
            for x, y in EYES:
                if r:
                    whites.append(e(x, y, r, r * 1.15, WHITE, 0))
                    whites.append(f'<ellipse cx="{x}" cy="{y}" rx="{r}" ry="{r * 1.15}" fill="none" stroke="{ink}" stroke-width="5"/>')
                    irises.append(c(x, y + dy * r / 18, iris, WHITE, 0))
                    pupils.append(c(x, y + dy * r / 18, pupil, ink, 0))
                    pupils.append(c(x + iris * 0.35, y + dy * r / 18 - iris * 0.4, iris * 0.32, WHITE, 0))
                else:
                    pupils.append(e(x, y + dy * 0.6, pupil, pupil * 1.2, ink, 0))
                    pupils.append(c(x + 3, y + dy * 0.6 - 4, 3, WHITE, 0))
                if style == "lashes":
                    for k in (-1, 0, 1):
                        ax = x + (12 if x > CX else -12) + k * 6
                        whites.append(f'<line x1="{ax}" y1="{y - r}" x2="{ax + (8 if x > CX else -8) + k * 3}" y2="{y - r - 12}" stroke="{ink}" stroke-width="4" stroke-linecap="round"/>')
            if whites:
                head(f"eyes_{style}_{state}_white", whites)
                head(f"eyes_{style}_{state}_iris", irises)
            head(f"eyes_{style}_{state}_pupil", pupils)
    # Shared moments.
    head("eyes_closed", [p(f"M{x - 18} {y} Q{x} {y + 16} {x + 18} {y}", "none", 6).replace(BLACK, ink) for x, y in EYES])
    head("eyes_happy", [p(f"M{x - 18} {y + 4} Q{x} {y - 20} {x + 18} {y + 4}", "none", 6).replace(BLACK, ink) for x, y in EYES])
    head("eyes_squeeze", [p(f"M{x - 18 * s} {y - 10} L{x + 12 * s} {y} L{x - 18 * s} {y + 10}", "none", 6).replace(BLACK, ink)
                          for (x, y), s in ((EYES[0], 1), (EYES[1], -1))])


BROW_Y = 118


def brows():
    ink = g.INK
    widths = {"soft": 5, "thick": 9, "straight": 6, "arched": 5}
    for style, w in widths.items():
        for state in ("calm", "up", "worried"):
            parts = []
            for (x, _), side in ((EYES[0], -1), (EYES[1], 1)):
                lift = -8 if state == "up" else 0
                if state == "worried":
                    inner, outer = (BROW_Y - 8, BROW_Y + 2)
                else:
                    inner = outer = BROW_Y + lift
                left, right = (x - 18, x + 18)
                y_left, y_right = (outer, inner) if side < 0 else (inner, outer)
                if style == "straight":
                    d = f"M{left} {y_left} L{right} {y_right}"
                else:
                    bend = 16 if style == "arched" else 10
                    d = f"M{left} {y_left} Q{x} {min(y_left, y_right) - bend} {right} {y_right}"
                parts.append(f'<path d="{d}" fill="none" stroke="{ink}" stroke-width="{w}" stroke-linecap="round"/>')
            head(f"brows_{style}_{state}", parts)


def noses():
    ink = g.INK
    y = CY + 20
    head("nose_button", [f'<path d="M{CX - 6} {y} Q{CX} {y + 6} {CX + 6} {y}" fill="none" stroke="{ink}" stroke-width="4" stroke-linecap="round"/>'])
    head("nose_dot", [e(CX, y, 5, 4, ink, 0)])
    head("nose_curve", [f'<path d="M{CX + 2} {y - 16} Q{CX - 10} {y + 2} {CX + 6} {y + 4}" fill="none" stroke="{ink}" stroke-width="4" stroke-linecap="round"/>'])
    head("nose_round", [f'<circle cx="{CX}" cy="{y}" r="8" fill="#000000" fill-opacity="0.08" stroke="{ink}" stroke-width="4"/>'])


def mouths():
    ink = g.INK
    mx, my, mw = CX, 186, 20
    line = f'fill="none" stroke="{ink}" stroke-width="6" stroke-linecap="round" stroke-linejoin="round"'
    head("mouth_smile", [f'<path d="M{mx - mw} {my} Q{mx} {my + mw * 0.9} {mx + mw} {my}" {line}/>'])
    head("mouth_big", [f'<path d="M{mx - mw * 1.2} {my - 2} Q{mx} {my + mw * 1.3} {mx + mw * 1.2} {my - 2} Z" fill="{ink}"/>',
                       f'<path d="M{mx - mw * 0.9} {my} L{mx + mw * 0.9} {my} L{mx + mw * 0.7} {my + 6} L{mx - mw * 0.7} {my + 6} Z" fill="#FFFFFF"/>'])
    head("mouth_cheeky", [f'<path d="M{mx - mw} {my + 4} Q{mx + 2} {my + 14} {mx + mw} {my - 6}" {line}/>'])
    head("mouth_tongue", [f'<path d="M{mx - mw} {my} Q{mx} {my + mw * 0.9} {mx + mw} {my}" {line}/>',
                          f'<path d="M{mx - 2} {my + 10} Q{mx + 8} {my + 26} {mx + 14} {my + 8}" fill="#FF6B6B" stroke="{ink}" stroke-width="4"/>'])
    head("mouth_talk", [f'<path d="M{mx - mw * 0.7} {my} Q{mx} {my - 4} {mx + mw * 0.7} {my} Q{mx} {my + mw * 1.1} {mx - mw * 0.7} {my} Z" fill="{ink}" stroke="{ink}" stroke-width="5"/>',
                        e(mx, my + mw * 0.45, mw * 0.36, mw * 0.18, "#FF6B6B", 0)])
    head("mouth_grin", [f'<path d="M{mx - mw * 1.1} {my - 4} Q{mx} {my - 2} {mx + mw * 1.1} {my - 4} Q{mx} {my + mw * 1.5} {mx - mw * 1.1} {my - 4} Z" fill="{ink}" stroke="{ink}" stroke-width="5"/>',
                        f'<path d="M{mx - mw * 0.55} {my + mw * 0.75} Q{mx} {my + mw * 0.35} {mx + mw * 0.55} {my + mw * 0.75} Q{mx} {my + mw * 1.05} {mx - mw * 0.55} {my + mw * 0.75} Z" fill="#FF6B6B"/>'])
    head("mouth_oh", [e(mx, my + 6, mw * 0.35, mw * 0.45, ink, 0)])
    head("mouth_wobble", [f'<path d="M{mx - mw} {my + 6} Q{mx - mw * 0.66} {my - 4} {mx - mw * 0.33} {my + 6} Q{mx} {my + 16} {mx + mw * 0.33} {my + 6} '
                          f'Q{mx + mw * 0.66} {my - 4} {mx + mw} {my + 6}" {line}/>'])


# Hats and glasses (full colour), on the head canvas ---------------------------------------

def hats():
    I = g.INK
    S = f'stroke="{I}" stroke-width="6" stroke-linejoin="round" stroke-linecap="round"'
    top = CY - 92
    hats = {
        "cap": [f'<path d="M{CX - 84} {CY - 34} Q{CX - 84} {top - 20} {CX} {top - 22} Q{CX + 84} {top - 20} {CX + 84} {CY - 34} Z" fill="{g.CORAL}" {S}/>',
                f'<path d="M{CX + 40} {CY - 38} Q{CX + 120} {CY - 44} {CX + 132} {CY - 28} Q{CX + 100} {CY - 20} {CX + 40} {CY - 26} Z" fill="#E85A5A" {S}/>',
                f'<circle cx="{CX}" cy="{top - 22}" r="10" fill="{g.CORAL}" {S}/>'],
        "beanie": [f'<path d="M{CX - 88} {CY - 30} Q{CX - 90} {top - 30} {CX} {top - 32} Q{CX + 90} {top - 30} {CX + 88} {CY - 30} Z" fill="{g.TEAL}" {S}/>',
                   f'<rect x="{CX - 94}" y="{CY - 46}" width="188" height="34" rx="16" fill="{g.TEAL_DARK}" {S}/>',
                   f'<circle cx="{CX}" cy="{top - 36}" r="20" fill="{g.YELLOW}" {S}/>'],
        "crown": [f'<path d="M{CX - 70} {CY - 60} L{CX - 80} {top - 40} L{CX - 40} {top - 10} L{CX} {top - 56} L{CX + 40} {top - 10} L{CX + 80} {top - 40} L{CX + 70} {CY - 60} Z" fill="{g.YELLOW}" {S}/>',
                  f'<circle cx="{CX}" cy="{CY - 82}" r="10" fill="{g.CORAL}" {S}/>',
                  f'<circle cx="{CX - 44}" cy="{CY - 78}" r="8" fill="{g.BLUE}" {S}/>',
                  f'<circle cx="{CX + 44}" cy="{CY - 78}" r="8" fill="{g.GREEN}" {S}/>'],
        "bunny": [f'<ellipse cx="{CX - 40}" cy="{top - 44}" rx="22" ry="62" fill="#FFFFFF" {S} transform="rotate(-12 {CX - 40} {top - 44})"/>',
                  f'<ellipse cx="{CX + 40}" cy="{top - 44}" rx="22" ry="62" fill="#FFFFFF" {S} transform="rotate(12 {CX + 40} {top - 44})"/>',
                  f'<ellipse cx="{CX - 40}" cy="{top - 40}" rx="9" ry="40" fill="{g.PINK}" transform="rotate(-12 {CX - 40} {top - 44})"/>',
                  f'<ellipse cx="{CX + 40}" cy="{top - 40}" rx="9" ry="40" fill="{g.PINK}" transform="rotate(12 {CX + 40} {top - 44})"/>',
                  f'<path d="M{CX - 80} {CY - 44} Q{CX} {top - 20} {CX + 80} {CY - 44}" fill="none" stroke="{I}" stroke-width="14" stroke-linecap="round"/>',
                  f'<path d="M{CX - 80} {CY - 44} Q{CX} {top - 20} {CX + 80} {CY - 44}" fill="none" stroke="{g.PINK}" stroke-width="6" stroke-linecap="round"/>'],
        "party": [f'<path d="M{CX - 50} {CY - 70} L{CX + 6} {top - 86} L{CX + 54} {CY - 74} Z" fill="{g.PURPLE}" {S}/>',
                  f'<path d="M{CX - 28} {CY - 112} L{CX + 34} {CY - 116} M{CX - 10} {CY - 150} L{CX + 22} {CY - 152}" fill="none" stroke="{g.YELLOW}" stroke-width="10" stroke-linecap="round"/>',
                  f'<circle cx="{CX + 6}" cy="{top - 90}" r="14" fill="{g.YELLOW}" {S}/>'],
        "flowers": [f'<path d="M{CX - 86} {CY - 46} Q{CX} {top - 24} {CX + 86} {CY - 46}" fill="none" stroke="{g.GREEN_DARK}" stroke-width="10" stroke-linecap="round"/>']
                   + [f'<circle cx="{CX + dx}" cy="{CY - 46 - (80 - abs(dx)) * 0.5 - 6}" r="16" fill="{col}" {S}/>'
                      f'<circle cx="{CX + dx}" cy="{CY - 46 - (80 - abs(dx)) * 0.5 - 6}" r="6" fill="{g.YELLOW}"/>'
                      for dx, col in ((-70, g.PINK), (-36, "#FFFFFF"), (0, g.CORAL), (36, g.PURPLE), (70, g.PINK))],
        "pirate": [f'<path d="M{CX - 110} {CY - 50} Q{CX} {CY - 30} {CX + 110} {CY - 50} Q{CX + 70} {top - 50} {CX} {top - 40} Q{CX - 70} {top - 50} {CX - 110} {CY - 50} Z" fill="{I}" {S}/>',
                   f'<circle cx="{CX}" cy="{top + 8}" r="16" fill="#FFFFFF"/>',
                   f'<circle cx="{CX - 6}" cy="{top + 6}" r="4" fill="{I}"/><circle cx="{CX + 6}" cy="{top + 6}" r="4" fill="{I}"/>'],
        "wizard": [f'<path d="M{CX - 110} {CY - 48} Q{CX} {CY - 26} {CX + 110} {CY - 48} Q{CX} {CY - 70} {CX - 110} {CY - 48} Z" fill="{g.BLUE_DARK}" {S}/>',
                   f'<path d="M{CX - 66} {CY - 54} Q{CX - 20} {top - 60} {CX + 40} {top - 120} Q{CX + 30} {top - 40} {CX + 66} {CY - 54} Z" fill="{g.BLUE_DARK}" {S}/>',
                   g.star(CX + 4, CY - 110, 18, 8, g.YELLOW, 4), g.star(CX - 26, CY - 76, 11, 5, g.YELLOW, 3)],
    }
    hats.update(shop_items.hats(g))
    for name, parts in hats.items():
        head(f"hat_{name}", parts)


def glasses():
    I = g.INK
    (lx, ly), (rx, ry) = EYES
    S = f'stroke="{I}" stroke-width="6" stroke-linejoin="round"'
    bridge = f'<path d="M{lx + 22} {ly - 2} Q{CX} {ly - 12} {rx - 22} {ry - 2}" fill="none" {S}/>'
    arms = f'<path d="M{lx - 26} {ly - 4} L{CX - R + 6} {ly - 8} M{rx + 26} {ry - 4} L{CX + R - 6} {ry - 8}" fill="none" {S}/>'
    shapes = {
        "round": [f'<circle cx="{x}" cy="{y}" r="25" fill="#FFFFFF" fill-opacity="0.15" stroke="{g.CORAL}" stroke-width="7"/>' for x, y in EYES] + [bridge, arms],
        "star": [g.star(x, y + 2, 34, 18, g.YELLOW, 0).replace('fill="', 'fill-opacity="0.35" fill="').replace("/>", f' stroke="{g.YELLOW_DARK}" stroke-width="6" stroke-linejoin="round"/>') for x, y in EYES] + [bridge, arms],
        "heart": [f'<path d="M{x} {y + 24} Q{x - 32} {y + 2} {x - 28} {y - 12} Q{x - 22} {y - 26} {x} {y - 12} Q{x + 22} {y - 26} {x + 28} {y - 12} Q{x + 32} {y + 2} {x} {y + 24} Z" '
                  f'fill="{g.PINK}" fill-opacity="0.35" stroke="#E85A9A" stroke-width="6"/>' for x, y in EYES] + [bridge, arms],
        "goggles": [f'<rect x="{lx - 34}" y="{ly - 22}" width="{rx - lx + 68}" height="44" rx="22" fill="{g.SKY}" fill-opacity="0.45" stroke="{g.BLUE_DARK}" stroke-width="7"/>',
                    f'<path d="M{CX - R + 2} {ly} L{lx - 34} {ly} M{rx + 34} {ry} L{CX + R - 2} {ry}" fill="none" stroke="{g.BLUE_DARK}" stroke-width="10"/>'],
        "shades": [f'<path d="M{x - 28} {y - 16} L{x + 28} {y - 16} Q{x + 28} {y + 22} {x} {y + 22} Q{x - 28} {y + 22} {x - 28} {y - 16} Z" fill="{I}" {S}/>' for x, y in EYES]
                  + [f'<path d="M{lx + 28} {ly - 14} L{rx - 28} {ry - 14}" fill="none" {S}/>', arms],
    }
    shapes.update(shop_items.glasses(g))
    for name, parts in shapes.items():
        head(f"glasses_{name}", parts)


# Bodies (recoloured layers per pose) -------------------------------------------------------

def bodies():
    poses = {
        "stand": dict(legs=([(136, 330), (134, 480)], [(204, 330), (206, 480)]), shoes=((124, 500), (216, 500)),
                      shorts="M92 250 L248 250 L256 350 L178 350 L170 320 L162 350 L84 350 Z",
                      shirt="M96 70 Q170 50 244 70 Q262 160 254 268 Q170 284 86 268 Q78 160 96 70 Z",
                      arms=([(104, 92), (74, 180), (70, 262)], [(236, 92), (266, 180), (270, 262)])),
        "wave": dict(legs=([(136, 330), (134, 480)], [(204, 330), (206, 480)]), shoes=((124, 500), (216, 500)),
                     shorts="M92 250 L248 250 L256 350 L178 350 L170 320 L162 350 L84 350 Z",
                     shirt="M96 70 Q170 50 244 70 Q262 160 254 268 Q170 284 86 268 Q78 160 96 70 Z",
                     arms=([(104, 92), (74, 180), (70, 262)], [(236, 92), (290, 40), (300, -20)])),
        "seated": dict(legs=([(128, 300), (126, 372)], [(212, 300), (214, 372)]), shoes=((120, 390), (220, 390)),
                       shorts="M78 238 L262 238 Q276 300 250 312 L190 312 L170 290 L150 312 L90 312 Q64 300 78 238 Z",
                       shirt="M96 70 Q170 50 244 70 Q262 150 256 250 Q170 266 84 250 Q78 150 96 70 Z",
                       arms=([(104, 92), (72, 168), (70, 226)], [(236, 92), (268, 168), (270, 226)])),
    }
    for pose, d in poses.items():
        body("skin", pose, [f'<rect x="150" y="20" width="40" height="50" rx="10" fill="{WHITE}" {st()}/>', limb(d["legs"][0], 38), limb(d["legs"][1], 38)])
        body("shorts", pose, [p(d["shorts"])])
        body("shoes", pose, [e(x, y, 36, 19) for x, y in d["shoes"]])
        body("shirt", pose, [p(d["shirt"]), p("M140 64 Q170 92 200 64", "none", 5),
                             p("M110 120 Q120 200 112 250", "none", 0, f'stroke="{SHADE}" stroke-width="10" stroke-linecap="round"'),
                             limb(d["arms"][0], 40), limb(d["arms"][1], 40)])
        body("hands", pose, [c(*d["arms"][0][-1], 22), c(*d["arms"][1][-1], 22)])


# Shirt pictures and things to hold (full colour) -------------------------------------------

def emblems():
    I = g.INK
    S = f'stroke="{I}" stroke-width="5" stroke-linejoin="round" stroke-linecap="round"'
    items = {
        "star": [g.star(60, 64, 46, 20, g.YELLOW, 5)],
        "heart": [f'<path d="M60 102 Q14 70 16 44 Q20 18 44 20 Q56 22 60 38 Q64 22 76 20 Q100 18 104 44 Q106 70 60 102 Z" fill="{g.CORAL}" {S}/>'],
        "dino": [f'<path d="M14 92 Q20 60 46 58 L62 30 Q70 16 86 22 Q100 28 96 42 L84 46 L80 62 Q100 70 104 92 Z" fill="{g.GREEN}" {S}/>',
                 f'<circle cx="84" cy="32" r="4" fill="{I}"/>', f'<path d="M30 60 L36 48 L42 58 L48 46 L54 56" fill="none" {S}/>'],
        "rocket": [f'<path d="M60 12 Q92 40 86 90 L34 90 Q28 40 60 12 Z" fill="#FFFFFF" {S}/>', f'<circle cx="60" cy="52" r="12" fill="{g.SKY}" {S}/>',
                   f'<path d="M34 70 L16 100 L36 94 Z M86 70 L104 100 L84 94 Z" fill="{g.CORAL}" {S}/>', f'<path d="M48 92 L60 114 L72 92 Z" fill="{g.ORANGE}" {S}/>'],
        "rainbow": [f'<path d="M{60 - r} 96 A{r} {r} 0 0 1 {60 + r} 96" fill="none" stroke="{col}" stroke-width="9" stroke-linecap="round"/>'
                    for r, col in ((48, g.CORAL), (39, g.ORANGE), (30, g.YELLOW), (21, g.GREEN), (12, g.BLUE))],
        "eye": [g.path(g.eye_shape(60, 60, 48, 22), "#FFFFFF", 5), f'<circle cx="60" cy="60" r="18" fill="{g.BLUE}" {S}/>', f'<circle cx="60" cy="60" r="8" fill="{I}"/>'],
        "paw": [f'<ellipse cx="60" cy="74" rx="26" ry="22" fill="{g.WOOD_DARK}" {S}/>'] + [f'<circle cx="{x}" cy="{y}" r="11" fill="{g.WOOD_DARK}" {S}/>' for x, y in ((28, 44), (48, 28), (72, 28), (92, 44))],
    }
    items.update(shop_items.emblems(g))
    for name, parts in items.items():
        g.write(f"Avatar/emblem_{name}.svg", 120, 120, parts)


def held_items():
    """Things to hold, on a 200x300 canvas. The hand grips the point (100, 250)."""
    I = g.INK
    S = f'stroke="{I}" stroke-width="6" stroke-linejoin="round" stroke-linecap="round"'
    items = {
        "balloon": [f'<path d="M100 250 Q86 200 104 150" fill="none" stroke="{I}" stroke-width="4"/>',
                    f'<ellipse cx="104" cy="80" rx="62" ry="72" fill="{g.CORAL}" {S}/>', f'<path d="M94 150 L114 150 L104 138 Z" fill="{g.CORAL}" {S}/>',
                    '<ellipse cx="80" cy="56" rx="12" ry="20" fill="#FFFFFF" fill-opacity="0.6" transform="rotate(24 80 56)"/>'],
        "teddy": [f'<circle cx="64" cy="128" r="22" fill="{g.WOOD}" {S}/>', f'<circle cx="136" cy="128" r="22" fill="{g.WOOD}" {S}/>',
                  f'<ellipse cx="100" cy="236" rx="56" ry="50" fill="{g.WOOD}" {S}/>', f'<circle cx="100" cy="166" r="50" fill="{g.WOOD}" {S}/>',
                  f'<ellipse cx="100" cy="184" rx="20" ry="14" fill="#F3D2A8" stroke="{I}" stroke-width="4"/>', f'<circle cx="100" cy="178" r="6" fill="{I}"/>',
                  f'<circle cx="82" cy="158" r="5" fill="{I}"/><circle cx="118" cy="158" r="5" fill="{I}"/>'],
        "wand": [f'<path d="M100 270 L100 110" fill="none" stroke="{I}" stroke-width="16" stroke-linecap="round"/>',
                 f'<path d="M100 270 L100 110" fill="none" stroke="{g.PURPLE}" stroke-width="6" stroke-linecap="round"/>', g.star(100, 82, 46, 20, g.YELLOW, 6),
                 f'<path d="M150 40 L158 48 M44 52 L36 44 M160 110 L170 112" fill="none" stroke="{g.YELLOW}" stroke-width="6" stroke-linecap="round"/>'],
        "flag": [f'<path d="M100 276 L100 40" fill="none" stroke="{I}" stroke-width="14" stroke-linecap="round"/>', f'<path d="M100 276 L100 40" fill="none" stroke="{g.WOOD}" stroke-width="5"/>',
                 f'<path d="M104 44 L186 70 L104 100 Z" fill="{g.GREEN}" {S}/>', g.star(132, 72, 12, 5, g.YELLOW, 3)],
        "icecream": [f'<path d="M70 150 L130 150 L100 270 Z" fill="{g.WOOD}" {S}/>', f'<path d="M80 176 L118 200 M76 160 L124 184" fill="none" stroke="{g.WOOD_DARK}" stroke-width="4"/>',
                     f'<circle cx="100" cy="128" r="40" fill="{g.PINK}" {S}/>', f'<circle cx="100" cy="76" r="32" fill="#FFF3D6" {S}/>', f'<circle cx="104" cy="42" r="10" fill="{g.CORAL}" {S}/>'],
        "sunflower": [f'<path d="M100 270 Q96 200 100 130" fill="none" stroke="{g.GREEN_DARK}" stroke-width="10" stroke-linecap="round"/>',
                      f'<ellipse cx="124" cy="200" rx="22" ry="10" fill="{g.GREEN}" {S} transform="rotate(-30 124 200)"/>']
                     + [f'<ellipse cx="{100 + math.cos(math.radians(a)) * 40:.1f}" cy="{90 + math.sin(math.radians(a)) * 40:.1f}" rx="18" ry="30" fill="{g.YELLOW}" {S} transform="rotate({a + 90} {100 + math.cos(math.radians(a)) * 40:.1f} {90 + math.sin(math.radians(a)) * 40:.1f})"/>'
                        for a in range(0, 360, 40)]
                     + [f'<circle cx="100" cy="90" r="28" fill="{g.WOOD_DARK}" {S}/>'],
    }
    items.update(shop_items.held(g))
    for name, parts in items.items():
        g.write(f"Avatar/held_{name}.svg", 200, 300, parts)


def generate(module):
    global g
    g = module
    heads()
    hairs()
    eyes()
    brows()
    noses()
    mouths()
    hats()
    glasses()
    bodies()
    emblems()
    held_items()
