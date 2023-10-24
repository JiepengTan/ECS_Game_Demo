using System.Runtime.InteropServices;
using Lockstep.Math;
using Unity.Mathematics;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PhysicData : IComponent {
        public LVector2Int GridCoord;
        /// <summary> 半径   /// </summary>
        public LFloat Radius;
        /// <summary> 速度   /// </summary>
        public LFloat Speed;
        /// <summary> 旋转速度   /// </summary>
        public LFloat RotateSpeed ;
    }
}