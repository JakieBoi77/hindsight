// Full-screen backdrop for the studio splash: a soft radial gradient, plus the light the mark
// gives off (a glow around it and the beam). Positions are in canvas units from the centre.
Shader "Hindsight/Splash Backdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Inner ("Centre colour", Color) = (0.11, 0.15, 0.28, 1)
        _Outer ("Edge colour", Color) = (0.04, 0.06, 0.12, 1)
        _Size ("Canvas size (xy)", Vector) = (1920, 1200, 0, 0)
        _Spot ("Glow: x, y, radius, strength", Vector) = (0, 0, 300, 0)
        _SpotColor ("Glow colour", Color) = (0.3, 0.8, 0.9, 1)
        _Beam ("Beam: x, y, reach, strength", Vector) = (0, 0, 0, 0)
        _BeamShape ("Beam: half height at the mark, spread, soft front, length of the round nose", Vector) = (80, 0.09, 420, 60)
        _BeamColor ("Beam colour", Color) = (0.75, 0.95, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

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

            float4 _Inner, _Outer, _Size, _Spot, _SpotColor, _Beam, _BeamShape, _BeamColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.alpha = v.color.a;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 p = (i.uv - 0.5) * _Size.xy;
                float3 color = lerp(_Inner.rgb, _Outer.rgb, smoothstep(0.0, 1.0, length(p) / (_Size.y * 0.95)));

                float2 s = (p - _Spot.xy) / _Spot.z;
                color += _SpotColor.rgb * (exp(-dot(s, s)) * _Spot.w);

                // The beam leaves the mark to the right: flat across its middle, soft at its edges
                // and its front, widening and thinning out a little as it goes. Where it starts it
                // is rounded like the end of a capsule (half an ellipse, _BeamShape.w long).
                float2 b = p - _Beam.xy;
                float width = _BeamShape.x + max(b.x, 0.0) * _BeamShape.y;
                float across = length(float2(min(b.x / _BeamShape.w - 1.0, 0.0), b.y / width));
                across *= across;
                float along = 1.0 - smoothstep(_Beam.z - _BeamShape.z, _Beam.z, b.x);
                float beam = exp(-across * across) * along * pow(_BeamShape.x / width, 0.8) * _Beam.w;
                color += _BeamColor.rgb * beam;

                // Half a display step of noise hides the banding dark gradients show on phones and tablets.
                float noise = frac(sin(dot(i.pos.xy, float2(12.9898, 78.233))) * 43758.5453) - 0.5;
                #ifdef UNITY_COLORSPACE_GAMMA
                color += noise / 255.0;
                #else
                color += noise * (2.2 / 255.0) * pow(max(color, 1e-4), 0.55);
                #endif
                return float4(color, i.alpha);
            }
            ENDCG
        }
    }
}
