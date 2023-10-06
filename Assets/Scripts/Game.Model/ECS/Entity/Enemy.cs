using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    public struct EnemyConfig {
        /// <summary> AI 类型   /// </summary>
        public int AIType;

        /// <summary> 技能类型   /// </summary>
        public int SkillType;

        /// <summary> 节能CD   /// </summary>
        public float SkillInterval;
    }

    public struct Enemy {
        public EntityData __EntityData;

        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        
        /// <summary> 状态集合   /// </summary>
        public BitSet32 StatusData;

        /// <summary> 是否已经释放   /// </summary>
        public bool IsDoneStart {
            get => StatusData.Is(0);
            set => StatusData.Set(0, value);
        }


        /// <summary> 位置   /// </summary>
        public float2 Pos;

        /// <summary> 半径   /// </summary>
        public float Radius;

        /// <summary> 速度   /// </summary>
        public float Speed;

        /// <summary> 旋转   /// </summary>
        public float Deg;
        public float2 Forward {
            get {
                float deg = math.radians(-Deg+90);
                return new float2( math.cos(deg),math.sin(deg));
            }
        }
        /// <summary> 旋转   /// </summary>
        public float3 Deg3 => new float3(0, Deg, 0);
        public float3 Pos3 => new float3(Pos.x, 0, Pos.y);
        public float3 Forward3 {
            get {
                float deg = math.radians(-Deg+90);
                return new float3( math.cos(deg),0,math.sin(deg));
            }
        }

        /// <summary> 旋转   /// </summary>
        public int Health;

        public float FireInterval;

        /// <summary> 技能 计时器   /// </summary>
        public float SkillTimer;

        /// <summary> AI 计时器   /// </summary>
        public float AITimer;

        public override string ToString() {
            return __EntityData.ToString();
        }
    }
}