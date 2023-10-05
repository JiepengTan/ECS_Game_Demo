namespace GamesTan.ECS.Game {
    public unsafe partial class SysUpdateEnemyPos : BaseGameSystem {
        public override void Update(float dt) {
            var enemys = Game.GetEnemys();
            foreach (var item in enemys) {
                var enemy = Game.GetEnemy(item);
                enemy->Deg += dt * 10;
            }
        }
    }
}