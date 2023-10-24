using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    
    public class SysTestUpdateSkinRender : IPureSystemWithEntity {
        public AssetData AssetData;
        public Transform3D Transform3D;
        public AnimRenderData AnimRenderData;
    }
    public class SysTestUpdateMeshRender : IPureSystemWithEntity {
        public AssetData AssetData;
        public Transform3D Transform3D;
        public MeshRenderData AnimRenderData;
    }
    
    // Enemy
    public class SysTestEnemyAwake : ISystemWithSpecifyEntity {
        public Enemy Entity;
    }
    public class SysTestEnemyUpdateAnimation : ISystemWithSpecifyEntity {
        public Enemy Entity;
    }
    public class SysTestEnemyUpdateAI : ISystemWithSpecifyEntity {
        public Enemy Entity;
    }
    
    // Bullet
    public class SysTestBulletAwake : ISystemWithSpecifyEntity {
        public Bullet Entity;
    }
    public class SysTestBulletUpdateCollision : ISystemWithSpecifyEntity {
        public Bullet Entity;
    }
    public class SysTestBulletUpdatePos : ISystemWithSpecifyEntity {
        public Bullet Entity;
    }
    
}