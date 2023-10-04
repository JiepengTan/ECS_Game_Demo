using System.Collections;
using System.Collections.Generic;
using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game
{
    public struct Bullet 
    {
        /// <summary> EntityId   /// </summary>
        public int EntityId;
        /// <summary> EntityId   /// </summary>
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
    }
}

