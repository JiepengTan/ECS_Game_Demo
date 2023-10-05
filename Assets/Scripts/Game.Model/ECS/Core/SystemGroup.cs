using System.Collections.Generic;

namespace GamesTan.ECS {
    public class SystemGroup : IEcsSystem {
        public bool IsEnable { get; set; }
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
                item.Update(dt);
            }
        }
    }
}