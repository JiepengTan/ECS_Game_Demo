using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyAwake : BaseGameSystem {
        public override void Update(float dt) {
            float dist = 100;
            if (Services.DebugOnlyOneEntity) {
                dist = Services.DebugEntityBornPosRange;
            }
            var enemys = EntityManager.GetEnemys();
            foreach (var item in enemys) {
                var enemy = EntityManager.GetEnemy(item);
                if (!enemy->IsDoneStart) {
                    enemy->IsDoneStart = true;
                    enemy->Speed = 1;
                    enemy->RotateSpeed = 10;
                    enemy->Scale = new float3(1, 1, 1) * (Services.RandomValue()*0.5f+0.5f);
                    enemy->Pos = new float3(Services.RandomValue() * dist, 0, Services.RandomValue() * dist);
                    enemy->DegY = Services.RandomValue()*360;
                    enemy->AnimInternalData.Timer = Services.RandomValue() * 3;
                    enemy->AnimInternalData.AnimId1[0] = Services.RandomRange(0,3) ;
                    enemy->AnimInternalData.AnimId1[1] = Services.RandomRange(0,3) ;  
                    WorldRegion.AddEntity(item,enemy->Pos);
                }
            }
        }
    }
}