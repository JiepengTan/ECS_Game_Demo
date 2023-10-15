//#define DEBUG_ONE_ENTITY

using System;
using System.Collections.Generic;
using GamesTan.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyCreate : BaseGameSystem {

        public override void Execute() {
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = Services.RandomRange(0, Services.DeleteOrNewCountPerFrame);
            float dist = Services.DebugOnlyOneEntity?Services.DebugEntityBornPosRange:100;
            if (Services.DebugOnlyOneEntity  ) {
                count = math.min(count, Services.DebugEntityCount-EntityManager.CurEnemyCount );
            }
            for (int i = 0; i < count; i++) {
                bool isSpawner = Services.RandomValue() >= Services.DeleteProbability;
                if (isSpawner) {
                    if (Services.DebugOnlyOneEntity && EntityManager.CurEnemyCount >= Services.DebugEntityCount) return;
                    var entity = EntityManager.PostCmdCreateEnemy();
                    entity->TransformData.Scale = new float3(1, 1, 1);
                    entity->AssetData.AssetId =Services.IsOnlyOnePreafb?10003:( Services.RandomValue() > 0.3 ? 10001 : 10003);
                    entity->AssetData.InstancePrefabIdx = RenderWorld.Instance.GetInstancePrefabIdx(entity->AssetData.AssetId);
                    entity->Scale = (Services.RandomValue()*0.5f+0.5f);
                    entity->TransformData.Position = new float3(Services.RandomValue() * dist, 0, Services.RandomValue() * dist);
                }
            }
        }
    }
}