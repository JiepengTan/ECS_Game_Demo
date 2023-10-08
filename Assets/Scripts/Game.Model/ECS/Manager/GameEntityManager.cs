using System.Collections.Generic;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public unsafe partial class GameEntityManager {
        
        public enum EEntityType {
            Enemy,
            Bullet,
        }

        public GameEcsWorld World;
        public void DoAwake(GameEcsWorld world) {
            this.World = world;
            CreatePools();
        }

        private void CreatePools() {
            _enemyPool.Init((int)EEntityType.Enemy, 200);
        }

        private NativePoolEnemy _enemyPool = new NativePoolEnemy();
        public NativePoolEnemy EnemyPool => _enemyPool;
        public EntityList EnemyList => _enemyList;
        public int EnemyCount => _enemyPool.Count;
        private EntityList _enemyList = new EntityList();
        public List<EntityData> GetEnemys() {
            return _enemyList.GetInternalData();
        }
        public Enemy* GetEnemy(EntityData data) {
            return _enemyPool.GetData(data);
        }

        public EntityData AllocEnemy() {
            var data = _enemyPool.Alloc();
            _enemyList.Add(data);
            return data;
        }
        public void FreeEnemy(EntityData item) {
            if (_enemyList.Remove(item)) {
                _enemyPool.QueueFree(item);
            }
        }
    }
}