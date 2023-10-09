using System.Runtime.InteropServices;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct EmitterData:IComponent {
        public int Deg;
        public int Count;
        public float LiveTime;
        public float Interval;
        public float Timer;
    }
}