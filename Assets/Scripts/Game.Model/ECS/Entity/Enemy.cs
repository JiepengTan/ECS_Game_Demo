using System;
using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    public struct EntityData {

        public const int SlotBitCount = 20;
        public const int TypeIdBitCount = 32-SlotBitCount;
        public const int MaxSlotId = 1<<SlotBitCount;
        /// <summary> TypeId   /// </summary>
        public uint TypeId=> DataInfo >> SlotBitCount ;

        /// <summary> SlotId   /// </summary>
        public uint SlotId=> DataInfo &0x0FFFFF;

        /// <summary> internalInfo   /// </summary>
        public UInt32 DataInfo;
        /// <summary> Version   /// </summary>
        public int Version;

        public bool IsAlive => Version < 0;
        public EntityData(int typeId, int slotId, int version) {
            Debug.Assert(typeId <1<<TypeIdBitCount,"TypeId out of range " +typeId );
            Debug.Assert(slotId <1<<SlotBitCount,"EntityId out of range "+ slotId );
            DataInfo = (uint)typeId <<SlotBitCount | (uint)slotId;
            Version = version;
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