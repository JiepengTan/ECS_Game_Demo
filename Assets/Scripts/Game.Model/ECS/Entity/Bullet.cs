using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Bullet {
        /// <summary> Entity Data   /// </summary>
        public EntityData __Data;
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        public int2 GridCoord;

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
    }
}