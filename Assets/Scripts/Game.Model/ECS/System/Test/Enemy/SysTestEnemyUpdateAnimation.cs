﻿using System.Text;
using Lockstep.Math;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAnimation : BaseGameExecuteSystem {
        [CustomSystem]
        public void Execute(Enemy* entity) {
            ref var internalData = ref entity->AnimData;
            if (!GlobalState.DebugStopAnimation) {
                internalData.Timer += deltaTime *LVector4.one;
            }

            internalData.LerpTimer += deltaTime*LVector4.one;
            for (int idx = 0; idx < 4; idx++) {
                if (idx == 0 && internalData.Timer[idx] > 3) {
                    internalData.Timer[idx] = 0;
                    internalData.LerpTimer[idx] = 0;
                    internalData.AnimId2[idx] = internalData.AnimId1[idx];
                    internalData.AnimId1[idx] = GlobalState.RandomRange(0, 3);
                }
            }

            ref var animInfo = ref entity->AnimRenderData;
            //TODO 处理其他的动画
            animInfo.AnimInfo0 = new LVector4(1, 0,
                AnimationUtil.GetAnimation(entity->AssetData.AssetId, internalData.AnimId1[0], internalData.Timer[0]),
                0);
            animInfo.AnimInfo1 = LVector4.zero;
            animInfo.AnimInfo2 = LVector4.zero;
            animInfo.AnimInfo3 = LVector4.zero;
        }

    }
}