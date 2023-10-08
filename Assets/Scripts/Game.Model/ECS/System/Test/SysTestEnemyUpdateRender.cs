using GamesTan.Rendering;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateRender : BaseEntityUpdateSystem {
        private InstanceRenderData _renderer;
        public override void Update(float dt) {
            _renderer = RenderWorld.Instance.RendererData;
            base.Update(dt);
        }
        protected override void Update(ref Enemy entity, float dt) {
            if (entity.InstancePrefabIdx != -1) {
                _renderer.AddRenderData(new RendererData(entity.InstancePrefabIdx,entity.Pos,entity.Rot,entity.Scale,entity.AnimInfo));
            }
        }
    }
}