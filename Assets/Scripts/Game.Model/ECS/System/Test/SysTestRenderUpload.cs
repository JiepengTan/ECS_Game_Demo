using GamesTan.Rendering;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestRenderUpload : BaseGameSystem {
        public override void Update(float dt) {
            var renderer = RenderWorld.Instance.RendererData;
            var pool = World.EnemyPool;
            var count = pool.MaxUsedSlot;
            var ptrAry = pool.GetData();
            for (int i = 0; i < count; i++) {
                var item = ptrAry[i];
                if (item.IsValid) {
                    if (item.InstancePrefabIdx != -1) {
                        renderer.AddRenderData(new RendererData(item.InstancePrefabIdx,item.Pos,item.Rot,item.Scale,item.AnimInfo));
                    }
                }
            }

        }
    }
}