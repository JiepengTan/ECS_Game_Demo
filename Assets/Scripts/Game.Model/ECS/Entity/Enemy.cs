using System.Runtime.InteropServices;
using Lockstep.NativeUtil;
using Unity.Mathematics;
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


    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Enemy {
        public EntityData __Data;
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        
        public int2 GridCoord;
        
        /// <summary> 状态集合   /// </summary>
        public BitSet32 StatusData;

        /// <summary> Prefab Id   /// </summary>
        public int PrefabId;
        /// <summary> Prefab 下标，用于Instance 批量渲染   /// </summary>
        public int InstancePrefabIdx;

        public bool IsValid => __Data.Version > 0;
        /// <summary> 是否已经释放   /// </summary>
        public bool IsDoneStart {
            get => StatusData.Is(0);
            set => StatusData.Set(0, value);
        }


        /// <summary> 位置   /// </summary>
        public float2 Pos2;

        /// <summary> 半径   /// </summary>
        public float Radius;

        /// <summary> 速度   /// </summary>
        public float Speed;

        /// <summary> 旋转   /// </summary>
        public float DegY {
            get => Rot.y;
            set => Rot.y = value;
        }

        public float RotateSpeed ;

        public float2 Forward2 {
            get {
                float deg = math.radians(-DegY+90);
                return new float2( math.cos(deg),math.sin(deg));
            }
        }

        /// <summary> 旋转   /// </summary>
        public float3 Rot;

        public float3 Pos;
        // TODO 
        public float3 Forward {
            get {
                float deg = math.radians(-DegY+90);
                return new float3( math.cos(deg),0,math.sin(deg));
            }
        }

        public float3 Scale;

        /// <summary> 旋转   /// </summary>
        public int Health;

        public float FireInterval;

        /// <summary> 技能 计时器   /// </summary>
        public float SkillTimer;

        /// <summary> AI 计时器   /// </summary>
        public float AITimer;

        [FormerlySerializedAs("AIData1")] public AITestData AIData;

        
        /// <summary> Animation   /// </summary>
        public AnimData AnimInfo;
        public AnimInternalData AnimInternalData;
        
        public override string ToString() {
            return __Data.ToString();
        }
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AITestData {
        public float TargetDeg;
        public float LerpInterval;
        public float LerpTimer;
    }
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimInternalData {
        public float4 Timer;
        public float4 LerpTimer;
        public int4 AnimId1;
        public int4 AnimId2;
        
    }
}