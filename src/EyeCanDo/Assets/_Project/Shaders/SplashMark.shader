// The Hindsight logo: a lamp's lens (a ringed disc) holding an H whose crossbar is a beam,
// thin at the left stem and widening to the right. Unlit (_Lit = 0) it is a quiet outline on a
// dark lens; lit, the lens is full of light and the H stands in it as a shadow.
// Drawn from signed distances, so it is sharp at any size. Lengths are in lens diameters,
// centred on the quad.
Shader "Hindsight/Splash Mark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Off ("Unlit: lines", Color) = (0.45, 0.53, 0.68, 1)
        _OffFill ("Unlit: lens", Color) = (0.07, 0.1, 0.17, 1)
        _Lens ("Lit: lens", Color) = (0.86, 0.96, 1, 1)
        _Ink ("Lit: the H", Color) = (0.02, 0.03, 0.06, 1)
        _GlowColor ("Lit: rim and glow", Color) = (0.56, 0.9, 1, 1)
        _Extent ("Half size of the quad", Float) = 0.8
        _Lit ("Lit", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "False" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha // premultiplied, so the glow can add light

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float alpha : TEXCOORD1;
            };

            float4 _Off, _OffFill, _Lens, _Ink, _GlowColor;
            float _Extent, _Lit;

            // The logo's proportions.
            static const float Radius = 0.5;
            static const float Rim = 0.036;        // thickness of the ring
            static const float HalfHeight = 0.235; // of the H
            static const float HalfWidth = 0.205;
            static const float Stem = 0.082;
            static const float BarThin = 0.016;    // half thickness of the crossbar at the left stem...
            static const float BarWide = 0.074;    // ...and at the right

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.alpha = v.color.a;
                return o;
            }

            float segment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a, ba = b - a;
                return length(pa - ba * saturate(dot(pa, ba) / dot(ba, ba)));
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 p = (i.uv - 0.5) * 2.0 * _Extent;
                float edge = length(fwidth(p)) * 0.55;
                #define COVER(d) (1.0 - smoothstep(-edge, edge, (d)))

                float t = Stem * 0.5;
                float a = HalfWidth - t;
                float reach = HalfHeight - t;
                float stems = min(segment(p, float2(-a, -reach), float2(-a, reach)),
                                  segment(p, float2(a, -reach), float2(a, reach))) - t;
                // The crossbar: a beam of light in miniature, widening from left to right.
                float along = saturate((p.x + a) / (2.0 * a));
                float bar = max(abs(p.y) - lerp(BarThin, BarWide, along), abs(p.x) - a);
                float letter = COVER(min(stems, bar));

                float fromCentre = length(p);
                float disc = COVER(fromCentre - Radius);
                float ring = COVER(abs(fromCentre - (Radius - Rim * 0.5)) - Rim * 0.5);

                // Unlit: a dark glass lens, a touch lighter toward the top, with pale lines.
                float3 off = _OffFill.rgb * (1.0 + 0.5 * saturate(p.y / Radius + 0.3));
                off = lerp(off, _Off.rgb * 0.75, ring);
                off = lerp(off, _Off.rgb, letter);
                // Lit: white-hot in the middle, tinted at the rim, and the H in shadow.
                float3 on = lerp(float3(1, 1, 1), _Lens.rgb, smoothstep(0.0, 1.0, fromCentre / Radius));
                on = lerp(on, float3(1, 1, 1), ring);
                on = lerp(on, _Ink.rgb, letter);

                float3 color = lerp(off, on, _Lit);
                // The halo must be gone before the quad ends, or the quad shows as a faint square.
                float2 border = abs(i.uv - 0.5) * 2.0;
                float halo = _Lit * exp(-max(fromCentre - Radius, 0.0) * 9.0) * (1.0 - disc) * (1.0 - smoothstep(0.7, 1.0, max(border.x, border.y)));
                return float4(color * disc + _GlowColor.rgb * halo, disc) * i.alpha;
            }
            ENDCG
        }
    }
}
