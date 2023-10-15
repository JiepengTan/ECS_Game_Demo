using UnityEngine;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public class RenderInfo {
        public Mesh mesh;
        public Material mat;
    }
    [System.Serializable]
    public struct UnityPrefabInfo {
        public GameObject Prefab;
        public int AssetId;
    }
}