// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/Wireframe Transparent" 
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
		[Toggle] _RemoveDiag("Remove diagonals?", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 1.0
		[KeywordEnum(Off, On)] _ZWrite("ZWrite", Float) = 1.0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2.0
	}
	SubShader 
	{
	
	    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		ZTest [_ZTest]
		ZWrite [_ZWrite]
		Cull [_Cull]

    	Pass 
    	{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#pragma shader_feature __ _REMOVEDIAG_ON
			
			half4 _Color;
		
			struct v2g 
			{
    			float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO_EYE_INDEX
			};
			
			struct g2f 
			{
    			float4  pos : SV_POSITION;
    			float3 dist : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert(appdata_full v)
			{
    			v2g OUT;
                UNITY_SETUP_INSTANCE_ID(v);
		        UNITY_INITIALIZE_OUTPUT_STEREO_EYE_INDEX(OUT);
				
    			OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.worldPos = v.vertex;
    			return OUT;
			}
			
			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN[0]);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN[1]);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN[2]);
				
				float3 param = float3(0, 0, 0);
				float2 WIN_SCALE = float2(_ScreenParams.x/2.0, _ScreenParams.y/2.0);
				
				//frag position
				float2 p0 = WIN_SCALE * IN[0].pos.xy / IN[0].pos.w;
				float2 p1 = WIN_SCALE * IN[1].pos.xy / IN[1].pos.w;
				float2 p2 = WIN_SCALE * IN[2].pos.xy / IN[2].pos.w;
				
				//barycentric position
				float2 v0 = p2-p1;
				float2 v1 = p2-p0;
				float2 v2 = p1-p0;
				//triangles area
				float area = abs(v1.x*v2.y - v1.y * v2.x);

				#if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.z = 10000;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 10000;
                else
                    param.y = 10000;
                #endif
			
				g2f OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.pos = IN[0].pos;
				OUT.dist = float3(area/length(v0),0,0) + param;
				triStream.Append(OUT);

				OUT.pos = IN[1].pos;
				OUT.dist = float3(0,area/length(v1),0) + param;
				triStream.Append(OUT);

				OUT.pos = IN[2].pos;
				OUT.dist = float3(0,0,area/length(v2)) + param;
				triStream.Append(OUT);
				
			}
			
			half4 frag(g2f IN) : COLOR
			{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				//distance of frag from triangles center
				float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
				//fade based on dist from center
 				float I = exp2(-4.0*d*d);

				clip(I - 0.5f);
				
 				return _Color;
			}
			
			ENDCG

    	}
	}
}