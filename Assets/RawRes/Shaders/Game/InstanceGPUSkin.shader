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
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
			    float4 tangent : TANGENT;
			    float3 normal : NORMAL;
			    float4 texcoord : TEXCOORD0;
			    float4 texcoord1 : TEXCOORD1;
			    float4 texcoord2 : TEXCOORD2;
			    float4 texcoord3 : TEXCOORD3;
			    fixed4 color : COLOR;
	
            	uint inst : SV_InstanceID;
            	uint id : SV_VertexID;
            };
            struct v2f
            {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

            sampler2D _Normal;
            sampler2D _AnimatedBoneMatrices;
	        float4 _AnimatedBoneMatrices_TexelSize;
	        float4x4 _AnimationState; //blendFactor,transpercent,frameIndex
	        float _EnableAnimation;
	        float _EnableInstance;

        	
            uniform float4 _LightColor0;
            
            #include "../Core/Instances/ShaderInclude_IndirectStructs.cginc"
            #include "../Core/GPUSkin/GPUSkin.cginc"
            uniform uint _ArgsOffset;
            StructuredBuffer<uint> _ArgsBuffer;
            StructuredBuffer<IndirectMatrix> _InstancesDrawMatrixRows;
            StructuredBuffer<AnimData> _InstancesDrawAnimData;


            float4 animStateRow1;
            v2f vert (appdata v)
            {
				v2f o;
                uint index = v.inst + _ArgsBuffer[_ArgsOffset];
                
                IndirectMatrix rows01 = _InstancesDrawMatrixRows[index];
				AnimData compAnimData = _InstancesDrawAnimData[index];

            	if(_EnableInstance) {
            		unity_ObjectToWorld = float4x4(rows01.row0, rows01.row1, rows01.row2, float4(0, 0, 0, 1));
            	}
            	
            	float3 animPos = v.vertex.xyz;
            	float3 animNor = v.normal.xyz;
            	float3 animTan = v.tangent.xyz;
            	
            	float4x4 animState =float4x4(compAnimData.AnimInfo0, compAnimData.AnimInfo1,
            	                             compAnimData.AnimInfo2, compAnimData.AnimInfo3);
            	
            	if(_EnableAnimation>0.5) {
            		float3x4 uvs = float3x4(v.texcoord1,  v.texcoord2,  v.texcoord3);
            		AnimateBlend_float(v.vertex, v.normal, v.tangent,uvs,
            			_AnimatedBoneMatrices, _AnimatedBoneMatrices_TexelSize.xy,
            			animState,  animPos,   animNor,   animTan) ;
            	}
            	o.uv = v.texcoord.xy;
            	o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(animPos, 1.0)));
				//world position for rim
				o.color = 1;//mul(unity_ObjectToWorld, pos);

                return o;
            }

			fixed4 frag (v2f i) : SV_Target
			{
				//Texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
        }
    }
}
