// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 


namespace GamesTan.UnsafeECSDefine {
    public partial class EnemyTag : IGameComponent {
        public int Padding;
    }

    public partial class BulletTag : IGameComponent {
        public int Padding;
    }

    public partial class SpawnerTag : IGameComponent {
        public int Padding;
    }

    public partial class UnitData : IGameComponent {
        [ToolTips("攻")]
        public int Attack;

        [ToolTips("防")]
        public int Defence;

        [ToolTips("血")]
        public int Health;
    }

    public partial class EmitterData : IGameComponent {
        public int Deg;
        public int Count;
        public float LiveTime;
        public float Interval;
        public float Timer;
    }

    public partial class AIData : IGameComponent {
        [ToolTips("AI 计时器 ")]
        public float AITimer;

        public float TargetDeg;
        public float LerpInterval;
        public float LerpTimer;
    }

    public partial class AnimData : IGameComponent {
        public Vector4 Timer;
        public Vector4 LerpTimer;
        public Vector4Int AnimId1;
        public Vector4Int AnimId2;
    }
}