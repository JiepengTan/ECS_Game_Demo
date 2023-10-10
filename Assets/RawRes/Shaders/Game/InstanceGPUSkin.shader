Shader "GamesTan/InstanceGUPSkin"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_Normal("Normal", 2D) = "white" {}
		_Color ("_Color", Color) = (1,1,1,1)
    	
        [NoScaleOffset]_AnimatedBoneMatrices("AnimatedBoneMatrices", 2D) = "white" {}
        [Toggle]_EnableAnimation("EnableAnimation", Float) = 0
        [Toggle]_EnableInstance("_EnableInstance", Float) = 0
    	
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			//#pragma multi_compile_fwdbase  // no shadow
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
            #include "../Core/GPUSkin/GPUSkinInclude.cginc"
            struct v2f
            {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD1;
				SHADOW_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

            sampler2D _Normal;
	
            v2f vert (appdata_gpu_skin v)
            {
            	v2f o;
				float3 animPos = v.vertex.xyz;
				float3 animNor = v.normal.xyz;
				float3 animTan = v.tangent.xyz; 
				CalcIndirectAnim(v, animPos, animNor, animTan);
				o.uv = v.texcoord.xy;
            	o.worldPos =  mul(unity_ObjectToWorld, float4(animPos, 1.0));
				o.pos = mul(UNITY_MATRIX_VP,o.worldPos);
				o.color = 1;
            	TRANSFER_SHADOW(o);
				return o;
            }

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
				fixed4 col = tex2D(_MainTex, i.uv) ;
				col.rgb = col.rgb * lerp(1,atten,0.5);
				return col;
			}
			ENDCG
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "../Core/GPUSkin/GPUSkinInclude.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_gpu_skin v)
            {
				float3 animPos = v.vertex.xyz;
				float3 animNor = v.normal.xyz;
				float3 animTan = v.tangent.xyz;
				CalcIndirectAnim(v, animPos, animNor, animTan);
            	v.vertex.xyz = animPos;
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        
       
    }
}
