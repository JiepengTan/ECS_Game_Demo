using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace GamesTan.ECS {
    public class DebugUtil {
        [Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition, object message) {
            if (!condition) {
                Debug.LogError(message);
            }
        }
    }

    public class SystemGroup : IEcsSystem {
        public string Name { get; set; }
        public bool IsEnable { get; set; } = true;
        public List<IEcsSystem> _systems = new List<IEcsSystem>();

        public void AddSystem(IEcsSystem sys) {
            _systems.Add(sys);
        }
        public void RemoveSystem(IEcsSystem sys) {
            _systems.Remove(sys);
        }
        public void Update(float dt) {
            if(!IsEnable) return;
            foreach (var item in _systems) {
                if(!item.IsEnable) return;
                Profiler.BeginSample(item.Name);
                item.Update(dt);
                Profiler.EndSample();
            }
        }
    }
}