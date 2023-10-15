using System;
using GamesTan.Spatial;
using Lockstep.Game;

namespace GamesTan.ECS.Game {
    
    public partial class BaseGameExecuteSystem : BaseExecuteSystem{
        public GameEcsWorld World;
        public Context EntityManager;
        public Region WorldRegion;
        public float deltaTime = 0.03f;
        public GameGlobalStateService Services;
        protected override void OnAwake(BaseContext context, IServiceContainer services) {
            Name = GetType().Name;
            World = services.GetService<IGlobalStateService>().World as GameEcsWorld;
            Services = services.GetService<IGlobalStateService>() as GameGlobalStateService;
            EntityManager = context as Context;
            WorldRegion = World.WorldRegion;
        }
    }
    public partial class BaseGameSystem : BaseSystem{
        public GameEcsWorld World;
        public Context EntityManager;
        public Region WorldRegion;
        public float deltaTime = 0.03f;
        public GameGlobalStateService Services;
        protected override void OnAwake(BaseContext context, IServiceContainer services) {
            Name = GetType().Name;
            World = services.GetService<IGlobalStateService>().World as GameEcsWorld;
            Services = services.GetService<IGlobalStateService>() as GameGlobalStateService;
            EntityManager = context as Context;
            WorldRegion = World.WorldRegion;
        }
    }
}