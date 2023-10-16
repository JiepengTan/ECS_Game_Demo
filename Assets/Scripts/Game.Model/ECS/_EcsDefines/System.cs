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
    
    
#if false
    
    public class InputSystem : IPureSystem {
        public SkillData SkillData;
    }
    

    public class SpawnSystem : IPureSystemWithEntity {
        public SpawnData SpawnData;
    }
    
    public unsafe class InputSystem : GameExecuteSystem {
        public void Execute(
            ref SkillData skillData,
        ) {
            
        }
    }
    public unsafe class SpawnSystem : GameExecuteSystem {
        public void Execute(Entity* entity, ref SpawnData spawnData) {
        }
    }

    public unsafe class InitSystem : GameBaseSystem, IInitializeSystem {
        public void Initialize(IContext context) {
        }
    }


    public class BoidSteerSystem : IJobForEachSystem {
        public Transform3D Transform;
        public BoidState BoidState;
    }
    public unsafe class BoidSteerSystem : GameJobSystem {
        public unsafe struct JobDefine {
            public ConfigBoidSharedData Settings;
            public LFloat DeltaTime;
            [ReadOnly] public NativeArray<LVector3> ObstaclePositions;

            public void Execute(int index, ref Transform3D transform3D, ref BoidState boidState) {
                if (boidState.IsDied) return;
                var forward = transform3D.Forward;
                var currentPosition = transform3D.Position;
            }
        }

        protected override bool BeforeSchedule() {
            //assign jobData info
            JobData.DeltaTime = _globalStateService.DeltaTime;
            JobData.Settings = _gameConfigService.BoidSettting;
            JobData.ObstaclePositions = _context._AllObstaclePos;
            return true;
        }
    }

    public class SinkSystem : IJobSystem {
        public Transform3D Transform;
        public BoidState BoidState;
    }
    
    public unsafe class SinkSystem : GameJobSystem {
        public unsafe struct JobDefine {
            [ReadOnly] public LVector3 SinkOffset;
            [ReadOnly] public LFloat DeltaTime;

            public void Execute(ref Transform3D transform3D, ref BoidState boidState){
                if (!boidState.IsDied) return;
                boidState.SinkTimer -= DeltaTime;
                transform3D.Position += SinkOffset;
            }
        }

        protected override bool BeforeSchedule(){
            //assign jobData info
            JobData.DeltaTime = _globalStateService.DeltaTime;
            JobData.SinkOffset = new LVector3(0,_gameConfigService.BoidSettting.SinkSpd * JobData.DeltaTime,0) ;
            return true;
        }
    }
#endif
}