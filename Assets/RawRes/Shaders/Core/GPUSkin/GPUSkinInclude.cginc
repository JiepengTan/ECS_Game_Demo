//UNITY_SHADER_NO_UPGRADE
#ifndef GPUSKIN_INCLUDED_CGINC
#define GPUSKIN_INCLUDED_CGINC

struct appdata_gpu_skin
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


sampler2D _AnimatedBoneMatrices;
float4 _AnimatedBoneMatrices_TexelSize;
float4x4 _AnimationState; //blendFactor,transpercent,frameIndex
float _EnableAnimation;
float _EnableInstance;



#include "../../Core/Instances/ShaderInclude_IndirectStructs.cginc"
#include "../../Core/GPUSkin/GPUSkin.cginc"

uniform uint _ArgsOffset;
StructuredBuffer<uint> _ArgsBuffer;
StructuredBuffer<IndirectMatrix> _InstancesDrawMatrixRows;
StructuredBuffer<AnimData> _InstancesDrawAnimData;

void CalcIndirectAnim(in appdata_gpu_skin v,inout float3 animPos,inout float3 animNor,inout float3 animTan) {
	uint index = v.inst ;
#if defined(SHADER_API_D3D11) 
	index +=  _ArgsBuffer[_ArgsOffset];
#endif
	IndirectMatrix indirectMatrix = _InstancesDrawMatrixRows[index];
	AnimData compAnimData = _InstancesDrawAnimData[index];

	if(_EnableInstance) {
		unity_ObjectToWorld = float4x4(indirectMatrix.row0, indirectMatrix.row1, indirectMatrix.row2, float4(0, 0, 0, 1));
	}
    
    
	float4x4 animState =float4x4(compAnimData.AnimInfo0, compAnimData.AnimInfo1,
				     compAnimData.AnimInfo2, compAnimData.AnimInfo3);
    
	if(_EnableAnimation>0.5) {
		float3x4 uvs = float3x4(v.texcoord1,  v.texcoord2,  v.texcoord3);
		AnimateBlend_float(v.vertex, v.normal, v.tangent,uvs,
		    _AnimatedBoneMatrices, _AnimatedBoneMatrices_TexelSize.xy,
		    animState,  animPos,   animNor,   animTan) ;
	}
}

#endif 