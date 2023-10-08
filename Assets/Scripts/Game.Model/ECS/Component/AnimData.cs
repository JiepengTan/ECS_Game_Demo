using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct AnimData : IComponent {
        public float4 Timer;
        public float4 LerpTimer;
        public int4 AnimId1;
        public int4 AnimId2;
    }
}