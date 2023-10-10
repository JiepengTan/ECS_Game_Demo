// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using System.Collections.Generic;
using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    [InitEntityCount(5)]
    public partial class BoidSpawnerAAA : IEntity{
        public Transform3D Transform3D2;
        public Prefab Prefab;
        public SpawnData Spawn;
        public AssetData BoidPrefab;
        public BoidSpawnerTag Tag;

        public void TestCodeGen(){
            System.Console.WriteLine("TestCodeGen:  1");
        }
    }
    [InitEntityCount(2)]
    public partial class BoidSpawner : IEntity{
        public Transform3D Transform3D2;
        public Prefab Prefab;
        public SpawnData Spawn;
        public AssetData BoidPrefab;
        public BoidSpawnerTag Tag;

        public void TestCodeGen(){
            System.Console.WriteLine("TestCodeGen:  1");
        }
    }
    [InitEntityCount(10)]
    public partial class BoidCell : IEntity{
        public CellData Cell;
    }

    [InitEntityCount(10)]
    public partial class TestDemos : IEntity{
        public CellData Cell;
    }

    [InitEntityCount(20)]
    public partial class Boid: IEntity,IUpdateViewEntity{
        public Transform2D Transform2D;
        public Prefab Prefab;
        public BoidState State;
        public BoidTag Tag;
    }
    
    [InitEntityCount(2)]
    public partial class BoidTarget: IEntity,IBindViewEntity {
        public Transform3D Transform3D;
        public TargetMoveInfo MoveInfo;
        public BoidTargetTag Tag;
    }

    [InitEntityCount(2)]
    public partial class BoidObstacle :IEntity,IBindViewEntity,IUpdateViewEntity{
        public Transform3D Transform3D;
        public PlayerData Player;
        public SkillData Skill;
        public MoveData Move;
        public BoidObstacleTag Tag;
    }


    [InitEntityCount(20)]
    public partial class BoidTarget1: IEntity,IBindViewEntity {
        public Transform3D Transform;
        public TargetMoveInfo MoveInfo;
        public BoidTargetTag Tag;
    }
}