using System.Collections.Generic;
using Lockstep.Math;
using Unity.Burst;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAI : BaseGameExecuteSystem {
        
        [CustomSystem]
        public void Execute(Enemy* entity) {
            if(GlobalState.DebugStopAI) return;
            if(!entity->IsAlreadyStart) return;
            entity->AIData.AITimer += deltaTime;
            if (entity->AIData.AITimer > 3) { // update ai
                entity->AIData.AITimer = GlobalState.RandomValue();
                entity->AIData.LerpInterval = new LFloat(null,200);
                entity->AIData.LerpTimer = 0;
                entity->AIData.TargetDeg = GlobalState.RandomRange(0, 360);
                entity->PhysicData.RotateSpeed = GlobalState.RandomRange(-30, 30);
            }
            ref var aiData = ref entity->AIData;
            aiData.LerpTimer += deltaTime;
            if (aiData.LerpTimer < aiData.LerpInterval) {
                // Turn to target 
                var percent = aiData.LerpTimer/aiData.LerpInterval;
                entity->DegY = LMath.Lerp(entity->DegY, aiData.TargetDeg, percent);
            }
            else {
                // auto move
                entity->TransformData.Position += entity->Forward * entity->PhysicData.Speed * deltaTime;
            }
            WorldRegion.Update(entity->EntityId, ref entity->PhysicData.GridCoord, entity->TransformData.Position);
        }
    }
}