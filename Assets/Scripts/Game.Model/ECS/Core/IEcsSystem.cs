
namespace GamesTan.ECS {
    public interface IEcsSystem {
        bool IsEnable { get; set; }
        void Update(float dt);
    }
}