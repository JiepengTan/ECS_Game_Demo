// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using System.Collections.Generic;
using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    [InitEntityCount(1)]
    public partial class PClassA : BaseGameEntity,IEntity,IVirtualClass {
        public int Val1;

    }
    [InitEntityCount(1)]
    public partial class SubClassA : PClassA {
        public float Val2;

    }
    [InitEntityCount(1)]
    public partial class SubClassB : PClassA {
        public long Val3;
    }
    
    [InitEntityCount(1)]
    [Abstract]
    public partial class BaseGameEntity  {
        public AssetData AssetData;
        public Transform3D TransformData;
        public BasicData BasicData;
        public PhysicData PhysicData;

        public void TestCodeGen(){
            System.Console.WriteLine("TestCodeGen:  1");
        }
    }
    [InitEntityCount(1)]
    public partial class Enemy : BaseGameEntity,IEntity{
        public EnemyTag EnemyTag;
        public UnitData UnitData;
        public AIData AIData;
        public AnimRenderData AnimRenderData;
        public AnimData AnimData;

    }
    
    [InitEntityCount(1)]
    public partial class Bullet :BaseGameEntity,IEntity, IBindViewEntity{
        public BulletTag BulletTag;
        public MeshRenderData MeshRenderData;
        public UnitData UnitData;
    }
    
    [InitEntityCount(1)]
    public partial class BulletEmitter: BaseGameEntity,IEntity  {
        public SpawnerTag SpawnerTag;
        public MeshRenderData MeshRenderData;
        public EmitterData EmitterData;
    }

}