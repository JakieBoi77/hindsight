#!/usr/bin/env python3
"""Generates the PoC's original placeholder artwork.

All illustrations are authored here as simple flat SVG so they are licence-clean,
easy to tweak, and trivially replaceable once real (purchased or commissioned) art
exists. Unity imports SVG natively (Vector Graphics module); see
Assets/_Project/Scripts/Editor/Import/ArtImportPostprocessor.cs for import settings.

UI shapes (rounded panels, circles, rings) are written as white PNGs so they can be
tinted with Image.color and 9-sliced. A tiny stdlib PNG encoder is used so the
script has no third-party dependencies.

Usage (from src/HindsightGame):  python Tools/ArtGen/generate_placeholder_art.py
"""

import math
import os
import struct
import zlib

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art")

# Palette ---------------------------------------------------------------------
INK = "#2E3A59"          # outlines
WHITE = "#FFFFFF"
DOC_SKIN = "#8D5A3B"
CHILD_SKIN = "#F2C69B"
HAIR_DARK = "#2B1B17"
HAIR_BROWN = "#7A4A2A"
TEAL = "#2BB3A3"
BLUE = "#4A90E2"
BLUE_DARK = "#2F6DB5"
YELLOW = "#FFC845"
CORAL = "#FF6B6B"
PURPLE = "#9B6BE0"
GREEN = "#5CBF73"
GREY = "#8E99AE"
GREY_DARK = "#56627A"
CHEEK = "#FF9B9B"
OUTLINE_W = 6


def svg(width, height, body):
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
        f'viewBox="0 0 {width} {height}">\n{body}\n</svg>\n'
    )


def write(rel_path, content):
    path = os.path.join(ART, rel_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(content)
    print("wrote", os.path.relpath(path, ROOT))


def stroke_attrs(width=OUTLINE_W, color=INK):
    return f'stroke="{color}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'


def limb(points, fill, width=52, outline=OUTLINE_W):
    """A thick rounded limb drawn as an outlined polyline (outline pass, then fill pass)."""
    pts = " ".join(f"{x},{y}" for x, y in points)
    return (
        f'<polyline points="{pts}" fill="none" stroke="{INK}" stroke-width="{width + 2 * outline}" '
        f'stroke-linejoin="round" stroke-linecap="round"/>\n'
        f'<polyline points="{pts}" fill="none" stroke="{fill}" stroke-width="{width}" '
        f'stroke-linejoin="round" stroke-linecap="round"/>'
    )


def hand(x, y, r=26, skin=DOC_SKIN):
    return f'<circle cx="{x}" cy="{y}" r="{r}" fill="{skin}" {stroke_attrs()}/>'


# Doctor ----------------------------------------------------------------------
def doctor(pose):
    w, h = 500, 900
    parts = []

    # Legs and shoes.
    parts.append(f'<rect x="185" y="640" width="55" height="200" rx="20" fill="#3D4A6B" {stroke_attrs()}/>')
    parts.append(f'<rect x="260" y="640" width="55" height="200" rx="20" fill="#3D4A6B" {stroke_attrs()}/>')
    parts.append(f'<ellipse cx="205" cy="850" rx="50" ry="24" fill="{INK}"/>')
    parts.append(f'<ellipse cx="295" cy="850" rx="50" ry="24" fill="{INK}"/>')

    # Arms behind the coat for down poses are drawn after the coat so they read clearly.
    arms = {
        "smile": ([(165, 360), (140, 480), (135, 580)], [(335, 360), (360, 480), (365, 580)]),
        "wave": ([(165, 360), (140, 480), (135, 580)], [(335, 360), (420, 280), (435, 160)]),
        "point": ([(165, 360), (140, 480), (135, 580)], [(335, 360), (420, 360), (470, 330)]),
        "cheer": ([(165, 360), (85, 260), (70, 150)], [(335, 360), (415, 260), (430, 150)]),
    }[pose]

    # Coat (white) over teal scrubs.
    parts.append(
        f'<path d="M160 330 Q250 300 340 330 L385 700 Q250 730 115 700 Z" fill="{WHITE}" {stroke_attrs()}/>'
    )
    parts.append(f'<path d="M215 330 L250 420 L285 330 Z" fill="{TEAL}" {stroke_attrs(4)}/>')
    parts.append(f'<path d="M215 330 L250 470 L232 700" fill="none" {stroke_attrs(4)}/>')
    parts.append(f'<path d="M285 330 L250 470 L268 700" fill="none" {stroke_attrs(4)}/>')
    # Pocket with pens and a name badge with an eye icon.
    parts.append(f'<rect x="290" y="480" width="60" height="55" rx="8" fill="{WHITE}" {stroke_attrs(4)}/>')
    parts.append(f'<rect x="300" y="455" width="10" height="40" rx="4" fill="{BLUE}"/>')
    parts.append(f'<rect x="318" y="460" width="10" height="35" rx="4" fill="{CORAL}"/>')
    parts.append(f'<rect x="160" y="450" width="70" height="40" rx="8" fill="{TEAL}" {stroke_attrs(4)}/>')
    parts.append(f'<ellipse cx="195" cy="470" rx="22" ry="12" fill="{WHITE}"/>')
    parts.append(f'<circle cx="195" cy="470" r="7" fill="{INK}"/>')

    # Arms (coat sleeves) and hands.
    for arm in arms:
        parts.append(limb(arm, WHITE))
        hx, hy = arm[-1]
        parts.append(hand(hx, hy))
    if pose == "point":
        parts.append(f'<rect x="478" y="318" width="22" height="16" rx="8" fill="{DOC_SKIN}" {stroke_attrs(4)}/>')

    # Neck and head.
    parts.append(f'<rect x="225" y="280" width="50" height="60" fill="{DOC_SKIN}" {stroke_attrs()}/>')
    parts.append(f'<circle cx="250" cy="95" r="48" fill="{HAIR_DARK}" {stroke_attrs()}/>')  # bun
    parts.append(f'<circle cx="160" cy="215" r="18" fill="{DOC_SKIN}" {stroke_attrs()}/>')
    parts.append(f'<circle cx="340" cy="215" r="18" fill="{DOC_SKIN}" {stroke_attrs()}/>')
    parts.append(f'<circle cx="250" cy="205" r="95" fill="{DOC_SKIN}" {stroke_attrs()}/>')
    parts.append(
        f'<path d="M155 200 Q150 110 250 108 Q350 110 345 200 Q320 150 250 150 Q180 150 155 200 Z" '
        f'fill="{HAIR_DARK}" {stroke_attrs()}/>'
    )
    # Eyes behind round glasses.
    parts.append(f'<ellipse cx="215" cy="212" rx="10" ry="13" fill="{INK}"/>')
    parts.append(f'<ellipse cx="285" cy="212" rx="10" ry="13" fill="{INK}"/>')
    parts.append(f'<circle cx="219" cy="207" r="4" fill="{WHITE}"/>')
    parts.append(f'<circle cx="289" cy="207" r="4" fill="{WHITE}"/>')
    parts.append(f'<circle cx="215" cy="212" r="28" fill="none" {stroke_attrs(5)}/>')
    parts.append(f'<circle cx="285" cy="212" r="28" fill="none" {stroke_attrs(5)}/>')
    parts.append(f'<path d="M243 210 Q250 202 257 210" fill="none" {stroke_attrs(5)}/>')
    parts.append(f'<circle cx="195" cy="250" r="12" fill="{CHEEK}" fill-opacity="0.6"/>')
    parts.append(f'<circle cx="305" cy="250" r="12" fill="{CHEEK}" fill-opacity="0.6"/>')
    if pose == "cheer":
        parts.append(f'<path d="M218 252 Q250 300 282 252 Z" fill="{INK}" {stroke_attrs(4)}/>')
        parts.append(f'<path d="M232 270 Q250 285 268 270" fill="{CORAL}"/>')
    else:
        parts.append(f'<path d="M222 255 Q250 282 278 255" fill="none" {stroke_attrs(6)}/>')

    return svg(w, h, "\n".join(parts))


# Exam chair and seated child (same canvas so they overlay exactly) -------------
CHAIR_W, CHAIR_H = 600, 700


def exam_chair():
    p = []
    p.append(f'<ellipse cx="300" cy="660" rx="170" ry="28" fill="{GREY_DARK}" {stroke_attrs()}/>')
    p.append(f'<rect x="270" y="470" width="60" height="190" fill="{GREY}" {stroke_attrs()}/>')
    p.append(f'<rect x="200" y="575" width="200" height="30" rx="12" fill="{GREY_DARK}" {stroke_attrs()}/>')  # footrest
    p.append(f'<rect x="250" y="60" width="100" height="70" rx="30" fill="{BLUE_DARK}" {stroke_attrs()}/>')  # headrest
    p.append(f'<rect x="170" y="120" width="260" height="330" rx="60" fill="{BLUE}" {stroke_attrs()}/>')  # back
    p.append(f'<rect x="130" y="330" width="70" height="40" rx="18" fill="{BLUE_DARK}" {stroke_attrs()}/>')  # arms
    p.append(f'<rect x="400" y="330" width="70" height="40" rx="18" fill="{BLUE_DARK}" {stroke_attrs()}/>')
    p.append(f'<rect x="140" y="400" width="320" height="80" rx="34" fill="{BLUE}" {stroke_attrs()}/>')  # seat
    p.append(f'<path d="M200 180 Q300 160 400 180" fill="none" stroke="{WHITE}" stroke-opacity="0.35" stroke-width="14" stroke-linecap="round"/>')
    return svg(CHAIR_W, CHAIR_H, "\n".join(p))


def child_seated():
    p = []
    # Legs hanging over the seat, shoes on the footrest.
    p.append(limb([(255, 450), (250, 560)], CHILD_SKIN, width=40))
    p.append(limb([(345, 450), (350, 560)], CHILD_SKIN, width=40))
    p.append(f'<path d="M215 420 L385 420 L395 480 L205 480 Z" fill="{BLUE_DARK}" {stroke_attrs()}/>')  # shorts
    p.append(f'<ellipse cx="245" cy="575" rx="34" ry="18" fill="{CORAL}" {stroke_attrs()}/>')
    p.append(f'<ellipse cx="355" cy="575" rx="34" ry="18" fill="{CORAL}" {stroke_attrs()}/>')
    # Body.
    p.append(f'<path d="M220 300 Q300 280 380 300 L390 430 L210 430 Z" fill="{YELLOW}" {stroke_attrs()}/>')
    p.append(f'<path d="M265 300 Q300 325 335 300" fill="none" {stroke_attrs(4)}/>')
    # Arms resting on the chair's armrests.
    p.append(limb([(225, 315), (180, 345)], YELLOW, width=40))
    p.append(limb([(375, 315), (420, 345)], YELLOW, width=40))
    p.append(hand(168, 350, r=22, skin=CHILD_SKIN))
    p.append(hand(432, 350, r=22, skin=CHILD_SKIN))
    # Head.
    p.append(f'<circle cx="300" cy="215" r="80" fill="{CHILD_SKIN}" {stroke_attrs()}/>')
    p.append(
        f'<path d="M222 205 Q225 128 300 130 Q378 128 380 205 Q360 168 330 172 Q300 150 270 172 Q240 168 222 205 Z" '
        f'fill="{HAIR_BROWN}" {stroke_attrs()}/>'
    )
    p.append(f'<path d="M300 130 Q310 105 330 112" fill="none" stroke="{HAIR_BROWN}" stroke-width="12" stroke-linecap="round"/>')
    p.append(f'<ellipse cx="272" cy="220" rx="11" ry="14" fill="{INK}"/>')
    p.append(f'<ellipse cx="328" cy="220" rx="11" ry="14" fill="{INK}"/>')
    p.append(f'<circle cx="276" cy="215" r="4" fill="{WHITE}"/>')
    p.append(f'<circle cx="332" cy="215" r="4" fill="{WHITE}"/>')
    p.append(f'<circle cx="252" cy="248" r="11" fill="{CHEEK}" fill-opacity="0.6"/>')
    p.append(f'<circle cx="348" cy="248" r="11" fill="{CHEEK}" fill-opacity="0.6"/>')
    p.append(f'<path d="M278 255 Q300 275 322 255" fill="none" {stroke_attrs(6)}/>')
    return svg(CHAIR_W, CHAIR_H, "\n".join(p))


# Eye parts (shared 600x320 canvas for white, lid and outline) -------------------
EYE_W, EYE_H = 600, 320
ALMOND = "M20 160 Q300 -40 580 160 Q300 360 20 160 Z"


def eye_white():
    return svg(EYE_W, EYE_H, f'<path d="{ALMOND}" fill="#FCFCFD"/>')


def eye_outline():
    lashes = []
    for i in range(7):
        t = (i + 1) / 8
        # Point on the upper quadratic curve and its outward normal.
        x = (1 - t) ** 2 * 20 + 2 * (1 - t) * t * 300 + t ** 2 * 580
        y = (1 - t) ** 2 * 160 + 2 * (1 - t) * t * (-40) + t ** 2 * 160
        dx = 2 * (1 - t) * (300 - 20) + 2 * t * (580 - 300)
        dy = 2 * (1 - t) * (-40 - 160) + 2 * t * (160 + 40)
        length = math.hypot(dx, dy)
        nx, ny = dy / length, -dx / length
        lashes.append(
            f'<line x1="{x:.1f}" y1="{y:.1f}" x2="{x + nx * 28:.1f}" y2="{y + ny * 28:.1f}" {stroke_attrs(8)}/>'
        )
    body = f'<path d="{ALMOND}" fill="none" {stroke_attrs(12)}/>\n' + "\n".join(lashes)
    # Canvas is padded so the lashes are not clipped; keep the almond aligned with eye_white.
    return svg(EYE_W, EYE_H, body)


def eyelid():
    return svg(
        EYE_W,
        EYE_H,
        f'<rect x="0" y="0" width="{EYE_W}" height="{EYE_H}" fill="{CHILD_SKIN}"/>\n'
        f'<path d="M20 300 Q300 340 580 300" fill="none" {stroke_attrs(10)}/>',
    )


def eye_socket():
    w, h = 760, 480
    body = (
        f'<rect x="0" y="0" width="{w}" height="{h}" rx="140" fill="{CHILD_SKIN}"/>\n'
        f'<path d="M150 70 Q380 0 610 70" fill="none" stroke="{HAIR_BROWN}" stroke-width="30" stroke-linecap="round"/>\n'
        f'<circle cx="120" cy="420" r="40" fill="{CHEEK}" fill-opacity="0.45"/>\n'
        f'<circle cx="640" cy="420" r="40" fill="{CHEEK}" fill-opacity="0.45"/>'
    )
    return svg(w, h, body)


def iris():
    s = 260
    spokes = []
    for i in range(16):
        a = i * math.pi / 8
        spokes.append(
            f'<line x1="{130 + math.cos(a) * 60:.1f}" y1="{130 + math.sin(a) * 60:.1f}" '
            f'x2="{130 + math.cos(a) * 118:.1f}" y2="{130 + math.sin(a) * 118:.1f}" '
            f'stroke="#9FD0F5" stroke-opacity="0.55" stroke-width="7" stroke-linecap="round"/>'
        )
    body = (
        '<defs><radialGradient id="irisFill" cx="0.5" cy="0.5" r="0.5">'
        '<stop offset="0" stop-color="#6FB3E8"/><stop offset="1" stop-color="#2D6AA6"/>'
        '</radialGradient></defs>\n'
        f'<circle cx="130" cy="130" r="125" fill="url(#irisFill)" {stroke_attrs(8)}/>\n' + "\n".join(spokes)
    )
    return svg(s, s, body)


def pupil():
    return svg(260, 260, '<circle cx="130" cy="130" r="128" fill="#111522"/>')


def eye_highlight():
    return svg(100, 100, f'<circle cx="50" cy="50" r="46" fill="{WHITE}" fill-opacity="0.9"/>')


# Tools and props ---------------------------------------------------------------
def eye_drop_bottle():
    # Upside down (nozzle pointing down), as it is held over the eye. Red label = dilating drops.
    body = (
        f'<rect x="30" y="10" width="140" height="200" rx="40" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<rect x="30" y="80" width="140" height="70" fill="{CORAL}" {stroke_attrs(4)}/>\n'
        f'<circle cx="100" cy="115" r="18" fill="{WHITE}"/>\n'
        f'<path d="M60 205 L140 205 L112 300 L88 300 Z" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<rect x="90" y="298" width="20" height="40" rx="8" fill="{WHITE}" {stroke_attrs()}/>'
    )
    return svg(200, 350, body)


def drop():
    body = (
        '<path d="M40 6 Q78 62 70 80 Q60 104 40 104 Q20 104 10 80 Q2 62 40 6 Z" '
        f'fill="#7CC6FE" {stroke_attrs(5)}/>\n'
        f'<ellipse cx="28" cy="70" rx="7" ry="12" fill="{WHITE}" fill-opacity="0.8"/>'
    )
    return svg(80, 110, body)


def ophthalmoscope():
    ridges = "\n".join(
        f'<line x1="105" y1="{y}" x2="155" y2="{y}" stroke="{GREY}" stroke-width="6" stroke-linecap="round"/>'
        for y in range(230, 390, 22)
    )
    body = (
        f'<rect x="95" y="200" width="70" height="210" rx="22" fill="{INK}"/>\n{ridges}\n'
        f'<rect x="60" y="20" width="140" height="180" rx="40" fill="{GREY_DARK}" {stroke_attrs()}/>\n'
        f'<circle cx="130" cy="80" r="30" fill="#1B2233" {stroke_attrs(5)}/>\n'
        f'<circle cx="130" cy="80" r="14" fill="{YELLOW}"/>\n'
        f'<rect x="100" y="140" width="60" height="30" rx="10" fill="{GREY}"/>'
    )
    return svg(260, 420, body)


def light_glow():
    body = (
        '<defs><radialGradient id="glow" cx="0.5" cy="0.5" r="0.5">'
        '<stop offset="0" stop-color="#FFFBE0" stop-opacity="1"/>'
        '<stop offset="0.45" stop-color="#FFF2A8" stop-opacity="0.55"/>'
        '<stop offset="1" stop-color="#FFF2A8" stop-opacity="0"/>'
        '</radialGradient></defs>\n'
        '<circle cx="300" cy="300" r="300" fill="url(#glow)"/>'
    )
    return svg(600, 600, body)


def snowflake():
    arms = []
    for i in range(6):
        a = i * math.pi / 3
        x2, y2 = 100 + math.cos(a) * 80, 100 + math.sin(a) * 80
        arms.append(f'<line x1="100" y1="100" x2="{x2:.1f}" y2="{y2:.1f}" stroke="{BLUE}" stroke-width="12" stroke-linecap="round"/>')
        for side in (-1, 1):
            bx, by = 100 + math.cos(a) * 50, 100 + math.sin(a) * 50
            b = a + side * math.pi / 4
            arms.append(
                f'<line x1="{bx:.1f}" y1="{by:.1f}" x2="{bx + math.cos(b) * 26:.1f}" y2="{by + math.sin(b) * 26:.1f}" '
                f'stroke="{BLUE}" stroke-width="10" stroke-linecap="round"/>'
            )
    body = f'<circle cx="100" cy="100" r="96" fill="#E3F4FF"/>\n' + "\n".join(arms)
    return svg(200, 200, body)


def child_head_tilt():
    # Side profile with the head tilted back and an arrow showing the motion.
    head = (
        f'<g transform="rotate(-28 250 300)">'
        f'<rect x="215" y="300" width="70" height="120" fill="{CHILD_SKIN}" {stroke_attrs()}/>'
        f'<circle cx="250" cy="230" r="110" fill="{CHILD_SKIN}" {stroke_attrs()}/>'
        f'<path d="M355 240 Q395 255 360 280" fill="{CHILD_SKIN}" {stroke_attrs()}/>'
        f'<path d="M145 230 Q140 110 255 115 Q330 118 350 170 Q290 150 250 175 Q190 180 175 260 Z" fill="{HAIR_BROWN}" {stroke_attrs()}/>'
        f'<ellipse cx="315" cy="215" rx="16" ry="20" fill="{WHITE}" {stroke_attrs(4)}/>'
        f'<circle cx="322" cy="208" r="8" fill="{INK}"/>'
        f'<path d="M318 300 Q335 310 350 300" fill="none" {stroke_attrs(5)}/>'
        f'<circle cx="270" cy="280" r="14" fill="{CHEEK}" fill-opacity="0.6"/>'
        f'</g>'
    )
    arrow = (
        f'<path d="M430 300 Q450 170 360 80" fill="none" stroke="{TEAL}" stroke-width="18" stroke-linecap="round"/>'
        f'<path d="M330 70 L385 55 L375 110 Z" fill="{TEAL}" {stroke_attrs(4, TEAL)}/>'
    )
    return svg(500, 500, head + "\n" + arrow)


def sticker():
    scallops = []
    for i in range(16):
        a = i * math.pi / 8
        scallops.append(f'<circle cx="{250 + math.cos(a) * 190:.1f}" cy="{230 + math.sin(a) * 190:.1f}" r="38" fill="{YELLOW}"/>')
    body = (
        f'<path d="M170 330 L130 490 L190 455 L225 500 L250 360 Z" fill="{CORAL}" {stroke_attrs()}/>\n'
        f'<path d="M330 330 L370 490 L310 455 L275 500 L250 360 Z" fill="{CORAL}" {stroke_attrs()}/>\n'
        + "\n".join(scallops)
        + f'\n<circle cx="250" cy="230" r="190" fill="{YELLOW}"/>'
        f'\n<circle cx="250" cy="230" r="150" fill="{WHITE}" {stroke_attrs(8)}/>'
        f'\n<path d="M130 230 Q250 120 370 230 Q250 340 130 230 Z" fill="#FCFCFD" {stroke_attrs(8)}/>'
        f'\n<circle cx="250" cy="230" r="52" fill="{BLUE}" {stroke_attrs(6)}/>'
        f'\n<circle cx="250" cy="230" r="24" fill="#111522"/>'
        f'\n<circle cx="236" cy="214" r="10" fill="{WHITE}"/>'
        + star_path(250, 120, 34, 15, YELLOW)
    )
    return svg(500, 520, body)


def star_path(cx, cy, outer, inner, fill):
    pts = []
    for i in range(10):
        r = outer if i % 2 == 0 else inner
        a = -math.pi / 2 + i * math.pi / 5
        pts.append(f"{cx + math.cos(a) * r:.1f},{cy + math.sin(a) * r:.1f}")
    return f'\n<polygon points="{" ".join(pts)}" fill="{fill}" {stroke_attrs(5)}/>'


def padlock():
    body = (
        f'<path d="M38 60 L38 42 Q38 12 64 12 Q90 12 90 42 L90 60" fill="none" stroke="{GREY_DARK}" stroke-width="14" stroke-linecap="round"/>\n'
        f'<rect x="22" y="56" width="84" height="64" rx="14" fill="{YELLOW}" {stroke_attrs(5)}/>\n'
        f'<circle cx="64" cy="82" r="9" fill="{INK}"/>\n'
        f'<rect x="60" y="84" width="8" height="20" rx="3" fill="{INK}"/>'
    )
    return svg(128, 128, body)


def home_icon():
    body = (
        f'<path d="M64 14 L120 62 L104 62 L104 114 L24 114 L24 62 L8 62 Z" fill="{WHITE}" '
        f'stroke="{WHITE}" stroke-width="8" stroke-linejoin="round"/>\n'
        f'<rect x="52" y="76" width="24" height="38" rx="4" fill="{BLUE}"/>'
    )
    return svg(128, 128, body)


def bubble_tail():
    # Pointed speech-bubble tail. Its top overlaps the bubble's border so the white fill hides
    # the border there; only the two slanted edges are outlined.
    body = (
        f'<polygon points="14,0 84,0 2,88" fill="{WHITE}"/>\n'
        f'<polyline points="12.8,9 2,88 75.6,9" fill="none" stroke="{INK}" stroke-width="7" '
        f'stroke-linejoin="round" stroke-linecap="round"/>'
    )
    return svg(110, 90, body)


# Small round icons (level cards and quiz answers) -------------------------------
def icon_eye(pupil_r=26, iris_fill=BLUE, extra=""):
    return (
        f'<path d="M28 128 Q128 40 228 128 Q128 216 28 128 Z" fill="{WHITE}" {stroke_attrs(8)}/>\n'
        f'<circle cx="128" cy="128" r="46" fill="{iris_fill}" {stroke_attrs(6)}/>\n'
        f'<circle cx="128" cy="128" r="{pupil_r}" fill="#111522"/>\n'
        f'<circle cx="114" cy="112" r="9" fill="{WHITE}"/>\n' + extra
    )


def icon_level_eye_drops():
    body = (
        f'<rect x="88" y="20" width="80" height="110" rx="24" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<rect x="88" y="55" width="80" height="38" fill="{CORAL}" {stroke_attrs(4)}/>\n'
        f'<path d="M104 128 L152 128 L136 178 L120 178 Z" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<path d="M128 192 Q150 222 140 234 Q128 246 116 234 Q106 222 128 192 Z" fill="#7CC6FE" {stroke_attrs(5)}/>'
    )
    return svg(256, 256, body)


def icon_level_vision_test():
    body = (
        f'<rect x="48" y="16" width="160" height="224" rx="18" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<circle cx="128" cy="62" r="26" fill="{CORAL}"/>\n'
        f'<rect x="78" y="112" width="36" height="36" fill="{BLUE}"/>\n'
        f'<path d="M160 148 L160 120 L178 104 L196 120 L196 148 Z" fill="{GREEN}" transform="translate(-14 0)"/>\n'
        f'<circle cx="90" cy="194" r="13" fill="{PURPLE}"/>\n<circle cx="128" cy="194" r="13" fill="{YELLOW}"/>\n'
        f'<circle cx="166" cy="194" r="13" fill="{TEAL}"/>'
    )
    return svg(256, 256, body)


def icon_level_slit_lamp():
    body = (
        f'<rect x="40" y="200" width="176" height="36" rx="12" fill="{GREY_DARK}" {stroke_attrs()}/>\n'
        f'<rect x="116" y="100" width="24" height="104" fill="{GREY}" {stroke_attrs()}/>\n'
        f'<rect x="70" y="56" width="116" height="60" rx="22" fill="{TEAL}" {stroke_attrs()}/>\n'
        f'<circle cx="98" cy="86" r="14" fill="{INK}"/>\n<circle cx="158" cy="86" r="14" fill="{INK}"/>\n'
        f'<path d="M186 86 L230 70 L230 102 Z" fill="{YELLOW}" {stroke_attrs(4)}/>'
    )
    return svg(256, 256, body)


def icon_level_eye_pressure():
    swirl = (
        f'<path d="M150 40 Q200 30 210 60" fill="none" stroke="{BLUE}" stroke-width="10" stroke-linecap="round"/>\n'
        f'<path d="M160 70 Q215 70 226 98" fill="none" stroke="{BLUE}" stroke-width="10" stroke-linecap="round"/>\n'
        f'<circle cx="214" cy="40" r="8" fill="{BLUE}"/>'
    )
    return svg(256, 256, icon_eye(extra=swirl))


def icon_level_eye_photo():
    body = (
        f'<rect x="28" y="70" width="200" height="140" rx="26" fill="{PURPLE}" {stroke_attrs()}/>\n'
        f'<rect x="84" y="46" width="70" height="34" rx="10" fill="{PURPLE}" {stroke_attrs()}/>\n'
        f'<circle cx="128" cy="140" r="48" fill="{WHITE}" {stroke_attrs()}/>\n'
        f'<circle cx="128" cy="140" r="26" fill="{INK}"/>\n'
        f'<circle cx="196" cy="98" r="10" fill="{YELLOW}"/>'
    )
    return svg(256, 256, body)


def icon_level_glasses():
    body = (
        f'<circle cx="78" cy="138" r="48" fill="#E3F4FF" {stroke_attrs(12)}/>\n'
        f'<circle cx="178" cy="138" r="48" fill="#E3F4FF" {stroke_attrs(12)}/>\n'
        f'<path d="M118 128 Q128 112 138 128" fill="none" {stroke_attrs(10)}/>\n'
        f'<path d="M30 126 L12 100" fill="none" {stroke_attrs(10)}/>\n<path d="M226 126 L244 100" fill="none" {stroke_attrs(10)}/>'
    )
    return svg(256, 256, body)


def icon_quiz_big_pupil():
    arrows = "\n".join(
        f'<path d="M{x1} {y1} L{x2} {y2}" fill="none" stroke="{TEAL}" stroke-width="10" stroke-linecap="round"/>'
        for x1, y1, x2, y2 in [(128, 30, 128, 6), (128, 226, 128, 250), (40, 70, 22, 52), (216, 70, 234, 52)]
    )
    return svg(256, 256, icon_eye(pupil_r=40) + arrows)


def icon_quiz_rainbow_eye():
    body = (
        f'<path d="M28 128 Q128 40 228 128 Q128 216 28 128 Z" fill="{WHITE}" {stroke_attrs(8)}/>\n'
        f'<circle cx="128" cy="128" r="46" fill="{CORAL}"/>\n<circle cx="128" cy="128" r="36" fill="{YELLOW}"/>\n'
        f'<circle cx="128" cy="128" r="27" fill="{GREEN}"/>\n<circle cx="128" cy="128" r="19" fill="{PURPLE}"/>\n'
        f'<circle cx="128" cy="128" r="46" fill="none" {stroke_attrs(6)}/>\n<circle cx="128" cy="128" r="12" fill="#111522"/>'
    )
    return svg(256, 256, body)


def icon_quiz_sleepy():
    z = lambda x, y, s: (  # noqa: E731 - tiny path helper
        f'<path d="M{x} {y} L{x + s} {y} L{x} {y + s} L{x + s} {y + s}" fill="none" stroke="{PURPLE}" '
        f'stroke-width="{max(6, s // 5)}" stroke-linejoin="round" stroke-linecap="round"/>'
    )
    body = (
        f'<path d="M150 40 A90 90 0 1 0 220 170 A70 70 0 1 1 150 40 Z" fill="{YELLOW}" {stroke_attrs()}/>\n'
        + z(40, 40, 40) + "\n" + z(90, 96, 28) + "\n" + z(56, 150, 20)
    )
    return svg(256, 256, body)


def icon_quiz_eyes_shut():
    body = (
        f'<path d="M40 130 Q128 190 216 130" fill="none" {stroke_attrs(12)}/>\n'
        + "\n".join(
            f'<line x1="{x}" y1="{y}" x2="{x + dx}" y2="{y + 28}" {stroke_attrs(8)}/>'
            for x, y, dx in [(70, 150, -10), (105, 162, -4), (150, 162, 4), (186, 150, 10)]
        )
        + f'\n<path d="M40 90 L70 104 M216 90 L186 104 M128 66 L128 92" fill="none" stroke="{CORAL}" stroke-width="10" stroke-linecap="round"/>'
    )
    return svg(256, 256, body)


def icon_quiz_head_back():
    arrow = (
        f'<path d="M128 70 L128 12" fill="none" stroke="{TEAL}" stroke-width="14" stroke-linecap="round"/>\n'
        f'<path d="M100 36 L128 6 L156 36" fill="none" stroke="{TEAL}" stroke-width="14" stroke-linecap="round" stroke-linejoin="round"/>'
    )
    body = (
        f'<path d="M28 150 Q128 62 228 150 Q128 238 28 150 Z" fill="{WHITE}" {stroke_attrs(8)}/>\n'
        f'<circle cx="128" cy="124" r="40" fill="{BLUE}" {stroke_attrs(6)}/>\n<circle cx="128" cy="124" r="20" fill="#111522"/>\n'
        f'<circle cx="116" cy="112" r="8" fill="{WHITE}"/>\n' + arrow
    )
    return svg(256, 256, body)


def icon_quiz_wiggle():
    body = (
        icon_eye()
        + f'\n<path d="M10 70 Q24 50 10 30" fill="none" stroke="{CORAL}" stroke-width="10" stroke-linecap="round"/>'
        f'\n<path d="M246 70 Q232 50 246 30" fill="none" stroke="{CORAL}" stroke-width="10" stroke-linecap="round"/>'
        f'\n<path d="M10 226 Q24 206 10 186" fill="none" stroke="{CORAL}" stroke-width="10" stroke-linecap="round"/>'
        f'\n<path d="M246 226 Q232 206 246 186" fill="none" stroke="{CORAL}" stroke-width="10" stroke-linecap="round"/>'
    )
    return svg(256, 256, body)


# Backgrounds -------------------------------------------------------------------
def exam_room():
    w, h = 1920, 1080
    p = [
        f'<rect x="0" y="0" width="{w}" height="{h}" fill="#DDF1F7"/>',
        f'<rect x="0" y="0" width="{w}" height="70" fill="#CBE7F1"/>',
        f'<rect x="0" y="800" width="{w}" height="280" fill="#BFE3D2"/>',
        f'<rect x="0" y="790" width="{w}" height="22" fill="#9FCDB8"/>',
        # Window with sky and a cloud (left, behind the doctor's shoulder).
        f'<rect x="560" y="140" width="300" height="240" rx="18" fill="#FFFFFF" {stroke_attrs(8, "#9DB7CC")}/>',
        f'<rect x="580" y="160" width="260" height="200" rx="10" fill="#BDE3FF"/>',
        f'<ellipse cx="680" cy="250" rx="60" ry="26" fill="#FFFFFF"/>',
        f'<ellipse cx="720" cy="232" rx="40" ry="26" fill="#FFFFFF"/>',
        f'<rect x="706" y="160" width="8" height="200" fill="#FFFFFF"/>',
        # Child-friendly picture eye chart on the right wall.
        f'<rect x="1600" y="130" width="220" height="320" rx="16" fill="#FFFFFF" {stroke_attrs(6, "#9DB7CC")}/>',
        f'<circle cx="1710" cy="195" r="32" fill="{CORAL}" fill-opacity="0.8"/>',
        f'<rect x="1650" y="255" width="40" height="40" fill="{BLUE}" fill-opacity="0.8"/>',
        f'<circle cx="1750" cy="275" r="20" fill="{GREEN}" fill-opacity="0.8"/>',
        f'<circle cx="1660" cy="350" r="12" fill="{PURPLE}" fill-opacity="0.8"/>',
        f'<rect x="1698" y="338" width="24" height="24" fill="{YELLOW}"/>',
        f'<circle cx="1760" cy="350" r="12" fill="{TEAL}" fill-opacity="0.8"/>',
        f'<rect x="1650" y="395" width="120" height="8" rx="4" fill="#C8D6E2"/>',
        # Plant in the corner.
        f'<path d="M1780 800 L1800 700 L1880 700 L1900 800 Z" fill="{CORAL}" fill-opacity="0.85"/>',
        f'<ellipse cx="1840" cy="650" rx="40" ry="70" fill="{GREEN}" fill-opacity="0.85"/>',
        f'<ellipse cx="1800" cy="670" rx="26" ry="50" fill="{GREEN}" fill-opacity="0.85" transform="rotate(-25 1800 670)"/>',
        f'<ellipse cx="1880" cy="670" rx="26" ry="50" fill="{GREEN}" fill-opacity="0.85" transform="rotate(25 1880 670)"/>',
    ]
    return svg(w, h, "\n".join(p))


def title_background():
    w, h = 1920, 1080
    p = [
        '<defs><linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">'
        '<stop offset="0" stop-color="#8FD3FF"/><stop offset="1" stop-color="#E4F6FF"/></linearGradient></defs>',
        f'<rect x="0" y="0" width="{w}" height="{h}" fill="url(#sky)"/>',
        f'<ellipse cx="960" cy="1150" rx="1300" ry="260" fill="#BFE3D2"/>',
    ]
    for cx, cy, s in [(260, 220, 1.2), (1620, 180, 1.0), (1500, 520, 0.7), (380, 600, 0.8)]:
        p.append(
            f'<g transform="translate({cx} {cy}) scale({s})">'
            '<ellipse cx="0" cy="0" rx="120" ry="48" fill="#FFFFFF" fill-opacity="0.9"/>'
            '<ellipse cx="60" cy="-30" rx="70" ry="50" fill="#FFFFFF" fill-opacity="0.9"/>'
            '<ellipse cx="-50" cy="-20" rx="60" ry="40" fill="#FFFFFF" fill-opacity="0.9"/></g>'
        )
    for i, (x, y, color) in enumerate([(160, 900, YELLOW), (1780, 860, CORAL), (1760, 420, PURPLE), (150, 470, GREEN)]):
        p.append(star_path(x, y, 34 - i * 2, 15, color))
    return svg(w, h, "\n".join(p))



# PNG UI shapes -----------------------------------------------------------------
def write_png(rel_path, size, coverage, samples=4):
    """Rasterises a white shape; coverage(x, y) -> bool tests sub-pixel points."""
    width, height = size
    rows = []
    step = 1.0 / samples
    for py in range(height):
        row = bytearray([0])  # filter type 0
        for px in range(width):
            hits = 0
            for sy in range(samples):
                for sx in range(samples):
                    if coverage(px + (sx + 0.5) * step, py + (sy + 0.5) * step):
                        hits += 1
            alpha = round(255 * hits / (samples * samples))
            row += bytes((255, 255, 255, alpha))
        rows.append(bytes(row))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(b"".join(rows), 9))
    png += chunk(b"IEND", b"")
    path = os.path.join(ART, rel_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as handle:
        handle.write(png)
    print("wrote", os.path.relpath(path, ROOT))


def rounded_rect(size, radius):
    def inside(x, y):
        cx = min(max(x, radius), size - radius)
        cy = min(max(y, radius), size - radius)
        return (x - cx) ** 2 + (y - cy) ** 2 <= radius ** 2

    return inside


def circle(size):
    r = size / 2

    def inside(x, y):
        return (x - r) ** 2 + (y - r) ** 2 <= r ** 2

    return inside


def ring(size, thickness):
    r = size / 2

    def inside(x, y):
        d = math.hypot(x - r, y - r)
        return r - thickness <= d <= r

    return inside



def main():
    for pose in ("smile", "wave", "point", "cheer"):
        write(f"Characters/doctor_{pose}.svg", doctor(pose))
    write("Characters/child_seated.svg", child_seated())
    write("Characters/child_head_tilt.svg", child_head_tilt())

    write("Clinic/exam_chair.svg", exam_chair())
    write("Clinic/eye_drop_bottle.svg", eye_drop_bottle())
    write("Clinic/drop.svg", drop())
    write("Clinic/ophthalmoscope.svg", ophthalmoscope())
    write("Clinic/light_glow.svg", light_glow())
    write("Clinic/snowflake.svg", snowflake())

    write("Eye/eye_white.svg", eye_white())
    write("Eye/eye_outline.svg", eye_outline())
    write("Eye/eyelid.svg", eyelid())
    write("Eye/eye_socket.svg", eye_socket())
    write("Eye/iris.svg", iris())
    write("Eye/pupil.svg", pupil())
    write("Eye/eye_highlight.svg", eye_highlight())

    write("Rewards/sticker_brave_eyes.svg", sticker())
    write("UI/padlock.svg", padlock())
    write("UI/home.svg", home_icon())
    write("UI/bubble_tail.svg", bubble_tail())

    write("Icons/level_eye_drops.svg", icon_level_eye_drops())
    write("Icons/level_vision_test.svg", icon_level_vision_test())
    write("Icons/level_slit_lamp.svg", icon_level_slit_lamp())
    write("Icons/level_eye_pressure.svg", icon_level_eye_pressure())
    write("Icons/level_eye_photo.svg", icon_level_eye_photo())
    write("Icons/level_glasses.svg", icon_level_glasses())
    write("Icons/quiz_big_pupil.svg", icon_quiz_big_pupil())
    write("Icons/quiz_rainbow_eye.svg", icon_quiz_rainbow_eye())
    write("Icons/quiz_sleepy.svg", icon_quiz_sleepy())
    write("Icons/quiz_eyes_shut.svg", icon_quiz_eyes_shut())
    write("Icons/quiz_head_back.svg", icon_quiz_head_back())
    write("Icons/quiz_wiggle.svg", icon_quiz_wiggle())

    write("Backgrounds/exam_room.svg", exam_room())
    write("Backgrounds/title_background.svg", title_background())

    # "_9s" marks 9-slice sprites; the import postprocessor sets their borders.
    write_png("UI/panel_rounded_9s.png", (128, 128), rounded_rect(128, 48))
    write_png("UI/circle.png", (128, 128), circle(128))
    write_png("UI/ring.png", (256, 256), ring(256, 30))


if __name__ == "__main__":
    main()
