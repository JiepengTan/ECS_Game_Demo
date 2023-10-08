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
            _bulletPool.Init((int)EEntityType.Bullet, 200);
        }

        public void DoDestroy() {
            _enemyPool.Destroy();
            _bulletPool.Destroy();
        }

        private NativePoolEnemy _enemyPool = new NativePoolEnemy();
        public NativePoolEnemy EnemyPool => _enemyPool;
        public EntityList EnemyList => _enemyList;
        public int EnemyCount => _enemyPool.Count;
        private EntityList _enemyList = new EntityList();
        public List<EntityData> GetAllEnemy() {
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
        
         
        private NativePoolBullet _bulletPool = new NativePoolBullet();
        public NativePoolBullet BulletPool => _bulletPool;
        public EntityList BulletList => _bulletList;
        public int BulletCount => _bulletPool.Count;
        private EntityList _bulletList = new EntityList();
        public List<EntityData> GetAllBullet() {
            return _bulletList.GetInternalData();
        }
        public Bullet* GetBullet(EntityData data) {
            return _bulletPool.GetData(data);
        }

        public EntityData AllocBullet() {
            var data = _bulletPool.Alloc();
            _bulletList.Add(data);
            return data;
        }
        public void FreeBullet(EntityData item) {
            if (_bulletList.Remove(item)) {
                _bulletPool.QueueFree(item);
            }
        }
    }
}