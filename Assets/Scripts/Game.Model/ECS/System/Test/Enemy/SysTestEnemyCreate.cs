//#define DEBUG_ONE_ENTITY

using System;
using System.Collections.Generic;
using GamesTan.Rendering;
using Lockstep.Math;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyCreate : BaseGameSystem {

        public override void Execute() {
            if (GlobalState.DeleteOrNewCountPerFrame == 0) return;
            int count = GlobalState.RandomRange(0, GlobalState.DeleteOrNewCountPerFrame);
            LFloat dist = GlobalState.DebugOnlyOneEntity?GlobalState.DebugEntityBornPosRange:100;
            if (GlobalState.DebugOnlyOneEntity  ) {
                count = math.min(count, GlobalState.DebugEntityCount-EntityManager.CurEnemyCount );
            }
            for (int i = 0; i < count; i++) {
                bool isSpawner = GlobalState.RandomValue() >= GlobalState.DeleteProbability;
                if (isSpawner) {
                    if (GlobalState.DebugOnlyOneEntity && EntityManager.CurEnemyCount >= GlobalState.DebugEntityCount) return;
                    var entity = EntityManager.PostCmdCreateEnemy();
                    entity->TransformData.Scale = new LVector3(1, 1, 1);
                    entity->AssetData.AssetId =GlobalState.IsOnlyOnePreafb?10003:( GlobalState.RandomValue() > new LFloat(null,300) ? 10001 : 10003);
                    entity->AssetData.InstancePrefabIdx = RenderWorld.Instance.GetInstancePrefabIdx(entity->AssetData.AssetId);
                    entity->Scale = (GlobalState.RandomValue()*LFloat.half+LFloat.half);
                    entity->TransformData.Position = new LVector3(GlobalState.RandomValue() * dist, 0, GlobalState.RandomValue() * dist);
                }
            }
        }
    }
}