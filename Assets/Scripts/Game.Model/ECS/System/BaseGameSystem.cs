using System;

namespace GamesTan.ECS.Game {

    public interface IGameSystem {
        void DoAwake(GameEcsWorld world);
    }

    public partial class BaseGameSystemGroup : SystemGroup,IGameSystem {
        public void DoAwake(GameEcsWorld world) {
            
            foreach (var sys in _systems) {
                var gameSys = sys as IGameSystem;
                gameSys?.DoAwake(world);
            }
        }
    }

    public partial class BaseGameSystem : BaseSystem ,IGameSystem{
        public GameEcsWorld World;
        public GameServices Services;
        public NativePoolEnemy EnemyPool;
        public EntityList EnemyList;
        
        public void DoAwake(GameEcsWorld world) {
            Name = GetType().Name;
            World = world;
            Services = World.Services;
            EnemyPool = World.EnemyPool;
            EnemyList = World.EnemyList;
        }
        
        
    }
}