using System.Runtime.InteropServices;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct UnitData:IComponent {
        /// <summary> 攻  /// </summary>
        public int Attack;
        /// <summary> 防  /// </summary>
        public int Defence;
        /// <summary> 血  /// </summary>
        public int Health;
    }
}