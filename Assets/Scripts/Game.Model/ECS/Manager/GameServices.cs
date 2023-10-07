using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public unsafe class GameServices :BaseServices{
        [Range(0,1.0f)]
        public float DeleteProbability = 0.5f;
        [Range(0,3000)]
        public int FreshCountPerFrame = 20;
        public bool IsCreateGo = true;
        public bool IsShowLog = false;
        public List<EntityData> _testUnits = new List<EntityData>();
        public Dictionary<int, GameObject> _id2Go = new Dictionary<int, GameObject>();
        public int CurGoId;
        public Transform transform;

        public int CurEnemyCount;
        public bool DebugOnlyOneEntity;
        public int DebugEntityCount = 1;
        public int DebugEntityBornRange = 30;
        public bool DebugStopAnimation = false;
    }
}