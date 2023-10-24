using System.Runtime.InteropServices;
using Lockstep.Math;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct AnimRenderDataForRendering:IComponent {
        /// <summary>
        /// x: AnimFactor
        /// y: FrameLerpFactor
        /// z: FrameIdx
        /// w: useless
        /// </summary>
        public Vector4 AnimInfo0;
        public Vector4 AnimInfo1;
        public Vector4 AnimInfo2;
        public Vector4 AnimInfo3;

        public override string ToString() {
            return
                $"anim1: AnimFactor:{AnimInfo0.x} FrameLerpFactor:{AnimInfo0.y} FrameIdx:{AnimInfo0.z}  \nAnimInfo1:{AnimInfo1}\nAnimInfo2:{AnimInfo2}\nAnimInfo3:{AnimInfo3}";
        }
        
        public void From(ref AnimRenderData tran) {
            AnimInfo0 = tran.AnimInfo0.ToVector4();
            AnimInfo1 = tran.AnimInfo1.ToVector4();
            AnimInfo2 = tran.AnimInfo2.ToVector4();
            AnimInfo3 = tran.AnimInfo3.ToVector4();
        }
    }
}