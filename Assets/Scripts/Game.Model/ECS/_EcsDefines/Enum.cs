// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

namespace GamesTan.UnsafeECSDefine {
  
    public enum ECampType  {
        Player,
        Enemy,
        Other,
    }
    public enum EItemType {
        AddLife,
        Boom,
        Upgrade,
        EnumCount,
    }
    public enum EDir {
        Up =0 ,
        Left = 1,
        Down = 2,
        Right = 3,
        EnumCount = 4,
    }
}