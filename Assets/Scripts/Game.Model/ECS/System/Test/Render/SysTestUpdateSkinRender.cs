using GamesTan.Rendering;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestUpdateSkinRender : BaseGameExecuteSystem {
        private InstanceRenderData _renderer;
        public override void BeforeExecute() {
            _renderer = RenderWorld.Instance.RendererData;
        }
        public void Execute(Entity* entity, ref AssetData assetData, ref Transform3D transformData,ref AnimRenderData animRenderData) {
            if (assetData.InstancePrefabIdx != -1) {
                _renderer.AddRenderData(new RendererData(assetData.InstancePrefabIdx,transformData,animRenderData));
            }
        }
    }
}