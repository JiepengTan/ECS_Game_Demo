using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    public struct EnemyConfig {
        /// <summary> AI 类型   /// </summary>
        public int AIType;
        /// <summary> 技能类型   /// </summary>
        public int SkillType;
        /// <summary> 节能CD   /// </summary>
        public float SkillInterval;
    }

    public partial struct Enemy : IEntity {

        public UnitData UnitData;
        public AIData AIData;
        /// <summary> Animation   /// </summary>
        public AnimRenderData AnimRenderData;
        public AnimData AnimInternalData;
        
         
        public override string ToString() {
            return __Data.ToString();
        }
    }
}