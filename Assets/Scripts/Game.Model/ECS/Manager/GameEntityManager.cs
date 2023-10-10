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
        public List<EntityRef> GetAllEnemy() {
            return _enemyList.GetInternalData();
        }
        public Enemy* GetEnemy(EntityRef data) {
            return _enemyPool.GetData(data);
        }

        public EntityRef AllocEnemy() {
            var data = _enemyPool.Alloc();
            _enemyList.Add(data);
            return data;
        }
        public void FreeEnemy(EntityRef item) {
            if (_enemyList.Remove(item)) {
                _enemyPool.QueueFree(item);
            }
        }
        
         
        private NativePoolBullet _bulletPool = new NativePoolBullet();
        public NativePoolBullet BulletPool => _bulletPool;
        public EntityList BulletList => _bulletList;
        public int BulletCount => _bulletPool.Count;
        private EntityList _bulletList = new EntityList();
        public List<EntityRef> GetAllBullet() {
            return _bulletList.GetInternalData();
        }
        public Bullet* GetBullet(EntityRef data) {
            return _bulletPool.GetData(data);
        }

        public EntityRef AllocBullet() {
            var data = _bulletPool.Alloc();
            _bulletList.Add(data);
            return data;
        }
        public void FreeBullet(EntityRef item) {
            if (_bulletList.Remove(item)) {
                _bulletPool.QueueFree(item);
            }
        }
    }
}