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
        [Range(0,40)]
        public int DebugAreaCount = 8;
        [Range(1,30000)]
        public int DebugEntityCount = 1;
        [Range(1,1000)]
        public int DebugEntityBornPosRange = 100;
        public bool DebugStopAnimation = false;
        public bool DebugStopAI = false;
        public bool IsOnlyOnePreafb = false;
        
        [Header("Status")]
        public int CurEnemyCount;
        public List<EntityRef> AllUnits = new List<EntityRef>();
        
        [Header("View")]
        public bool IsCreateView = true;
        public Transform ViewRoot;
        public Dictionary<int, GameObject> Id2View = new Dictionary<int, GameObject>();
        public int GlobalViewId;

        public Material BulletMaterial;
        public override void DoDestroy() {
        }
    }
}