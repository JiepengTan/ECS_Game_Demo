using System.Runtime.InteropServices;
using Lockstep.NativeUtil;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace GamesTan.ECS.Game {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Bullet : IEntity {
        /// <summary> Entity Data   /// </summary>
        public EntityData __Data;
        public EntityData __EntityData {
            get => __Data;
            set => __Data = value;
        }
        public bool IsValid => __Data.Version > 0;

        public BasicData BasicData;
        public AssetData AssetData;
        public PhysicData PhysicData;
        public TransformData TransformData;
        
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId {
            get => BasicData.GObjectId;
            set => BasicData.GObjectId = value;
        }
        
        /// <summary> 是否已经释放   /// </summary>
        public bool IsAlreadyStart {
            get => BasicData.StatusData.Is(0);
            set => BasicData.StatusData.Set(0, value);
        }

        /// <summary> 旋转   /// </summary>
        public float DegY {
            get => TransformData.Rot.y;
            set => TransformData.Rot.y = value;
        }
        public float Radius {
            get => PhysicData.Radius;
            set => PhysicData.Radius = value;
        }
        public float3 Pos3 {
            get => TransformData.Pos;
            set => TransformData.Pos = value;
        }
        public float3 Rot3 {
            get => TransformData.Rot;
            set => TransformData.Rot = value;
        }
        public float3 Scale3 {
            get => TransformData.Scale;
            set => TransformData.Scale = value;
        }
        
        public float3 Forward {
            get {
                float deg = math.radians(-DegY + 90);
                return new float3(math.cos(deg), 0, math.sin(deg));
            }
        }

        public float2 Forward2 {
            get {
                float deg = math.radians(-DegY + 90);
                return new float2(math.cos(deg), math.sin(deg));
            }
        }
        public float Scale {
            get => TransformData.Scale.x;
            set {
                TransformData.Scale = new float3(2, 2, 2) * value;
                PhysicData.Radius = value;
            }
        }
    }


    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Enemy : IEntity {
        /// <summary> Entity Data   /// </summary>
        public EntityData __Data;
        public EntityData __EntityData {
            get => __Data;
            set => __Data = value;
        }
        public bool IsValid => __Data.Version > 0;

        public BasicData BasicData;
        public AssetData AssetData;
        public PhysicData PhysicData;
        public TransformData TransformData;
        
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId {
            get => BasicData.GObjectId;
            set => BasicData.GObjectId = value;
        }
        
        /// <summary> 是否已经释放   /// </summary>
        public bool IsAlreadyStart {
            get => BasicData.StatusData.Is(0);
            set => BasicData.StatusData.Set(0, value);
        }

        /// <summary> 旋转   /// </summary>
        public float DegY {
            get => TransformData.Rot.y;
            set => TransformData.Rot.y = value;
        }

        public float3 Forward {
            get {
                float deg = math.radians(-DegY + 90);
                return new float3(math.cos(deg), 0, math.sin(deg));
            }
        }

        public float2 Forward2 {
            get {
                float deg = math.radians(-DegY + 90);
                return new float2(math.cos(deg), math.sin(deg));
            }
        }
        public float Radius {
            get => PhysicData.Radius;
            set => PhysicData.Radius = value;
        }
        public float3 Pos3 {
            get => TransformData.Pos;
            set => TransformData.Pos = value;
        }
        public float3 Rot3 {
            get => TransformData.Rot;
            set => TransformData.Rot = value;
        }
        public float3 Scale3 {
            get => TransformData.Scale;
            set => TransformData.Scale = value;
        }
        public float Scale {
            get => TransformData.Scale.x;
            set {
                TransformData.Scale = new float3(2, 2, 2) * value;
                PhysicData.Radius = value;
            }
        }
    }
}