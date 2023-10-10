using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
   
    #region Boid
    //Input
    public class InputSystem : IPureSystem {
        public SkillData SkillData;
        public PlayerData PlayerData;
        public MoveData MoveData;
        public Transform3D Transform;
    }
    //Init 
        //BoidInitSystem
    
    // game logic
        //boid  
        public class BoidHashPosSystem : IJobForEachSystem {
            public Transform3D Transform;
            public BoidTag Tag;
        }

        public class BoidSteerSystem : IJobForEachSystem {
            public Transform3D Transform;
            public BoidState BoidState;
        }
        public class BoidMergeSystem : IJobHashMapSystem { }
        public class BoidCopyStateSystem : IJobForEachSystem {
            public Transform3D Transform;
            public BoidTag Tag;
        }


    public class SpawnSystem : IPureSystemWithEntity {
        public SpawnData SpawnData;
        public AssetData AssetData;
    }

    public class TargetMoveSystem : IPureSystem{
        public Transform3D Transform;
        public TargetMoveInfo TargetMoveInfo;
        public BoidTargetTag Tag;
    }

    public class PlayerMoveSystem : IPureSystem {
        public Transform3D transform3D;
        public SkillData skillData;
        public MoveData moveData;
    }

    public class SkillSystem : IPureSystemWithEntity {
        public SkillData SkillData;
    }
    public class CollisionSystem : IJobSystem {
        public Transform3D Transform;
        public BoidState BoidState;
    }  
    public class SinkSystem : IJobSystem {
        public Transform3D Transform;
        public BoidState BoidState;
    }
    public class DestroySystem : IPureSystemWithEntity {
        public BoidState BoidState;
    }
    public class ScaleSystem : IPureSystem {
        public Transform3D Transform;
        public PlayerData BoidPlayerData;
    }
    
    #endregion
}