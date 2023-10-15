using System.Runtime.InteropServices;
using GamesTan.ECS;

namespace GamesTan.ECS {
    public class Define {
        public const int PackSize = 4;
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct BasicData : IComponent {
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        /// <summary> 状态集合   /// </summary>
        public Bitset32 StatusData;
    }
}