// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/LoadingSprites-Vertical"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
        _Fill ("Fill", Range(0.0, 1.0)) = 0.5
        _MinY ("MinY", Float) = 0
        _MaxY ("MaxY", Float) = 1
        
        _BarColor ("Bar Color", Color) = (1,1,1,1)
        _BgColor ("Background Color", Color) = (0.2,0.2,0.2,1)
     }

    SubShader
    {
        LOD 200

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Cull Off 
            Lighting Off
            ZWrite Off
            Offset -1, -1
            Fog { Mode Off }
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
			
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MinY;
            float _MaxY;
            float _Fill;
            float4 _BgColor;
			float4 _BarColor;
			
			
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0; 
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            half4 frag (v2f IN) : COLOR 
            {
            	half4 c = tex2D(_MainTex, IN.texcoord);
            	if ((IN.texcoord.y<_MinY)|| (IN.texcoord.y>(_MinY+_Fill*(_MaxY-_MinY))))
                {
                   return c * _BgColor;
                  
                }
                
                else{
                	return c * _BarColor;
                }
                
            }
            ENDCG
        }
    }
} 