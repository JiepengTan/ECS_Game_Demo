using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
   
    public class SysTestEnemy : IPureSystem {
        public Transform3D Transform;
        public EnemyTag EnemyTag;
    }
    public class SysDealBullet : IPureSystem {
        public Transform3D Transform;
        public BulletTag Tag;
    }
}