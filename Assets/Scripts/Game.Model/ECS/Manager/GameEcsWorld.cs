
using System.Collections.Generic;
using GamesTan.Rendering;
using UnityEngine.Profiling;

namespace GamesTan.ECS.Game {
    
    [System.Serializable]
    public unsafe partial class GameEcsWorld {
        public GameServices _services = new GameServices();
        public GameServices Services => _services;
        
        public List<IEcsSystem> _systems = new List<IEcsSystem>();
        public void DoAwake() {
            CreatePools();
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
        }

        private void RegisterSystems() {
            _systems.Add(new SysGroupTest1());
        }

        
        public enum EEntityType {
            Enemy,
            Bullet,
        }
        private void CreatePools() {
            _enemyPool.Init((int)EEntityType.Enemy, 200);
        }

        private NativePoolEnemy _enemyPool = new NativePoolEnemy();
        public NativePoolEnemy EnemyPool => _enemyPool;
        public EntityList EnemyList => _enemyList;
        public int EnemyCount => _enemyPool.Count;
        private EntityList _enemyList = new EntityList();
        public List<EntityData> GetEnemys() {
            return _enemyList.GetInternalData();
        }
        public Enemy* GetEnemy(EntityData data) {
            return _enemyPool.GetData(data);
        }

        public EntityData CreateEnemy() {
            var data = _enemyPool.Alloc();
            _enemyList.Add(data);
            return data;
        }
        public void DestroyEnemy(EntityData item) {
            if (_enemyList.Remove(item)) {
                _enemyPool.QueueFree(item);
            }
        }
    }
}