
using System.Collections.Generic;
using GamesTan.Rendering;
using GamesTan.Spatial;
using Lockstep.Game;
using UnityEngine;
using UnityEngine.Profiling;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public unsafe partial class GameEcsWorld :EcsWorld{
        public Region WorldRegion = new Region() ;
        public GameGlobalStateService Services ;
        
        public Context EntityManager;

        public GameEcsWorld(IServiceContainer services) : base(services) {
            Services = services.GetService<IGlobalStateService>() as GameGlobalStateService;
            WorldRegion.DoAwake();
        }
        public override void DoDestroy(){
            base.DoDestroy();
            _context.DoDestroy();
            WorldRegion.DoDestroy();
        }
    }
}