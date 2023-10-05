using System;
using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    public struct EntityData {
        /// <summary> TypeId   /// </summary>
        public short TypeId;

        /// <summary> EntityId   /// </summary>
        public short SlotId;

        /// <summary> Version   /// </summary>
        public int Version;

        public bool IsAlive => Version < 0;

        public EntityData(int typeId, int entityId, int version) {
            SlotId = (short)entityId;
            Version = version;
            TypeId = (short)typeId;
        }

        public override string ToString() {
            return $" typeId{TypeId} SlotId:{SlotId} Version:{Version}";
        }
    }

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
        public bool IsMemFree {
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