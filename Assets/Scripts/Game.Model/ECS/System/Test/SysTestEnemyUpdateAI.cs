using System.Collections.Generic;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAI : BaseEntityUpdateSystem {
        protected override void Update(ref Enemy entity, float dt) {
            entity.AITimer += dt;
            if (entity.AITimer > 3) { // update ai
                entity.AITimer = Services.RandomValue();
                entity.AIData.LerpInterval = 0.2f;
                entity.AIData.LerpTimer = 0;
                entity.AIData.TargetDeg = Services.RandomRange(0, 360);
                entity.RotateSpeed = Services.RandomRange(-30, 30);
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
                entity.Pos += entity.Forward * entity.Speed * dt;
            }
            //WorldRegion.Update(entity.__Data, ref entity.GridCoord, entity.Pos);
        }
    }
}