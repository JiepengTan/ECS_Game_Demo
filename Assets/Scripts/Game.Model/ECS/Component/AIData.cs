using System.Runtime.InteropServices;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct AIData:IComponent {
        /// <summary> AI 计时器   /// </summary>
        public float AITimer;
        public float TargetDeg;
        public float LerpInterval;
        public float LerpTimer;
    }
}