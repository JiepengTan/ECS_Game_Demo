// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using System;
using System.Collections.Generic;
using GamesTan.UnsafeECSDefine;

namespace GamesTan.UnsafeECSDefine {
    public class CollisionConfig { }
    public class Msg_G2C_GameStartInfo { }
    public class ConfigSpawnInfo{}
    public class ConfigObstacleInfo{}
    public class ConfigBoidSharedData{}
    public class ConfigSkillData{}
    public class ConfigTargetInfo{}
    
    public partial class GameStateService : IServiceState,IGameStateService {
        // states
        public bool IsPlaying;
        public bool IsGameOver;
        public byte LocalEntityId; 

        // volatile states
        public int CurEnemyCount;
        public int CurScore;
        public float CurScale;
        
    }

    public partial class GameConfigService : IServiceState,IGameConfigService {
        public string RelPath;
        public string RecorderFilePath;
        public string DumpStrPath;

        public int InitScale;
        public int MaxPlayerCount;

        public CollisionConfig CollisionConfig;
        public Msg_G2C_GameStartInfo ClientModeInfo;
        public List<ConfigTargetInfo> TargetInfos;
        public List<ConfigSpawnInfo> SpawnInfos;
        public List<ConfigObstacleInfo> ObstacleInfos;
        public ConfigBoidSharedData BoidSettting;
    }
}