// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    public partial struct EnemyTag : IComponent {
        public int Padding;
    }

    public partial struct BulletTag : IComponent {
        public int Padding;
    }

    public partial struct SpawnerTag : IComponent {
        public int Padding;
    }

    public partial struct UnitData : IComponent {
        /// <summary> 攻  /// </summary>
        public int Attack;

        /// <summary> 防  /// </summary>
        public int Defence;

        /// <summary> 血  /// </summary>
        public int Health;
    }

    public partial struct EmitterData : IComponent {
        public int Deg;
        public int Count;
        public float LiveTime;
        public float Interval;
        public float Timer;
    }

    public partial struct AIData : IComponent {
        /// <summary> AI 计时器   /// </summary>
        public float AITimer;

        public float TargetDeg;
        public float LerpInterval;
        public float LerpTimer;
    }

    public partial struct AnimData : IComponent {
        public Vector4 Timer;
        public Vector4 LerpTimer;
        public Vector4Int AnimId1;
        public Vector4Int AnimId2;
    }
}