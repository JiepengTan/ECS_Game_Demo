
namespace GamesTan.ECS {
    public interface IEcsSystem {
        string Name { get; }
        bool IsEnable { get; set; }
        void Update(float dt);
    }
}