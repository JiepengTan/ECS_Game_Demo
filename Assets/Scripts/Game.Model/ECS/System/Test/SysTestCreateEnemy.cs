//#define DEBUG_ONE_ENTITY

using System;
using System.Collections.Generic;
using GamesTan.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestCreateEnemy : BaseGameSystem {
        private List<EntityData> _testUnits => Services.AllUnits;

        public override void Update(float dt) {
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = Services.RandomRange(0, Services.DeleteOrNewCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (!isDelete) {
                    if (Services.DebugOnlyOneEntity && World.EnemyCount >= Services.DebugEntityCount) return;
                    var entity = World.CreateEnemy();
                    _testUnits.Add(entity);
                }
            }
        }
    }

    public unsafe partial class SysTestDestroyEnemy : BaseGameSystem {
        private List<EntityData> _testUnits => Services.AllUnits;

        public override void Update(float dt) {
            if (Services.DebugOnlyOneEntity) return;
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = Services.RandomRange(0, Services.DeleteOrNewCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (isDelete && _testUnits.Count > 0) {
                    var idx = Services.RandomRange(0, _testUnits.Count);
                    var unit = _testUnits[idx];
                    World.DestroyEnemy(unit);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                }
            }

            Services.CurEnemyCount = World.EnemyPool.Count;
        }
    }
}