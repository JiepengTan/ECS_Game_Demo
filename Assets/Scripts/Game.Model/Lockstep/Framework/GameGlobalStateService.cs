using System.Collections.Generic;
using GamesTan.ECS;
using Lockstep.Game;
using UnityEngine;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    public class GameGlobalStateService : GlobalStateService {
        [Header("DebugConfig")]
        [Range(0,1.0f)]
        public float DeleteProbability = 0f;
        [Range(0,3000)]
        public int DeleteOrNewCountPerFrame = 100;
        
        [Header("Debug")]
        public bool DebugOnlyOneEntity = true;
        [Range(0,40)]
        public int DebugAreaCount = 6;
        [Range(1,30000)]
        public int DebugEntityCount = 500;
        [Range(1,1000)]
        public int DebugEntityBornPosRange = 100;
        public bool DebugStopAnimation = false;
        public bool DebugStopAI = false;
        public bool IsOnlyOnePreafb = false;
        
        
    }
}