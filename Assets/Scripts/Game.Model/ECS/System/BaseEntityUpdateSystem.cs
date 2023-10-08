namespace GamesTan.ECS.Game {
    public unsafe partial  class BaseEntityUpdateSystem :BaseGameSystem  {
        public override void Update(float dt) {
            var pool = EntityManager.EnemyPool;
            var count = pool.MaxUsedSlot;
            var ptrAry = pool.GetData();
            for (int i = 0; i < count; i++) {
                ref var entity =ref ptrAry[i];
                if (entity.IsValid) {
                    Update(ref entity, dt);
                }
            }
        }

        protected virtual void Update(ref Enemy entity, float dt) {
        }
    }
}