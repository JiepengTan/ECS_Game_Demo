using System.Runtime.InteropServices;
using Lockstep.NativeUtil;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct BasicData : IComponent {
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        /// <summary> 状态集合   /// </summary>
        public BitSet32 StatusData;
    }
}