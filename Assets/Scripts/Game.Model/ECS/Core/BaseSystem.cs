namespace GamesTan.ECS {
    public partial class BaseSystem : IEcsSystem {
        public bool IsEnable { get; set; } = true;
        public virtual void Update(float dt) {
        }
    }
}