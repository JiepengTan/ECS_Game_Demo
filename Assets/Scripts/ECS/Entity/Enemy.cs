using System;
using Lockstep.NativeUtil;
using Unity.Mathematics;

namespace GamesTan.ECS.Game
{
    
    public struct EnemyConfig
    {
        /// <summary> AI 类型   /// </summary>
        public int AIType;
        /// <summary> 技能类型   /// </summary>
        public int SkillType;
        /// <summary> 节能CD   /// </summary>
        public float SkillInterval;
    }

    public struct EnemyRef
    {
        /// <summary> EntityId   /// </summary>
        public int EntityId;
        /// <summary> Slot Index   /// </summary>
        public int SlotId;
    }

    public struct Enemy
    {
        /// <summary> EntityId   /// </summary>
        public int EntityId;
        /// <summary> 状态集合   /// </summary>
        public BitSet32 StatusData;
        public bool IsFree
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