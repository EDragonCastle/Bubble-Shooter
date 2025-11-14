// 지금 있는 Texture와 Attack Texture or Bomb Texture를 혼합시키기 위해 만든 Shader다.
Shader "Custom/Bubble"
{
     Properties
    {
        _Color ("Tint", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _SubTex ("SubTexture", 2D) = "white"{}
        _UseSubTex("Use Sub Texture", float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        Cull off

        Pass
        {
            CGPROGRAM
            
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SubTex;
            float4 _SubTex_ST;

            float4 _Color;
            float _UseSubTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float2 subUV : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                // 현재 사용하는 texture의 크기가 너무 커서 줄이고 offset을 이동한다.
                float scale = 1.3f;
                o.subUV =  v.uv * scale;
                float offset = (1.0 - scale) * 0.5f;
                o.subUV += float2(offset, offset);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                col *= _Color;
                col *= i.color;

                // _Use 값이 1이면 Texure를 혼합한다.
                if (_UseSubTex > 0.5)
                {
                    fixed4 col2 = tex2D(_SubTex, i.subUV);
                    col = lerp(col, col2, col2.a);
                }

                return col; 
            }
            ENDCG
        }
    }
}
