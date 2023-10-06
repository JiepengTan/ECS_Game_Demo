﻿using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyAwake : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = World.GetEnemys();
            foreach (var item in enemys) {
                var enemy = World.GetEnemy(item);
                if (!enemy->IsDoneStart) {
                    enemy->IsDoneStart = true;
                    enemy->Speed = 1;
                    enemy->Scale = new float3(1, 1, 1) * (Services.RandomValue()*0.5f+0.5f);
                    enemy->Pos = new float3(Services.RandomValue() * 100, 0, Services.RandomValue() * 100);
                    enemy->DegY = Services.RandomValue()*360;
                    
                    enemy->AnimInternalData.Timer = Services.RandomValue() * 3;
                    enemy->AnimInternalData.AnimId1[0] = Services.RandomRange(0,3) ;
                    enemy->AnimInternalData.AnimId1[1] = Services.RandomRange(0,3) ;
                }
            }
        }
    }
}