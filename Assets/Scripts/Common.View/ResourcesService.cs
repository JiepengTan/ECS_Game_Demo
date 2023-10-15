using System.Collections.Generic;
using UnityEngine;

namespace GamesTan {
    public class ResourcesService : MonoBehaviour {
        public static ResourcesService Instance;
        public List<GameObject> Prefabs = new List<GameObject>();
        private Dictionary<int, GameObject> id2Prefabs = new Dictionary<int, GameObject>();
        private void Awake() {
            Instance = this;
            foreach (var prefab in Prefabs) {
                var idStr = prefab.name.Split("_")[0];
                if (int.TryParse((string)idStr, out var id)) {
                    id2Prefabs[id] = prefab;
                }
                else {
                    Debug.LogError("Prefan name is invalid " + prefab.name);
                }
            }
        }
        GameObject LoadPrefab(int id) {
            if (id2Prefabs.TryGetValue(id, out var fab)) return fab;
            return null;
        }

        public GameObject Instantiate(int id, Vector3 pos, Vector3 euler, Transform parent = null) {
            var prefab = LoadPrefab(id);
            if (prefab == null) return null;
            var go = GameObject.Instantiate(prefab, pos, Quaternion.Euler(euler),parent);
            return go;
        }
    }
}