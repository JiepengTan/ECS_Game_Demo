using System.Runtime.InteropServices;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct MeshRenderData : IComponent {
        public int Padding;
    }
}