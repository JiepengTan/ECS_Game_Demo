Shader "Unlit/Graphics_DrawMeshInstancedIndirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_Normal("Normal", 2D) = "white" {}
		_Color ("_Color", Color) = (1,1,1,1)
    	
        [NoScaleOffset]_AnimatedBoneMatrices("AnimatedBoneMatrices", 2D) = "white" {}
        _EnableAnimation("EnableAnimation", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
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
	        float4x4 _AnimationState;
	        float _EnableAnimation;

            
            uniform float4 _LightColor0;
            
            #include "ShaderInclude_IndirectStructs.cginc"
            uniform uint _ArgsOffset;
            StructuredBuffer<uint> _ArgsBuffer;
            StructuredBuffer<Indirect2x2Matrix> _InstancesDrawMatrixRows01;
            StructuredBuffer<Indirect2x2Matrix> _InstancesDrawMatrixRows23;
            StructuredBuffer<Indirect2x2Matrix> _InstancesDrawMatrixRows45;
            
            v2f vert (appdata v)
            {
				v2f o;
                uint index = v.inst + _ArgsBuffer[_ArgsOffset];
                
                Indirect2x2Matrix rows01 = _InstancesDrawMatrixRows01[index];
                Indirect2x2Matrix rows23 = _InstancesDrawMatrixRows23[index];
                Indirect2x2Matrix rows45 = _InstancesDrawMatrixRows45[index];
                
                float4x4 obj2world = float4x4(rows01.row0, rows01.row1, rows23.row0, float4(0, 0, 0, 1));
            	
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            	o.vertex = mul(UNITY_MATRIX_VP, mul(obj2world, float4(v.vertex.xyz, 1.0)));
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
