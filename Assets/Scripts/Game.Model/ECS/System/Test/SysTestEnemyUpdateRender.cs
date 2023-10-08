using GamesTan.Rendering;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateRender : BaseEntityUpdateSystem {
        private InstanceRenderData _renderer;
        public override void Update(float dt) {
            _renderer = RenderWorld.Instance.RendererData;
            base.Update(dt);
        }
        protected override void Update(ref Enemy entity, float dt) {
            if (entity.AssetData.InstancePrefabIdx != -1) {
                _renderer.AddRenderData(new RendererData(entity.AssetData.InstancePrefabIdx,entity.TransformData.Pos,entity.TransformData. Rot,entity.TransformData.Scale,entity.AnimRenderData));
            }
        }
    }
}