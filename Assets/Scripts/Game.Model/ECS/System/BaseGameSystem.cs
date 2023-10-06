using System;

namespace GamesTan.ECS.Game {

    public partial class BaseGameSystemGroup : SystemGroup {
    }

    public partial class BaseGameSystem : BaseSystem {
        public GameEcsWorld World;
        public GameServices Services;
        public NativePoolEnemy EnemyPool;
        public EntityList EnemyList;
        
        public void DoAwake(GameEcsWorld world) {
            World = world;
            Services = World.Services;
            EnemyPool = World.EnemyPool;
            EnemyList = World.EnemyList;
        }
        
        
    }
}