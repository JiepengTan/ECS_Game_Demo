using System.Runtime.InteropServices;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct AssetData : IComponent {
        /// <summary> Prefab Id   /// </summary>
        public int PrefabId;
        /// <summary> Prefab 下标，用于Instance 批量渲染   /// </summary>
        public int InstancePrefabIdx;
    }
}