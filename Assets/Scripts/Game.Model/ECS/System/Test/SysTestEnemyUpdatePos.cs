using System.Collections.Generic;
using GamesTan.Rendering;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdatePos : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = World.GetEnemys();
            foreach (var item in enemys) {
                var enemy = World.GetEnemy(item);
                enemy->DegY += dt * Services.RandomRange(-10, 10);
                enemy->Pos += enemy->Forward * enemy->Speed * dt;
            }
        }
    }

    public unsafe partial class SysRenderUpload : BaseGameSystem {
        public override void Update(float dt) {
            var renderer = RenderWorld.Instance.RendererData;
            var pool = World.EnemyPool;
            for (int i = 0; i < pool.MaxUsedSlot; i++) {
                ref var ptr = ref pool.GetData(i);
                if (ptr.IsValid) {
                    renderer.AddRenderData(new RendererData(ptr.InstancePrefabIdx,ptr.Pos,ptr.Rot,ptr.Scale));
                }
            }

        }
    }
}