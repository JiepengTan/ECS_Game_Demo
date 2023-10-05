namespace GamesTan.ECS.Game {
    public partial class BaseGameSystem : BaseSystem {
        public GameEcsWorld Game;
        public GameServices Services;
        public NativePoolEnemy EnemyPool;
        public EntityList EnemyList;


        public void DoAwake(GameEcsWorld game) {
            Game = game;
            Services = Game.Services;
            EnemyPool = Game.EnemyPool;
            EnemyList = Game.EnemyList;
        }
        
        
    }
}