using GamesTan.Rendering;
using Lockstep.Math;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestBulletCreate : BaseGameSystem {
        public override void Execute() {
            if (GlobalState.DeleteOrNewCountPerFrame == 0) return;
            int cur = EntityManager.CurBulletCount ;
            var targetCount = GlobalState.DebugAreaCount;
            if(cur >=targetCount) return;
            for (int i = cur; i < targetCount; i++) {
                var entity = EntityManager.PostCmdCreateBullet();
                entity->AssetData.AssetId = 11001;
                entity->AssetData.InstancePrefabIdx = 0;
                entity->DegY = GlobalState.RandomValue()*360;
                entity->Scale = 10;
                entity->TransformData.Position = new LVector3(GlobalState.RandomValue() * 100, 0, GlobalState.RandomValue() * 100);
            }
        }
    }
}