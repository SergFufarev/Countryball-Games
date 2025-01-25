// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/LoadingSprite-Radial"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
        
        _Fill ("Fill", Range(0.0, 1.0)) = 0.5

        _Radius ("Radius", Float) = 1
        _Cutoff ("Cut-off", Range(0.0, 1.415)) = 0
        
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
            float _MinX;
            float _MaxX;
            float _Fill;
            float4 _BarColor;
            float4 _BgColor;
            float _Radius;
            float _Cutoff;

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
                
                float y = IN.texcoord.y - 0.5;
                float x = IN.texcoord.x - 0.5;
                float r = _Radius / 2;
                float rx = _Radius * _Cutoff / 2;
                float r0 = (x * x + y * y);
                
                if(r0 < rx * rx ){
               		c.a = 0;
               	}
               	
               	float angle  = 180 * atan(x/y) / 3.14159;
               	
               	if(x > 0 && y < 0 || x < 0 && y < 0)
               		angle = angle + 180;

               	else if(x < 0 || y < 0)
               		angle = angle + 360;		
               	
                return angle / 360 < _Fill ? c*_BarColor : c*_BgColor;
            }
            ENDCG
        }
    }
} 