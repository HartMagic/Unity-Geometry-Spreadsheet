Shader "Hidden/GeometrySpreadsheet/InternalUvLayout"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
         _UvChannel("Uv Channel", Int) = 0
		_SrcBlend ("SrcBlend", Int) = 5.0 
		_DstBlend ("DstBlend", Int) = 10.0
		_ZWrite ("ZWrite", Int) = 1.0
		_ZTest ("ZTest", Int) = 4.0
		_Cull ("Cull", Int) = 0.0
		_ZBias ("ZBias", Float) = 0.0
    }
    SubShader
    {
       Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			ZTest [_ZTest]
			Cull [_Cull]
			Offset [_ZBias], [_ZBias]
            
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            	float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
			
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
			
			v2f vert(appdata v)
			{
				v2f o;
            	o.uv = get_uv(v);
				o.vertex = UnityObjectToClipPos(float4(o.uv.x, o.uv.y, 0, 1));
				o.color = _Color;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			
			ENDCG
        }
    }
}