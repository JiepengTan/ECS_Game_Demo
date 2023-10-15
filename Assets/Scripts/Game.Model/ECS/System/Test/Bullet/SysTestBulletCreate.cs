using GamesTan.Rendering;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestBulletCreate : BaseGameSystem {
        public override void Execute() {
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int cur = EntityManager.CurBulletCount ;
            var targetCount = Services.DebugAreaCount;
            if(cur >=targetCount) return;
            for (int i = cur; i < targetCount; i++) {
                var entity = EntityManager.PostCmdCreateBullet();
                entity->AssetData.AssetId = 11001;
                entity->AssetData.InstancePrefabIdx = 0;
                entity->DegY = Services.RandomValue()*360;
                entity->Scale = 10;
                entity->TransformData.Position = new float3(Services.RandomValue() * 100, 0, Services.RandomValue() * 100);
            }
        }
    }
}