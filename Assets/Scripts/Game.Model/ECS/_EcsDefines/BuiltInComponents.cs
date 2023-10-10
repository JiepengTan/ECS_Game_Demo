using GamesTan.UnsafeECSDefine;
namespace GamesTan.UnsafeECSDefineBuiltIn {

    public partial struct TransformData :IBuildInComponent{
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public override string ToString() {
            return $"pos{Position} rot:{Rotation} scale:{Scale}";
        }
    }
    
    public partial struct AnimRenderData:IBuildInComponent {
        /// <summary>
        /// x: AnimFactor
        /// y: FrameLerpFactor
        /// z: FrameIdx
        /// w: useless
        /// </summary>
        public Vector4 AnimInfo0;
        public Vector4 AnimInfo1;
        public Vector4 AnimInfo2;
        public Vector4 AnimInfo3;
    }
    
    public partial struct AssetData : IBuildInComponent {
        /// <summary> Prefab Id   /// </summary>
        public int PrefabId;
        /// <summary> Prefab 下标，用于Instance 批量渲染   /// </summary>
        public int InstancePrefabIdx;
    }
    
    public partial struct PhysicData : IBuildInComponent {
        public Vector2Int GridCoord;
        /// <summary> 半径   /// </summary>
        public float Radius;
        /// <summary> 速度   /// </summary>
        public float Speed;
        /// <summary> 旋转速度   /// </summary>
        public float RotateSpeed ;
    }
    
    public partial struct BasicData : IBuildInComponent {
        /// <summary> GameObject Id   /// </summary>
        public int GObjectId;
        /// <summary> 状态集合   /// </summary>
        public Bitset32 StatusData;
    }
}