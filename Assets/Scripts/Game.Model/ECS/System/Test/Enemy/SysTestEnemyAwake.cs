using Lockstep.Math;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyAwake : BaseGameExecuteSystem {
        public void Execute(Enemy* entity) {
            if (!entity->IsAlreadyStart) {
                entity->IsAlreadyStart = true;
                entity->PhysicData.Speed = 5;
                entity->PhysicData.RotateSpeed = 10;
                entity->UnitData.Health = 100;
                entity->DegY = GlobalState.RandomValue()*360;
                entity->AnimData.Timer = (GlobalState.RandomValue() * 3) * LVector4.one;
                entity->AnimData.AnimId1[0] = GlobalState.RandomRange(0,3) ;
                entity->AnimData.AnimId1[1] = GlobalState.RandomRange(0,3) ;  
                WorldRegion.AddEntity(entity->EntityId, ref entity->PhysicData.GridCoord,entity->TransformData.Position);
            }
        }

    }
}