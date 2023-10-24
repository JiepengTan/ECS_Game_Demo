using System;
using GamesTan.Spatial;
using Lockstep.Game;
using Lockstep.Math;

namespace GamesTan.ECS.Game {
    
    public partial class BaseGameExecuteSystem : BaseExecuteSystem{
        public GameEcsWorld World;
        public Context EntityManager;
        public Region WorldRegion;
        public LFloat deltaTime = new LFloat(null,30);
        public GameGlobalStateService GlobalState;
        public GameInputService InputService;
        protected override void OnAwake(BaseContext context, IServiceContainer services) {
            Name = GetType().Name;
            World = services.GetService<IGlobalStateService>().World as GameEcsWorld;
            GlobalState = services.GetService<IGlobalStateService>() as GameGlobalStateService;
            InputService  = services.GetService<IInputService>() as GameInputService;
            EntityManager = context as Context;
            WorldRegion = World.WorldRegion;
        }
    }
    public partial class BaseGameSystem : BaseSystem{
        public GameEcsWorld World;
        public Context EntityManager;
        public Region WorldRegion;
        public LFloat deltaTime = new LFloat(null,30);
        public GameGlobalStateService GlobalState;
        public GameInputService InputService;
        protected override void OnAwake(BaseContext context, IServiceContainer services) {
            Name = GetType().Name;
            World = services.GetService<IGlobalStateService>().World as GameEcsWorld;
            GlobalState = services.GetService<IGlobalStateService>() as GameGlobalStateService;
            InputService  = services.GetService<IInputService>() as GameInputService;
            EntityManager = context as Context;
            WorldRegion = World.WorldRegion;
        }
    }
}