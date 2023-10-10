// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using System.Collections.Generic;
using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {

    [InitEntityCount(1)]
    public partial class BaseGameEntity : IEntity {
        public BasicData BasicData;
        public AssetData AssetData;
        public PhysicData PhysicData;
        public TransformData TransformData;

        public void TestCodeGen(){
            System.Console.WriteLine("TestCodeGen:  1");
        }
    }
    [InitEntityCount(1)]
    public partial class Enemy : BaseGameEntity {
        public EnemyTag EnemyTag;
        public UnitData UnitData;
        public AIData AIData;
        public AnimRenderData AnimRenderData;
        public AnimData AnimInternalData;
        
    }
    
    [InitEntityCount(1)]
    public partial class Bullet :BaseGameEntity{
        public BulletTag BulletTag;
        public UnitData UnitData;
    }
    
    [InitEntityCount(1)]
    public partial class BulletEmitter: IEntity  {
        public SpawnerTag SpawnerTag;
        public EmitterData EmitterData;
    }

}