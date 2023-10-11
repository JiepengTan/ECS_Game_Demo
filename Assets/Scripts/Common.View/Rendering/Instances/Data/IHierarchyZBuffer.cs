using UnityEngine;

namespace GamesTan.Rendering {
    public interface IHierarchyZBuffer {
        Vector2 TextureSize { get; }
        RenderTexture Texture{ get; }

        void InitializeTexture();
        bool Enabled { get; set; }
    }
}