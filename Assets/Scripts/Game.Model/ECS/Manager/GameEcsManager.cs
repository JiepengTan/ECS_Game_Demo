namespace GamesTan.ECS.Game {
    public unsafe class GameEcsManager {
        private static GameEcsManager _instance;

        public static GameEcsManager Instance {
            get {
                if (_instance == null) {
                    _instance = new GameEcsManager();
                }

                return _instance;
            }
        }

        public enum EEntityType {
            Enemy,
            Bullet,
        }

        public void DoStart() {
            Enemys.Init((int)EEntityType.Enemy, 200);
        }

        public NativePoolEnemy Enemys = new NativePoolEnemy();

        public int EnemyCount => Enemys.Count;

        public EntityData CreateEnemy() {
            return Enemys.Alloc();
        }

        public void DestroyEnemy(EntityData item) {
            Enemys.QueueFree(item);
        }
    }
}