Shader "Hidden/GeometrySpreadsheet/InternalUvChecker"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _UvChannel("Uv Channel", Int) = 0
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
                float2 uv5 : TEXCOORD5;
                float2 uv6 : TEXCOORD6;
                float2 uv7 : TEXCOORD7;
                fixed4 color : COLOR;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _UvChannel;
            float4 _Color;

            float2 get_uv(appdata v)
            {
                if(_UvChannel == 1)
                    return v.uv1;
                if(_UvChannel == 2)
                    return v.uv2;
                if(_UvChannel == 3)
                    return v.uv3;
                if(_UvChannel == 4)
                    return v.uv4;
                if(_UvChannel == 5)
                    return v.uv5;
                if(_UvChannel == 6)
                    return v.uv6;
                if(_UvChannel == 7)
                    return v.uv7;

                return v.uv0;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(get_uv(v), _MainTex);

                half3 skyColor = half3(0.848, 0.908, 1.0);
                half3 groundColor = half3(0.188, 0.172, 0.14);

                float lerpValue = v.normal.y * 0.5 + 0.5;
                o.color.rgb = lerp(groundColor, skyColor, lerpValue);
                o.color.a = 1.0;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * _Color * i.color;
            }
            ENDCG
        }
    }
}