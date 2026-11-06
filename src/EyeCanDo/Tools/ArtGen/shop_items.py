"""More shop items for the avatar: hats, glasses, shirt pictures and things to hold.

Drawn on the same canvases as avatar_parts.py (hats and glasses on the head canvas in the
260x260 layout, shirt pictures on 120x120, held items on 200x300 with the grip at (100, 250)).
Each function returns {name: [svg parts]} and avatar_parts.py writes them with the rest.
"""

import math

CX, CY, R = 130, 140, 88
TOP = CY - 92
EYES = ((100, 150), (160, 150))


def _s(g, width=6):
    return f'stroke="{g.INK}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'


def hats(g):
    I, S = g.INK, _s(g)
    top = TOP
    return {
        "cowboy": [f'<path d="M{CX - 62} {CY - 50} Q{CX - 70} {top - 40} {CX - 30} {top - 34} Q{CX} {top - 18} {CX + 30} {top - 34} '
                   f'Q{CX + 70} {top - 40} {CX + 62} {CY - 50} Z" fill="{g.WOOD}" {S}/>',
                   f'<path d="M{CX - 60} {CY - 62} Q{CX} {CY - 74} {CX + 60} {CY - 62} L{CX + 61} {CY - 50} Q{CX} {CY - 62} {CX - 61} {CY - 50} Z" fill="{g.CORAL}" {S}/>',
                   f'<path d="M{CX - 128} {CY - 58} Q{CX - 120} {CY - 30} {CX} {CY - 34} Q{CX + 120} {CY - 30} {CX + 128} {CY - 58} '
                   f'Q{CX + 70} {CY - 48} {CX} {CY - 50} Q{CX - 70} {CY - 48} {CX - 128} {CY - 58} Z" fill="{g.WOOD_DARK}" {S}/>',
                   g.star(CX, CY - 56, 9, 4, g.YELLOW, 3)],
        "chef": [f'<circle cx="{CX - 46}" cy="{top - 4}" r="34" fill="#FFFFFF" {S}/>',
                 f'<circle cx="{CX + 46}" cy="{top - 4}" r="34" fill="#FFFFFF" {S}/>',
                 f'<circle cx="{CX}" cy="{top - 26}" r="42" fill="#FFFFFF" {S}/>',
                 f'<rect x="{CX - 70}" y="{CY - 80}" width="140" height="44" rx="10" fill="#FFFFFF" {S}/>',
                 f'<path d="M{CX - 30} {CY - 78} L{CX - 30} {CY - 40} M{CX + 30} {CY - 78} L{CX + 30} {CY - 40}" fill="none" stroke="#D6DCE6" stroke-width="5"/>'],
        "bow": [f'<path d="M{CX + 40} {top + 14} L{CX - 6} {top - 22} Q{CX - 16} {top + 14} {CX - 6} {top + 50} Z" fill="{g.PINK}" {S}/>',
                f'<path d="M{CX + 40} {top + 14} L{CX + 86} {top - 22} Q{CX + 96} {top + 14} {CX + 86} {top + 50} Z" fill="{g.PINK}" {S}/>',
                f'<circle cx="{CX + 40}" cy="{top + 14}" r="15" fill="#E85A9A" {S}/>'],
        "headphones": [f'<path d="M{CX - 96} {CY - 4} Q{CX - 100} {top - 46} {CX} {top - 48} Q{CX + 100} {top - 46} {CX + 96} {CY - 4}" fill="none" stroke="{I}" stroke-width="22" stroke-linecap="round"/>',
                       f'<path d="M{CX - 96} {CY - 4} Q{CX - 100} {top - 46} {CX} {top - 48} Q{CX + 100} {top - 46} {CX + 96} {CY - 4}" fill="none" stroke="{g.PURPLE}" stroke-width="10" stroke-linecap="round"/>',
                       f'<rect x="{CX - R - 26}" y="{CY - 28}" width="40" height="66" rx="18" fill="{g.TEAL}" {S}/>',
                       f'<rect x="{CX + R - 14}" y="{CY - 28}" width="40" height="66" rx="18" fill="{g.TEAL}" {S}/>'],
        "tiara": [f'<path d="M{CX - 60} {CY - 66} L{CX - 52} {top - 2} L{CX - 26} {top + 8} L{CX} {top - 26} L{CX + 26} {top + 8} L{CX + 52} {top - 2} L{CX + 60} {CY - 66} '
                  f'Q{CX} {CY - 80} {CX - 60} {CY - 66} Z" fill="#E8EDF5" {S}/>',
                  f'<circle cx="{CX}" cy="{top - 8}" r="10" fill="{g.PINK}" {S}/>',
                  f'<circle cx="{CX - 34}" cy="{top + 18}" r="7" fill="{g.SKY}" {S}/>',
                  f'<circle cx="{CX + 34}" cy="{top + 18}" r="7" fill="{g.SKY}" {S}/>'],
        "propeller": [f'<path d="M{CX - 86} {CY - 36} Q{CX - 86} {top - 22} {CX} {top - 24} Q{CX + 86} {top - 22} {CX + 86} {CY - 36} Z" fill="{g.YELLOW}" {S}/>',
                      f'<path d="M{CX} {top - 24} Q{CX - 30} {top} {CX - 34} {CY - 36} L{CX + 34} {CY - 36} Q{CX + 30} {top} {CX} {top - 24} Z" fill="{g.CORAL}" {S}/>',
                      f'<path d="M{CX} {top - 24} L{CX} {top - 50}" fill="none" stroke="{I}" stroke-width="8"/>',
                      f'<ellipse cx="{CX - 34}" cy="{top - 54}" rx="34" ry="11" fill="{g.BLUE}" {S}/>',
                      f'<ellipse cx="{CX + 34}" cy="{top - 54}" rx="34" ry="11" fill="{g.GREEN}" {S}/>',
                      f'<circle cx="{CX}" cy="{top - 54}" r="8" fill="{g.YELLOW}" {S}/>'],
        "catears": [f'<path d="M{CX - 80} {CY - 50} Q{CX} {top - 34} {CX + 80} {CY - 50}" fill="none" stroke="{I}" stroke-width="16" stroke-linecap="round"/>',
                    f'<path d="M{CX - 80} {CY - 50} Q{CX} {top - 34} {CX + 80} {CY - 50}" fill="none" stroke="{g.GREY_DARK}" stroke-width="6" stroke-linecap="round"/>',
                    f'<path d="M{CX - 78} {top + 22} L{CX - 74} {top - 52} L{CX - 26} {top - 14} Z" fill="{g.GREY_DARK}" {S}/>',
                    f'<path d="M{CX + 78} {top + 22} L{CX + 74} {top - 52} L{CX + 26} {top - 14} Z" fill="{g.GREY_DARK}" {S}/>',
                    f'<path d="M{CX - 66} {top + 4} L{CX - 64} {top - 30} L{CX - 40} {top - 12} Z" fill="{g.PINK}"/>',
                    f'<path d="M{CX + 66} {top + 4} L{CX + 64} {top - 30} L{CX + 40} {top - 12} Z" fill="{g.PINK}"/>'],
        "unicorn": [f'<path d="M{CX - 80} {CY - 50} Q{CX} {top - 34} {CX + 80} {CY - 50}" fill="none" stroke="{I}" stroke-width="16" stroke-linecap="round"/>',
                    f'<path d="M{CX - 80} {CY - 50} Q{CX} {top - 34} {CX + 80} {CY - 50}" fill="none" stroke="{g.PINK}" stroke-width="6" stroke-linecap="round"/>',
                    f'<path d="M{CX - 22} {top - 10} L{CX} {top - 96} L{CX + 22} {top - 10} Z" fill="{g.YELLOW}" {S}/>',
                    f'<path d="M{CX - 16} {top - 30} L{CX + 14} {top - 40} M{CX - 10} {top - 54} L{CX + 10} {top - 62}" fill="none" stroke="{g.YELLOW_DARK}" stroke-width="5" stroke-linecap="round"/>',
                    f'<circle cx="{CX - 50}" cy="{top - 2}" r="13" fill="{g.PURPLE}" {S}/>', f'<circle cx="{CX + 50}" cy="{top - 2}" r="13" fill="{g.SKY}" {S}/>'],
        "viking": [f'<path d="M{CX - 70} {top - 10} Q{CX - 120} {top - 20} {CX - 112} {top - 78} Q{CX - 96} {top - 36} {CX - 60} {top - 30} Z" fill="#FFF6E0" {S}/>',
                   f'<path d="M{CX + 70} {top - 10} Q{CX + 120} {top - 20} {CX + 112} {top - 78} Q{CX + 96} {top - 36} {CX + 60} {top - 30} Z" fill="#FFF6E0" {S}/>',
                   f'<path d="M{CX - 88} {CY - 40} Q{CX - 88} {top - 40} {CX} {top - 42} Q{CX + 88} {top - 40} {CX + 88} {CY - 40} Z" fill="{g.GREY}" {S}/>',
                   f'<rect x="{CX - 94}" y="{CY - 56}" width="188" height="26" rx="12" fill="{g.YELLOW}" {S}/>',
                   f'<path d="M{CX} {top - 40} L{CX} {CY - 56}" fill="none" stroke="{g.GREY_DARK}" stroke-width="6"/>'],
        "bucket": [f'<path d="M{CX - 70} {CY - 50} L{CX - 58} {top - 30} Q{CX} {top - 42} {CX + 58} {top - 30} L{CX + 70} {CY - 50} Z" fill="{g.GREEN}" {S}/>',
                   f'<path d="M{CX - 112} {CY - 30} Q{CX - 96} {CY - 62} {CX} {CY - 62} Q{CX + 96} {CY - 62} {CX + 112} {CY - 30} '
                   f'Q{CX} {CY - 46} {CX - 112} {CY - 30} Z" fill="{g.GREEN_DARK}" {S}/>',
                   f'<circle cx="{CX - 30}" cy="{top - 2}" r="7" fill="{g.YELLOW}"/><circle cx="{CX + 24}" cy="{top - 14}" r="7" fill="{g.YELLOW}"/>'
                   f'<circle cx="{CX + 4}" cy="{top + 20}" r="7" fill="{g.YELLOW}"/>'],
    }


def glasses(g):
    I = g.INK
    (lx, ly), (rx, ry) = EYES
    S = f'stroke="{I}" stroke-width="6" stroke-linejoin="round"'
    bridge = f'<path d="M{lx + 24} {ly - 2} Q{CX} {ly - 12} {rx - 24} {ry - 2}" fill="none" {S}/>'
    arms = f'<path d="M{lx - 28} {ly - 4} L{CX - R + 6} {ly - 8} M{rx + 28} {ry - 4} L{CX + R - 6} {ry - 8}" fill="none" {S}/>'
    lens = 'fill="#FFFFFF" fill-opacity="0.15"'
    return {
        "square": [f'<rect x="{x - 27}" y="{y - 21}" width="54" height="42" rx="9" {lens} stroke="{g.BLUE_DARK}" stroke-width="7"/>' for x, y in EYES] + [bridge, arms],
        "cateye": [f'<path d="M{x - 26 * d} {y - 18} Q{x} {y - 22} {x + 30 * d} {y - 26} Q{x + 26 * d} {y + 20} {x} {y + 20} Q{x - 28 * d} {y + 18} {x - 26 * d} {y - 18} Z" '
                   f'{lens} stroke="{g.PURPLE}" stroke-width="7" stroke-linejoin="round"/>' for (x, y), d in ((EYES[0], -1), (EYES[1], 1))] + [bridge, arms],
        "threed": [f'<rect x="{lx - 28}" y="{ly - 20}" width="56" height="40" rx="6" fill="{g.CORAL}" fill-opacity="0.4" stroke="#FFFFFF" stroke-width="8"/>',
                   f'<rect x="{rx - 28}" y="{ry - 20}" width="56" height="40" rx="6" fill="{g.SKY}" fill-opacity="0.55" stroke="#FFFFFF" stroke-width="8"/>',
                   f'<rect x="{lx - 33}" y="{ly - 25}" width="{rx - lx + 66}" height="50" rx="10" fill="none" stroke="{I}" stroke-width="3"/>', arms],
        "sparkle": [f'<circle cx="{x}" cy="{y}" r="25" {lens} stroke="{g.YELLOW_DARK}" stroke-width="7"/>' for x, y in EYES]
                   + [bridge, arms, g.star(lx - 24, ly - 24, 13, 5, g.YELLOW, 3), g.star(rx + 24, ry - 24, 13, 5, g.YELLOW, 3)],
        "mask": [f'<path d="M{CX - 84} {ly - 6} Q{CX - 70} {ly - 34} {CX} {ly - 22} Q{CX + 70} {ly - 34} {CX + 84} {ly - 6} Q{CX + 74} {ly + 30} {CX + 20} {ly + 22} '
                 f'Q{CX} {ly + 14} {CX - 20} {ly + 22} Q{CX - 74} {ly + 30} {CX - 84} {ly - 6} Z '
                 f'M{lx - 18} {ly} a18 13 0 1 0 36 0 a18 13 0 1 0 -36 0 Z M{rx - 18} {ry} a18 13 0 1 0 36 0 a18 13 0 1 0 -36 0 Z" '
                 f'fill="{g.CORAL}" fill-rule="evenodd" {S}/>'],
        "aviator": [f'<path d="M{x - 28} {y - 18} L{x + 28} {y - 18} Q{x + 30} {y + 10} {x + 6 * d} {y + 22} Q{x - 26} {y + 16} {x - 28} {y - 18} Z" '
                    f'fill="{g.BLUE_DARK}" fill-opacity="0.45" stroke="{g.YELLOW_DARK}" stroke-width="5" stroke-linejoin="round"/>' for (x, y), d in ((EYES[0], -1), (EYES[1], 1))]
                   + [f'<path d="M{lx + 28} {ly - 16} L{rx - 28} {ry - 16}" fill="none" stroke="{g.YELLOW_DARK}" stroke-width="5"/>', arms],
    }


def emblems(g):
    I = g.INK
    S = f'stroke="{I}" stroke-width="5" stroke-linejoin="round" stroke-linecap="round"'
    sun_rays = "".join(f'<path d="M{60 + math.cos(math.radians(a)) * 34:.1f} {60 + math.sin(math.radians(a)) * 34:.1f} '
                       f'L{60 + math.cos(math.radians(a)) * 52:.1f} {60 + math.sin(math.radians(a)) * 52:.1f}" fill="none" stroke="{g.ORANGE}" stroke-width="9" stroke-linecap="round"/>'
                       for a in range(0, 360, 45))
    petals = [f'<circle cx="{60 + math.cos(math.radians(a)) * 26:.1f}" cy="{60 + math.sin(math.radians(a)) * 26:.1f}" r="19" fill="{g.PINK}" {S}/>' for a in range(-90, 270, 72)]
    return {
        "moon": [f'<path d="M72 12 A48 48 0 1 0 108 82 A38 38 0 0 1 72 12 Z" fill="{g.YELLOW}" {S}/>', g.star(86, 30, 10, 4, "#FFFFFF", 3)],
        "sun": [sun_rays, f'<circle cx="60" cy="60" r="28" fill="{g.YELLOW}" {S}/>',
                f'<circle cx="51" cy="56" r="4" fill="{I}"/><circle cx="69" cy="56" r="4" fill="{I}"/>', f'<path d="M50 68 Q60 76 70 68" fill="none" {S}/>'],
        "flower": petals + [f'<circle cx="60" cy="60" r="16" fill="{g.YELLOW}" {S}/>'],
        "cat": [f'<path d="M24 58 L26 16 L52 38 Z M96 58 L94 16 L68 38 Z" fill="{g.ORANGE}" {S}/>', f'<ellipse cx="60" cy="68" rx="42" ry="36" fill="{g.ORANGE}" {S}/>',
                f'<circle cx="46" cy="62" r="5" fill="{I}"/><circle cx="74" cy="62" r="5" fill="{I}"/>', f'<path d="M56 76 L64 76 L60 81 Z" fill="{g.PINK}" {S}/>',
                f'<path d="M14 74 L38 76 M14 88 L38 82 M106 74 L82 76 M106 88 L82 82" fill="none" stroke="{I}" stroke-width="3" stroke-linecap="round"/>'],
        "fish": [f'<path d="M84 60 L112 36 L112 84 Z" fill="{g.ORANGE}" {S}/>', f'<ellipse cx="54" cy="60" rx="42" ry="28" fill="{g.SKY}" {S}/>',
                 f'<circle cx="34" cy="54" r="6" fill="{I}"/>', f'<path d="M58 40 Q66 60 58 80" fill="none" {S}/>'],
        "bolt": [f'<path d="M70 8 L28 66 L56 66 L46 112 L92 48 L64 48 Z" fill="{g.YELLOW}" {S}/>'],
        "music": [f'<path d="M44 86 L44 26 L96 14 L96 74" fill="none" stroke="{I}" stroke-width="9" stroke-linecap="round" stroke-linejoin="round"/>',
                  f'<ellipse cx="32" cy="88" rx="16" ry="12" fill="{g.PURPLE}" {S}/>', f'<ellipse cx="84" cy="76" rx="16" ry="12" fill="{g.PURPLE}" {S}/>'],
        "planet": [f'<circle cx="60" cy="60" r="32" fill="{g.PURPLE}" {S}/>',
                   f'<ellipse cx="60" cy="62" rx="56" ry="16" fill="none" stroke="{I}" stroke-width="12" transform="rotate(-18 60 62)"/>',
                   f'<ellipse cx="60" cy="62" rx="56" ry="16" fill="none" stroke="{g.YELLOW}" stroke-width="5" transform="rotate(-18 60 62)"/>',
                   f'<path d="M30 52 A32 32 0 0 1 90 52" fill="{g.PURPLE}" stroke="none"/>'],
        "butterfly": [f'<ellipse cx="36" cy="42" rx="26" ry="22" fill="{g.PINK}" {S}/>', f'<ellipse cx="84" cy="42" rx="26" ry="22" fill="{g.PINK}" {S}/>',
                      f'<ellipse cx="40" cy="80" rx="20" ry="18" fill="{g.PURPLE}" {S}/>', f'<ellipse cx="80" cy="80" rx="20" ry="18" fill="{g.PURPLE}" {S}/>',
                      f'<rect x="54" y="28" width="12" height="70" rx="6" fill="{I}"/>', f'<path d="M56 30 L46 14 M64 30 L74 14" fill="none" {S}/>'],
        "smile": [f'<circle cx="60" cy="60" r="46" fill="{g.YELLOW}" {S}/>', f'<circle cx="44" cy="50" r="6" fill="{I}"/><circle cx="76" cy="50" r="6" fill="{I}"/>',
                  f'<path d="M36 70 Q60 96 84 70" fill="none" {S}/>'],
    }


def held(g):
    I = g.INK
    S = f'stroke="{I}" stroke-width="6" stroke-linejoin="round" stroke-linecap="round"'

    def stick(top, color="#FFFFFF", width=10):
        return [f'<path d="M100 272 L100 {top}" fill="none" stroke="{I}" stroke-width="{width + 10}" stroke-linecap="round"/>',
                f'<path d="M100 272 L100 {top}" fill="none" stroke="{color}" stroke-width="{width}" stroke-linecap="round"/>']

    swirl = " ".join(f"{100 + math.cos(t) * t * 4.6:.1f},{92 + math.sin(t) * t * 4.6:.1f}" for t in [i * 0.25 for i in range(4, 44)])
    return {
        "lollipop": stick(140) + [f'<circle cx="100" cy="92" r="54" fill="{g.PINK}" {S}/>',
                                  f'<polyline points="{swirl}" fill="none" stroke="#FFFFFF" stroke-width="9" stroke-linecap="round"/>'],
        "kite": [f'<path d="M100 250 Q70 200 110 170 Q140 150 120 136" fill="none" stroke="{I}" stroke-width="4"/>',
                 f'<path d="M120 10 L170 70 L120 136 L70 70 Z" fill="{g.CORAL}" {S}/>', f'<path d="M120 10 L120 136 M70 70 L170 70" fill="none" stroke="{I}" stroke-width="4"/>',
                 f'<path d="M120 10 L170 70 L120 70 Z M70 70 L120 136 L120 70 Z" fill="{g.YELLOW}"/>',
                 f'<path d="M120 10 L170 70 L120 136 L70 70 Z" fill="none" {S}/>',
                 f'<path d="M108 186 L124 178 L120 194 Z M98 212 L114 206 L108 222 Z" fill="{g.BLUE}" {S}/>'],
        "trophy": [f'<rect x="62" y="236" width="76" height="32" rx="6" fill="{g.WOOD_DARK}" {S}/>', f'<rect x="88" y="186" width="24" height="54" fill="{g.YELLOW_DARK}" {S}/>',
                   f'<path d="M58 116 Q28 116 30 140 Q34 166 66 164 M142 116 Q172 116 170 140 Q166 166 134 164" fill="none" stroke="{I}" stroke-width="16" stroke-linecap="round"/>',
                   f'<path d="M58 116 Q28 116 30 140 Q34 166 66 164 M142 116 Q172 116 170 140 Q166 166 134 164" fill="none" stroke="{g.YELLOW}" stroke-width="6" stroke-linecap="round"/>',
                   f'<path d="M50 96 L150 96 Q148 192 100 192 Q52 192 50 96 Z" fill="{g.YELLOW}" {S}/>', g.star(100, 140, 22, 9, "#FFFFFF", 4)],
        "umbrella": [f'<path d="M100 120 L100 256 Q100 280 80 276 Q66 272 68 258" fill="none" stroke="{I}" stroke-width="16" stroke-linecap="round"/>',
                     f'<path d="M100 120 L100 256 Q100 280 80 276 Q66 272 68 258" fill="none" stroke="{g.WOOD}" stroke-width="6" stroke-linecap="round"/>',
                     f'<path d="M10 120 Q14 30 100 26 Q186 30 190 120 Q168 104 145 120 Q122 104 100 120 Q78 104 55 120 Q32 104 10 120 Z" fill="{g.BLUE}" {S}/>',
                     f'<path d="M100 26 Q70 60 55 120 M100 26 Q130 60 145 120" fill="none" stroke="{g.BLUE_DARK}" stroke-width="5"/>',
                     f'<circle cx="100" cy="22" r="8" fill="{g.YELLOW}" {S}/>'],
        "book": [f'<rect x="44" y="140" width="112" height="134" rx="10" fill="#FFFFFF" {S}/>', f'<rect x="36" y="132" width="112" height="134" rx="10" fill="{g.TEAL}" {S}/>',
                 f'<path d="M52 140 L52 258" fill="none" stroke="{g.TEAL_DARK}" stroke-width="6"/>', g.star(100, 196, 26, 11, g.YELLOW, 4)],
        "magnifier": stick(170, g.WOOD_DARK, 16) + [f'<circle cx="100" cy="112" r="56" fill="{g.SKY}" fill-opacity="0.6" stroke="{I}" stroke-width="18"/>',
                                                     f'<circle cx="100" cy="112" r="56" fill="none" stroke="{g.CORAL}" stroke-width="8"/>',
                                                     '<path d="M72 92 Q80 76 96 72" fill="none" stroke="#FFFFFF" stroke-width="8" stroke-linecap="round"/>'],
        "rocket": [f'<path d="M100 20 Q148 70 140 196 L60 196 Q52 70 100 20 Z" fill="#FFFFFF" {S}/>', f'<circle cx="100" cy="102" r="20" fill="{g.SKY}" {S}/>',
                   f'<path d="M60 150 L32 210 L62 198 Z M140 150 L168 210 L138 198 Z" fill="{g.CORAL}" {S}/>',
                   f'<path d="M76 198 L100 244 L124 198 Z" fill="{g.ORANGE}" {S}/>', f'<path d="M100 20 Q120 40 128 60 L72 60 Q80 40 100 20 Z" fill="{g.CORAL}" {S}/>'],
        "dino": [f'<path d="M40 250 Q20 200 50 170 Q24 150 30 132 Q60 140 70 160 L80 110 Q84 70 120 66 Q160 66 162 100 Q164 128 132 132 L130 178 Q172 210 150 262 Z" fill="{g.GREEN}" {S}/>',
                 f'<circle cx="136" cy="94" r="7" fill="{I}"/>', f'<path d="M140 116 Q150 120 156 114" fill="none" {S}/>',
                 f'<path d="M84 104 L72 92 L86 84 M80 132 L66 124 L80 114" fill="{g.YELLOW}" {S}/>', f'<ellipse cx="100" cy="220" rx="26" ry="30" fill="#CDEFC1" {S}/>'],
        "bubbles": stick(110, g.PURPLE, 8) + [f'<circle cx="100" cy="76" r="34" fill="none" stroke="{I}" stroke-width="16"/>',
                                              f'<circle cx="100" cy="76" r="34" fill="none" stroke="{g.PINK}" stroke-width="6"/>']
                   + [f'<circle cx="{x}" cy="{y}" r="{r}" fill="{g.SKY}" fill-opacity="0.45" stroke="{g.BLUE}" stroke-width="4"/>'
                      f'<circle cx="{x - r * 0.35:.1f}" cy="{y - r * 0.35:.1f}" r="{r * 0.2:.1f}" fill="#FFFFFF"/>'
                      for x, y, r in ((156, 40, 20), (170, 96, 13), (40, 30, 15), (150, 150, 10))],
        "guitar": [f'<path d="M100 150 L100 20" fill="none" stroke="{I}" stroke-width="24" stroke-linecap="round"/>',
                   f'<path d="M100 150 L100 20" fill="none" stroke="{g.WOOD_DARK}" stroke-width="12" stroke-linecap="round"/>',
                   f'<rect x="84" y="8" width="32" height="34" rx="8" fill="{g.WOOD_DARK}" {S}/>',
                   f'<path d="M100 130 Q60 128 62 168 Q64 190 52 214 Q40 270 100 272 Q160 270 148 214 Q136 190 138 168 Q140 128 100 130 Z" fill="{g.ORANGE}" {S}/>',
                   f'<circle cx="100" cy="200" r="18" fill="{I}"/>', f'<rect x="78" y="240" width="44" height="10" rx="4" fill="{g.WOOD_DARK}" {S}/>'],
    }
