using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamesTan.Rendering {
    [System.Serializable]
    public class IndirectInstanceData {
        public int prefabId;
        public GameObject prefab;
        public Material indirectMaterial;
        public Vector3 positionOffset = new Vector3();
        public Mesh lod00Mesh;
        public Mesh lod01Mesh;
        public Mesh lod02Mesh;
    
    }
    [CreateAssetMenu(menuName = "Assets/InstanceConfig")]
    public class InstanceConfig : ScriptableObject {
        public List<IndirectInstanceData> Infos = new List<IndirectInstanceData>();
    }

}