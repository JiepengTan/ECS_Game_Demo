
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct AnimRenderData:IComponent {
        /// <summary>
        /// x: AnimFactor
        /// y: FrameLerpFactor
        /// z: FrameIdx
        /// w: useless
        /// </summary>
        public float4 AnimInfo0;
        public float4 AnimInfo1;
        public float4 AnimInfo2;
        public float4 AnimInfo3;

        public override string ToString() {
            return
                $"anim1: AnimFactor:{AnimInfo0.x} FrameLerpFactor:{AnimInfo0.y} FrameIdx:{AnimInfo0.z}  \nAnimInfo1:{AnimInfo1}\nAnimInfo2:{AnimInfo2}\nAnimInfo3:{AnimInfo3}";
        }
    }
}