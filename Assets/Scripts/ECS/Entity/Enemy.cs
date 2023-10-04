using System;
using Lockstep.NativeUtil;
using Unity.Mathematics;

namespace GamesTan.ECS.Game
{
    public struct EntityData
    {
        /// <summary> EntityId   /// </summary>
        public int SlotId;
        /// <summary> Version   /// </summary>
        public int Version;

        public EntityData(int entityId,int version)
        {
            SlotId = entityId;
            Version = version;
        }
    }

    public struct EnemyConfig
    {
        /// <summary> AI 类型   /// </summary>
        public int AIType;
        /// <summary> 技能类型   /// </summary>
        public int SkillType;
        /// <summary> 节能CD   /// </summary>
        public float SkillInterval;
    }

    public struct Enemy
    {
        public EntityData EntityData;
        /// <summary> EntityId   /// </summary>
        public int EntityId => EntityData.SlotId;
        /// <summary> Version   /// </summary>
        public int Version=> EntityData.SlotId;
        
        /// <summary> 状态集合   /// </summary>
        public BitSet32 StatusData;
        /// <summary> 是否已经释放   /// </summary>
        public bool IsMemFree
        {
            get => StatusData.Is(0);
            set => StatusData.Set(0,value);
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
    }
}