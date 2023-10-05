using System.Collections.Generic;
using GamesTan.ECS.Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.Game.View.Test {

    public unsafe class TestEntityPool : MonoBehaviour {
        [UnityEngine.Range(1,10000)]
        public int InitActorCount = 5000;
        [UnityEngine.Range(1,1000)]
        public int FreshCountPerFrame = 50;

        [UnityEngine.Range(0,1.0f)]
        public float DeletePropability = 0.4f;

        private List<EntityData> _testUnits = new List<EntityData>();
        public void Start() {
            //UnityEngine.Random.InitState(32);
            GameEcsManager.Instance.DoStart();
        }

        private Dictionary<int, GameObject> _id2Go = new Dictionary<int, GameObject>();

        public int goId;
        public void Update() {
            int count = UnityEngine.Random.Range(0, FreshCountPerFrame);
            var enemys = GameEcsManager.Instance.Enemys;
            for (int i = 0; i < count; i++) {
                bool isDelete = UnityEngine.Random.value<DeletePropability;
                if (isDelete&& _testUnits.Count>0) {
                    var idx = UnityEngine.Random.Range(0, _testUnits.Count);
                    var unit = _testUnits[idx];
                    var ptr = enemys.GetData(unit);
                    if(_id2Go.TryGetValue(ptr->GObjectId,out var go)){
                        GameObject.Destroy(go);
                        _id2Go.Remove(ptr->GObjectId);
                    }
                    enemys.QueueFree(unit);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                    Debug.Log("DestroyOne " + enemys.Count + unit+ "  \n" + enemys.ToString());
                }
                else if(!enemys.IsFull) {
                    var entity = enemys.Alloc();
                    _testUnits.Add(entity);
                    Debug.Log("CreateOne " + enemys.Count + entity + "  \n" + enemys.ToString());
                    var obj = new GameObject();
                    obj.name = $"{ goId++}_UnitID_{ entity.SlotId}";
                    obj.transform.SetParent(transform);
                    enemys.GetData(entity)->GObjectId = obj.GetInstanceID();
                    _id2Go.Add(obj.GetInstanceID(), obj);
                }
            }
        }
    }
}