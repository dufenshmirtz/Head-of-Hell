Shader "HeadOfHell/PlayerSpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineSize ("Outline Size", Float) = 1
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, IN.uv) * IN.color;
                float baseAlpha = baseColor.a;

                float2 texel = _MainTex_TexelSize.xy * max(_OutlineSize, 0.0);

                float outlineAlpha = 0.0;
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2( texel.x, 0.0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2(-texel.x, 0.0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2(0.0,  texel.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2(0.0, -texel.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2( texel.x,  texel.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2(-texel.x,  texel.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2( texel.x, -texel.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.uv + float2(-texel.x, -texel.y)).a);

                if (baseAlpha <= 0.001 && outlineAlpha > 0.001)
                {
                    fixed4 outlineColor = _OutlineColor;
                    outlineColor.a *= outlineAlpha;
                    return outlineColor;
                }

                return baseColor;
            }
            ENDCG
        }
    }
}
