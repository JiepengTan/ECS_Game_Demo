
using System.Collections.Generic;
using GamesTan.Rendering;
using GamesTan.Spatial;
using UnityEngine;
using UnityEngine.Profiling;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public unsafe partial class GameEcsWorld {
        [SerializeField] private GameServices _services = new GameServices();
        public GameServices Services {
            get => _services;
            set => _services = value;
        }
        public GameEntityManager EntityManager = new GameEntityManager();
        public List<IEcsSystem> _systems = new List<IEcsSystem>();
        public Region WorldRegion = new Region();
        public void DoAwake() {
            EntityManager.DoAwake(this);
            WorldRegion.DoAwake();
            RegisterSystems();
            Services.DoAwake();
            foreach (var sys in _systems) {
                var gameSys = sys as IGameSystem;
                gameSys?.DoAwake(this);
            }
        }


        public void Update(float dt) {
            _services.Frame++;
            _services.DeltaTime = dt;
            _services.TimeSinceLevelLoad += dt;
            Profiler.BeginSample("OnFrameStart");
            RenderWorld.Instance.RendererData.OnFrameStart();
            Profiler.EndSample();
            Profiler.BeginSample("SystemUpdate");
            foreach (var sys in _systems) {
                sys.Update(dt);
            }
            Profiler.EndSample();
            Profiler.BeginSample("OnFrameEnd");
            RenderWorld.Instance.RendererData.OnFrameEnd();
            Profiler.EndSample();
        }


        public void DoDestroy() {
            Services.DoDestroy();
            EntityManager.DoDestroy();
            WorldRegion.DoDestroy();
        }

        private void RegisterSystems() {
            _systems.Add(new SysGroupTest1());
        }

        
    }
}