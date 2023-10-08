using System;
using Gamestan.Spatial;

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
        public GameEntityManager EntityManager;
        public GameServices Services;
        public NativePoolEnemy EnemyPool;
        public EntityList EnemyList;
        public Region WorldRegion;
        public void DoAwake(GameEcsWorld world) {
            Name = GetType().Name;
            World = world;
            Services = World.Services;
            WorldRegion = World.WorldRegion;
            EntityManager = World.EntityManager;
            EnemyPool = EntityManager.EnemyPool;
            EnemyList = EntityManager.EnemyList;
        }
    }
}