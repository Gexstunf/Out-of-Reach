Shader "UI/CRT_Heavy"
{
    Properties
    {
        _MainTex("Main Tex (UI)", 2D) = "white" {}
        _NoiseTex("Noise Texture (grayscale)", 2D) = "white" {}
        _ScanTex("Scanline Texture (grayscale)", 2D) = "white" {}

        _TintColor("Tint Color", Color) = (0.0, 1.0, 0.2, 1)      // green phosphor
        _Contrast("Contrast", Range(0,2)) = 1.2
        _Brightness("Brightness", Range(-0.5,0.5)) = 0.02

        _ScanIntensity("Scanline Intensity", Range(0,2)) = 0.6
        _ScanSpeed("Scanline Speed", Range(-2,2)) = 0.2

        _NoiseIntensity("Noise Intensity", Range(0,1)) = 0.06
        _NoiseSpeed("Noise Speed", Range(0,10)) = 1.2

        _FlickerAmount("Flicker Amount", Range(0,1)) = 0.08
        _FlickerSpeed("Flicker Speed", Range(0,10)) = 2.0

        _Curvature("Curvature (barrel)", Range(0,1)) = 0.18
        _Chromatic("Chromatic Aberration", Range(0,0.02)) = 0.008

        _Vignette("Vignette (0..1)", Range(0,1)) = 0.45
        _Glow("Glow Intensity", Range(0,2)) = 0.5

        _Alpha("Global Alpha", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _ScanTex;
            float4 _MainTex_ST;

            float4 _TintColor;
            float _Contrast;
            float _Brightness;

            float _ScanIntensity;
            float _ScanSpeed;

            float _NoiseIntensity;
            float _NoiseSpeed;

            float _FlickerAmount;
            float _FlickerSpeed;

            float _Curvature;
            float _Chromatic;

            float _Vignette;
            float _Glow;
            float _Alpha;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Barrel (curvature) distortion helper
            float2 BarrelDistort(float2 uv, float curvature)
            {
                // map uv to -1..1
                float2 p = uv * 2.0 - 1.0;
                float r2 = dot(p, p);
                // scale factor
                float k = 1.0 + curvature * r2;
                float2 p2 = p * k;
                // map back to 0..1
                return saturate(p2 * 0.5 + 0.5);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // simple contrast adjust
            fixed3 ContrastAndBright(fixed3 col, float contrast, float brightness)
            {
                col = (col - 0.5) * contrast + 0.5;
                col += brightness;
                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // apply slight barrel distortion (curvature)
                float2 dUV_R = BarrelDistort(uv, _Curvature);
                float2 dUV_G = BarrelDistort(uv + float2(_Chromatic, 0), _Curvature);
                float2 dUV_B = BarrelDistort(uv - float2(_Chromatic, 0), _Curvature);

                // sample main image (per channel offsets for chromatic aberration)
                fixed4 colR = tex2D(_MainTex, dUV_R);
                fixed4 colG = tex2D(_MainTex, dUV_G);
                fixed4 colB = tex2D(_MainTex, dUV_B);

                fixed3 col = fixed3(colR.r, colG.g, colB.b);

                // tint (phosphor)
                col *= _TintColor.rgb;

                // scanline — sample _ScanTex or generate using sin if not present
                // We'll sample the scan texture using vertical uv, scaled by speed
                float scanV = tex2D(_ScanTex, float2(uv.x * 1.0, uv.y * 1.0 + _Time.y * _ScanSpeed)).r;
                float scanMod = lerp(1.0, scanV, _ScanIntensity);

                col *= scanMod;

                // noise
                float2 noiseUV = uv * float2(8.0, 8.0) + float2(_Time.y * _NoiseSpeed, _Time.y * 0.3);
                float noise = tex2D(_NoiseTex, noiseUV).r;
                col = lerp(col, col * (1.0 - noise * _NoiseIntensity), noise);

                // flicker (global brightness modulation)
                float flick = 1.0 + (sin(_Time.y * _FlickerSpeed * 6.28318) * 0.5 + 0.5) * _FlickerAmount;
                col *= flick;

                // contrast/brightness
                col = ContrastAndBright(col, _Contrast, _Brightness);

                // vignette
                float2 centered = uv - 0.5;
                float dist = length(centered);
                float vig = smoothstep(0.5, 0.25, dist * (1.0 + _Vignette * 2.0));
                col *= vig;

                // glow (cheap: boost near bright areas)
                float lum = dot(col, fixed3(0.299, 0.587, 0.114));
                float glow = pow(lum, 2.0) * _Glow;
                col += glow * 0.08;

                // final clamp and alpha
                col = saturate(col);
                fixed4 outc = fixed4(col, _Alpha);

                return outc;
            }
            ENDCG
        }
    }
}
