Shader "Hidden/PsychedelicEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 3)) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                float dist = length(toCenter) * 2.0;
                float2 dir = normalize(toCenter + 0.001);

                float aberration = dist * _Intensity * 0.02;
                float wave = _Intensity * 0.03 * sin(uv.y * 20.0 + _Time.y * 5.0) * dist;
                float wave2 = _Intensity * 0.02 * sin(uv.x * 15.0 + _Time.y * 4.0) * dist;

                float2 uvR = uv + dir * (aberration + wave) + float2(wave2, 0);
                float2 uvG = uv + float2(wave2 * 0.5, wave * 0.5);
                float2 uvB = uv - dir * (aberration + wave) - float2(wave2, 0);

                fixed4 col;
                col.r = tex2D(_MainTex, uvR).r;
                col.g = tex2D(_MainTex, uvG).g;
                col.b = tex2D(_MainTex, uvB).b;
                col.a = 1;

                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
