using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAI : BaseEntityUpdateSystem {
        
        [BurstCompile]
        protected override void Update(ref Enemy entity, float dt) {
            if(Services.DebugStopAI) return;
            entity.AIData.AITimer += dt;
            if (entity.AIData.AITimer > 3) { // update ai
                entity.AIData.AITimer = Services.RandomValue();
                entity.AIData.LerpInterval = 0.2f;
                entity.AIData.LerpTimer = 0;
                entity.AIData.TargetDeg = Services.RandomRange(0, 360);
                entity.PhysicData.RotateSpeed = Services.RandomRange(-30, 30);
            }
            ref var aiData = ref entity.AIData;
            aiData.LerpTimer += dt;
            if (aiData.LerpTimer < aiData.LerpInterval) {
                // Turn to target 
                var percent = aiData.LerpTimer/aiData.LerpInterval;
                entity.DegY = math.lerp(entity.DegY, aiData.TargetDeg, percent);
            }
            else {
                // auto move
                entity.TransformData.Pos += entity.Forward * entity.PhysicData.Speed * dt;
            }
            WorldRegion.Update(entity.__Data, ref entity.PhysicData.GridCoord, entity.TransformData.Pos);
        }
    }
}