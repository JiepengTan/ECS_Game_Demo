namespace GamesTan.ECS {
    public partial class BaseSystem : IEcsSystem {
        public string Name { get; set; }
        public bool IsEnable { get; set; } = true;
        public virtual void Update(float dt) {
        }
    }
}