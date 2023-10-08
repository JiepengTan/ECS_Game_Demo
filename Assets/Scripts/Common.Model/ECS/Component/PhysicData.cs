using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PhysicData : IComponent {
        public int2 GridCoord;
        /// <summary> 半径   /// </summary>
        public float Radius;
        /// <summary> 速度   /// </summary>
        public float Speed;
        /// <summary> 旋转速度   /// </summary>
        public float RotateSpeed ;
    }
}