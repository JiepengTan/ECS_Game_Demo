//#define DEBUG_ONE_ENTITY

using System;
using System.Collections.Generic;
using GamesTan.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestCreateEnemy : BaseGameSystem {
        private List<EntityRef> _testUnits => Services.AllUnits;

        public override void Update(float dt) {
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = Services.RandomRange(0, Services.DeleteOrNewCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (!isDelete) {
                    if (Services.DebugOnlyOneEntity && EntityManager.EnemyCount >= Services.DebugEntityCount) return;
                    var entity = World.CreateEnemy();
                    _testUnits.Add(entity);
                }
            }
        }
    }
}