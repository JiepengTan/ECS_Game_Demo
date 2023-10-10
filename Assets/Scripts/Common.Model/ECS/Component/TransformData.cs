using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct TransformData :IComponent{
        /// <summary> 旋转   /// </summary>
        public float3 Pos;
        public float3 Rot;
        public float3 Scale;

        public override string ToString() {
            return $"pos{Pos} rot:{Rot} scale:{Scale}";
        }
    }
}