// A UI sprite shader that blurs the sprite by a number of screen pixels, for Blurry Spy:
// after eye drops things can look blurry for a while. Based on Unity's UI/Default shader.
// _BlurPixels is in screen pixels; the blur is sampled with 16 taps on two rings.
Shader "EyeCanDo/UI Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurPixels ("Blur (screen pixels)", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float _BlurPixels;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 Sample(float2 uv)
            {
                // Outside the sprite counts as transparent, so edges soften instead of smearing.
                float inside = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
                return (tex2D(_MainTex, uv) + _TextureSampleAdd) * inside;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 dx = ddx(IN.texcoord) * _BlurPixels;
                float2 dy = ddy(IN.texcoord) * _BlurPixels;
                fixed4 sum = Sample(IN.texcoord) * 2.0;
                float weight = 2.0;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 0.785398;
                    float2 dir = float2(cos(angle), sin(angle));
                    sum += Sample(IN.texcoord + (dx * dir.x + dy * dir.y) * 0.5) * 1.5;
                    sum += Sample(IN.texcoord + (dx * cos(angle + 0.3927) + dy * sin(angle + 0.3927)));
                    weight += 2.5;
                }

                fixed4 color = sum / weight;
                color *= IN.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
