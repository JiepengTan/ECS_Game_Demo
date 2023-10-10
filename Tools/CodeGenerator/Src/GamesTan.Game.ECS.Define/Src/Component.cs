// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    

    public partial class BoidSpawnerTag : IGameComponent {
        public int Pad;
    }

    public partial class BoidTag : IGameComponent {
        public int Pad;
    }

    public partial class BoidObstacleTag : IGameComponent {
        public int Pad;
    }

    public partial class BoidTargetTag : IGameComponent {
        public int Pad;
    }


    public partial class TargetMoveInfo : IGameComponent {
        public Vector3 InitPos;
        public float Interval;
        public float Radius;
        public float InitDeg;
    }

    public partial class BoidState : IGameComponent {
        public float SinkTimer;
        public bool IsDied;
        public bool IsScored;
        public int KillerIdx;
    }
    public partial class ViewData : IGameComponent {
        public int ViewId;
    }

    public partial class SpawnData : IGameComponent {
        public int Count;
        public float Radius;
        public Vector3 Position;
    }

    public partial class AssetData : IGameComponent {
        public int AssetId;
    }

    public partial class CellIndexData : IGameComponent {
        public int Index;
    }

    public partial class ScaleData : IGameComponent {
        public Vector3 Value;
    }

    public partial class PlayerData : IGameComponent {
        public int Score;
        public int LocalId;
    }

    public partial class SkillData : IGameComponent {
         public bool IsNeedFire;
        public bool IsFiring;
        public float CD;
         public float CdTimer; // <=0 表示cd 冷却
        public float Duration;
         public float DurationTimer;
        public float AtkRange;
    }

    public partial class MoveData : IGameComponent {
        public float MoveSpd;
        public float AcceleratedSpd;
        public float CurSpd;
        public float AngularSpd;
        public float DeltaDeg;
    }

    public partial class CellData : IGameComponent {
        public int Count;
        public Vector3 Alignment;
        public Vector3 Separation;
        public float ObstacleDistance;
        public int ObstaclePositionIndex;
        public int TargetPositionIndex;
        public int Index;
    }

    //``````` end of boid demo
}