using System.Collections.Generic;
using GamesTan.ECS;
using GamesTan.ECS.Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.Game.View.Test {
    public unsafe class TestEntityPool : MonoBehaviour {
        [UnityEngine.Range(1, 10000)] public int InitActorCount = 5000;
        [UnityEngine.Range(1, 3000)] public int FreshCountPerFrame = 50;

        [UnityEngine.Range(0, 1.0f)] public float DeletePropability = 0.4f;

        public bool IsShowLog = false;
        public int CurCount;
        public int CurCapacity;
        public int CurGoId;

        private List<EntityRef> _testUnits = new List<EntityRef>();

        private NativePoolEnemy _pool;

        private Dictionary<int, GameObject> _id2Go = new Dictionary<int, GameObject>();
        public bool IsCreateGo = true;

        public void Update() {
            int count = UnityEngine.Random.Range(0, FreshCountPerFrame);
            CurCount = _pool.Count;
            CurCapacity = _pool.Capacity;
            for (int i = 0; i < count; i++) {
                bool isDelete = UnityEngine.Random.value < DeletePropability;
                if (isDelete && _testUnits.Count > 0) {
                    var idx = UnityEngine.Random.Range(0, _testUnits.Count);
                    var unit = _testUnits[idx];
                    var ptr = _pool.GetData(unit);
                    if (IsCreateGo) {
                        if (_id2Go.TryGetValue(ptr->GObjectId, out var go)) {
                            GameObject.Destroy(go);
                            _id2Go.Remove(ptr->GObjectId);
                        }
                    }

                    _pool.QueueFree(unit);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                    if (IsShowLog) Debug.Log("DestroyOne " + _pool.Count + unit + "  \n" + _pool.ToString());
                }
                else {
                    var entity = _pool.Alloc();
                    _testUnits.Add(entity);
                    if (IsShowLog) Debug.Log("CreateOne " + _pool.Count + entity + "  \n" + _pool.ToString());
                    if (IsCreateGo) {
                        var obj = new GameObject();
                        obj.name = $"{CurGoId++}_UnitID_{entity.SlotId}";
                        obj.transform.SetParent(transform);
                        _pool.GetData(entity)->GObjectId = obj.GetInstanceID();
                        _id2Go.Add(obj.GetInstanceID(), obj);
                    }
                }
            }
        }
    }
}