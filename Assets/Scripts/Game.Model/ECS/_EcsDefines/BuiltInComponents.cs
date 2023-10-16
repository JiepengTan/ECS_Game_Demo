
namespace GamesTan.UnsafeECSDefine {
    public partial class MeshRenderData : IBuiltInComponent {
        public int Padding;
    }

    public partial class AnimRenderData:IBuiltInComponent {
        [ToolTips("w: useless \nx: AnimFactor \ny: FrameLerpFactor\nz: FrameIdx")]
        public Vector4 AnimInfo0;
        public Vector4 AnimInfo1;
        public Vector4 AnimInfo2;
        public Vector4 AnimInfo3;
    }
    

    
    public partial class PhysicData : IBuiltInComponent {
        public Vector2Int GridCoord;
        [ToolTips("半径")]
        public float Radius;
        [ToolTips("速度")]
        public float Speed;
        [ToolTips("旋转速度")]
        public float RotateSpeed ;
    }
    
    public partial class BasicData : IBuiltInComponent {
        [ToolTips("GameObject Id")]
        public int GObjectId;
        [ToolTips("状态集合")]
        public Bitset32 StatusData;
    }
}