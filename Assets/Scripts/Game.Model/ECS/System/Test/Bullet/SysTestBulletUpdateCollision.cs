using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestBulletUpdateCollision : BaseGameExecuteSystem {
        public void Execute(Bullet* entity) {
            var lst = WorldRegion.QueryCollision(entity->TransformData.Position, entity->PhysicData.Radius);
            foreach (var item in lst) {
                var target = EntityManager.GetEnemy((int)item);
                if (target == null) 
                    continue;
                var isCollision = CollisionUtil.IsCollision(target->Pos3, target->Radius, entity->Pos3, entity->Radius);
                if (isCollision) {
                    target->UnitData.Health = 0;
                    WorldRegion.RemoveEntity(target->EntityId,target->PhysicData.GridCoord);
                    EntityManager.DestroyEnemy(target);
                }
            }
        }
    }
}