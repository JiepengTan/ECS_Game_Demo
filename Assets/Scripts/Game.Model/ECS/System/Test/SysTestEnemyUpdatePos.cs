using System.Collections.Generic;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdatePos : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = World.GetEnemys();
            foreach (var item in enemys) {
                var enemy = World.GetEnemy(item);
                //enemy->DegY += dt * Services.RandomRange(-10, 10);
                //enemy->Pos += enemy->Forward * enemy->Speed * dt;
            }
        }
    }
}