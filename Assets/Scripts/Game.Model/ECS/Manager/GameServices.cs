using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public unsafe class GameServices :BaseServices{
        
        [Header("DebugConfig")]
        [Range(0,1.0f)]
        public float DeleteProbability = 0.5f;
        [Range(0,3000)]
        public int DeleteOrNewCountPerFrame = 20;
        
        [Header("Debug")]
        public bool DebugOnlyOneEntity;
        public int DebugEntityCount = 1;
        public int DebugEntityBornPosRange = 100;
        public bool DebugStopAnimation = false;
        
        [Header("Status")]
        public int CurEnemyCount;
        public List<EntityData> AllUnits = new List<EntityData>();
        
        [Header("View")]
        public bool IsCreateView = true;
        public Transform ViewRoot;
        public Dictionary<int, GameObject> Id2View = new Dictionary<int, GameObject>();
        public int GlobalViewId;
    }
}