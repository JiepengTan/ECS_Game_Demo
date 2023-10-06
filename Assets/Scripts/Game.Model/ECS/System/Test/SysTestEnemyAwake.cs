namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyAwake : BaseGameSystem {
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
}