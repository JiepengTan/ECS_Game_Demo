using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    
    public unsafe partial class SysTestCreateBullet : BaseGameSystem {
        public override void Update(float dt) {
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = EntityManager.BulletCount;
            if(count >=8) return;
            World.CreateBullet();
        }
    }
    public unsafe partial class SysTestBulletAwake : BaseGameSystem {
        public override void Update(float dt) {
            var entities = EntityManager.GetAllBullet();
            foreach (var item in entities) {
                var entity = EntityManager.GetBullet(item);
                if (!entity->IsAlreadyStart) {
                    entity->IsAlreadyStart = true;
                    entity->PhysicData.Speed = 1;
                    entity->PhysicData.RotateSpeed = 10;
                    entity->UnitData.Health = 100;
                    entity->Scale = 10;
                    
                    entity->DegY = Services.RandomValue()*360;
                    //WorldRegion.AddEntity(item, ref entity->PhysicData.GridCoord,entity->TransformData.Pos);
                }
            }
        }
    }

    public unsafe partial class SysTestBulletUpdateCollision : BaseGameSystem {

        HashSet<EntityData> _needDestroyEntities = new HashSet<EntityData>();
        public override void Update(float dt) {
            var pool = EntityManager.BulletPool;
            var count = pool.MaxUsedSlot;
            var ptrAry = pool.GetData();
            _needDestroyEntities.Clear();
            for (int i = 0; i < count; i++) {
                ref var entity =ref ptrAry[i];
                if (entity.IsValid) {
                    var lst = WorldRegion.QueryCollision(entity.TransformData.Pos, entity.PhysicData.Radius);
                    foreach (var item in lst) {
                        var target = EntityManager.GetEnemy(item);
                        var isCollision = CollisionUtil.IsCollision(target->Pos3, target->Radius, entity.Pos3, entity.Radius);
                        if (isCollision) {
                            target->UnitData.Health = 0;
                            _needDestroyEntities.Add(target->__EntityData);
                        }
                    }
                }
            }

            foreach (var target in _needDestroyEntities) {
                World.DestroyEnemy(target);
            }
        }
    }
}