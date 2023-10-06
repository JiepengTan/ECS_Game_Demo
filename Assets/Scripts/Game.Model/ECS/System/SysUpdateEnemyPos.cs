namespace GamesTan.ECS.Game {
    public unsafe partial class SysEnemyAwake : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = World.GetEnemys();
            foreach (var item in enemys) {
                var enemy = World.GetEnemy(item);
                if (!enemy->IsDoneStart) {
                    enemy->IsDoneStart = true;
                    enemy->Speed = 1;
                }
            }
        }
    }
    
    public unsafe partial class SysUpdateEnemyPos : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = World.GetEnemys();
            foreach (var item in enemys) {
                var enemy = World.GetEnemy(item);
                enemy->Deg += dt * Services.RandomRange(-10,10);
                enemy->Pos += enemy->Forward * enemy->Speed * dt;
            }
        }
    }
 
}