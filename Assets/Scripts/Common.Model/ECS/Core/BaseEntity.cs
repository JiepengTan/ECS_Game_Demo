using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct BaseEntity : IEntity {
        /// <summary> Entity Data   /// </summary>
        public EntityRef __Data;
        public EntityRef __EntityData {
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
            get => TransformData.Rotation.y;
            set => TransformData.Rotation.y = value;
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
    }
}