using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAnimation : BaseEntityUpdateSystem {
        [BurstCompile]
        protected override void Update(ref Enemy entity, float dt) {
            ref var internalData = ref entity.AnimInternalData;
            if (!Services.DebugStopAnimation) {
                internalData.Timer += dt;
            }

            internalData.LerpTimer += dt;
            for (int idx = 0; idx < 4; idx++) {
                if (idx == 0 && internalData.Timer[idx] > 3) {
                    internalData.Timer[idx] = 0;
                    internalData.LerpTimer[idx] = 0;
                    internalData.AnimId2[idx] = internalData.AnimId1[idx];
                    internalData.AnimId1[idx] = Services.RandomRange(0, 3);
                }
            }

            ref var animInfo = ref entity.AnimRenderData;
            //TODO 处理其他的动画
            animInfo.AnimInfo0 = new float4(1, 0,
                AnimationUtil.GetAnimation(entity.AssetData.PrefabId, internalData.AnimId1[0], internalData.Timer[0]), 0);
            animInfo.AnimInfo1 = float4.zero;
            animInfo.AnimInfo2 = float4.zero;
            animInfo.AnimInfo3 = float4.zero;
            //Debug.Log(animInfo.AnimInfo0.ToString());
        }
    }
}