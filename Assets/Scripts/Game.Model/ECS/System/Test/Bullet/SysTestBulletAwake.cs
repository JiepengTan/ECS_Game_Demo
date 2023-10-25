using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestBulletAwake : BaseGameExecuteSystem {
        
        [CustomSystem]
        public void Execute(Bullet* entity) {
            if (!entity->IsAlreadyStart) {
                entity->IsAlreadyStart = true;
                entity->PhysicData.Speed = 1;
                entity->PhysicData.RotateSpeed = 10;
                entity->UnitData.Health = 100;
                    
                //WorldRegion.AddEntity(item, ref entity->PhysicData.GridCoord,entity->TransformData.Pos);
            }
        }
    }
}