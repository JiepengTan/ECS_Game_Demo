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
        
        public void Update() {
            int count = UnityEngine.Random.Range(0, FreshCountPerFrame);
            var enemys = GameEcsManager.Instance.Enemys;
            for (int i = 0; i < count; i++) {
                bool isDelete = UnityEngine.Random.value<DeletePropability;
                if (isDelete&& _testUnits.Count>0) {
                    var idx = UnityEngine.Random.Range(0, _testUnits.Count);
                    enemys.QueueFree(_testUnits[idx]);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                }
                else if(!enemys.IsFull) {
                    var entity = enemys.Alloc();
                    _testUnits.Add(entity);
                }
            }
        }
    }
}