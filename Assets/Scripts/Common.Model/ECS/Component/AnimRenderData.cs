
using System.Runtime.InteropServices;
using Lockstep.Math;

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
        public LVector4 AnimInfo0;
        public LVector4 AnimInfo1;
        public LVector4 AnimInfo2;
        public LVector4 AnimInfo3;

        public override string ToString() {
            return
                $"anim1: AnimFactor:{AnimInfo0.x} FrameLerpFactor:{AnimInfo0.y} FrameIdx:{AnimInfo0.z}  \nAnimInfo1:{AnimInfo1}\nAnimInfo2:{AnimInfo2}\nAnimInfo3:{AnimInfo3}";
        }
    }
}