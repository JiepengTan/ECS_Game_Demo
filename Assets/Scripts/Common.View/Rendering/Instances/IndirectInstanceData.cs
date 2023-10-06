using System.Collections;
using UnityEngine;

[System.Serializable]
public class IndirectInstanceData {
    public int prefabId;
    public GameObject prefab;
    public Material indirectMaterial;
    public Vector3 positionOffset = new Vector3();
    public Mesh lod00Mesh;
    public Mesh lod01Mesh;
    public Mesh lod02Mesh;
    
    private bool AssertInstanceData() {
        if (prefab == null) {
            Debug.LogError("Missing Prefab on instance at index: " + prefabId+ "! Aborting.");
            return false;
        }

        if (indirectMaterial == null) {
            Debug.LogError("Missing indirectMaterial on instance at index: " + prefabId+ "! Aborting.");
            return false;
        }

        if (lod00Mesh == null) {
            Debug.LogError("Missing lod00Mesh on instance at index: " + prefabId+ "! Aborting.");
            return false;
        }

        if (lod01Mesh == null) {
            Debug.LogError("Missing lod01Mesh on instance at index: " + prefabId+ "! Aborting.");
            return false;
        }

        if (lod02Mesh == null) {
            Debug.LogError("Missing lod02Mesh on instance at index: " + prefabId+ "! Aborting.");
            return false;
        }

        return true;
    }
}