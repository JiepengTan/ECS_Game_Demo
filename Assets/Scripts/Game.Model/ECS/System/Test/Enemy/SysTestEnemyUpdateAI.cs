using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAI : BaseGameExecuteSystem {
        
        public void Execute(Enemy* entity) {
            if(Services.DebugStopAI) return;
            if(!entity->IsAlreadyStart) return;
            entity->AIData.AITimer += deltaTime;
            if (entity->AIData.AITimer > 3) { // update ai
                entity->AIData.AITimer = Services.RandomValue();
                entity->AIData.LerpInterval = 0.2f;
                entity->AIData.LerpTimer = 0;
                entity->AIData.TargetDeg = Services.RandomRange(0, 360);
                entity->PhysicData.RotateSpeed = Services.RandomRange(-30, 30);
            }
            ref var aiData = ref entity->AIData;
            aiData.LerpTimer += deltaTime;
            if (aiData.LerpTimer < aiData.LerpInterval) {
                // Turn to target 
                var percent = aiData.LerpTimer/aiData.LerpInterval;
                entity->DegY = math.lerp(entity->DegY, aiData.TargetDeg, percent);
            }
            else {
                // auto move
                entity->TransformData.Position += entity->Forward * entity->PhysicData.Speed * deltaTime;
            }
            WorldRegion.Update(entity->EntityId, ref entity->PhysicData.GridCoord, entity->TransformData.Position);
        }
    }
}