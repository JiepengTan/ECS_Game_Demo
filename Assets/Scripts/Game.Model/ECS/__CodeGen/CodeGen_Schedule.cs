
//------------------------------------------------------------------------------    
// <auto-generated>                                                                 
//     This code was generated by GamesTan.Tools.MacroExpansion, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null. 
//     https://github.com/JiepengTan/LockstepECS                                         
//     Changes to this file may cause incorrect behavior and will be lost if        
//     the code is regenerated.                                                     
// </auto-generated>                                                                
//------------------------------------------------------------------------------  

//Power by ME //src: https://github.com/JiepengTan/ME  

//#define DONT_USE_GENERATE_CODE                                                                 
                                                                                                 
using System.Linq;                                                                               
using System.Runtime.InteropServices;                                                            
using System;                                                                                    
using System.Collections.Generic;                                                                
using System.Collections;                                                                        
using System.Runtime.CompilerServices;                                                           
using Lockstep.Game;                                                                             
using Lockstep.Math;                                                                             
using Unity.Burst;                                                                               
using Unity.Mathematics;                                                                                                                                                                            
namespace GamesTan.ECS.Game {  
    public unsafe partial class Context {    
        
        private void RegisterSystemFunctions(){
            _RegisterExecuteSystemFunc();
            _RegisterPostScheduleSystemFunc();
        }
        public void _RegisterExecuteSystemFunc(){
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestBulletAwake),ScheduleSysTestBulletAwake);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestBulletUpdateCollision),ScheduleSysTestBulletUpdateCollision);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestBulletUpdatePos),ScheduleSysTestBulletUpdatePos);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestEnemyAwake),ScheduleSysTestEnemyAwake);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestEnemyUpdateAI),ScheduleSysTestEnemyUpdateAI);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestEnemyUpdateAnimation),ScheduleSysTestEnemyUpdateAnimation);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestUpdateMeshRender),ScheduleSysTestUpdateMeshRender);
            _RegisterScheduleSystemFunc(typeof(GamesTan.ECS.Game.SysTestUpdateSkinRender),ScheduleSysTestUpdateSkinRender); 
        }
        public void _RegisterPostScheduleSystemFunc(){
            _RegisterPostScheduleSystemFunc(EntityIds.PClassA,PostUpdateCreatePClassA);
            _RegisterPostScheduleSystemFunc(EntityIds.SubClassA,PostUpdateCreateSubClassA);
            _RegisterPostScheduleSystemFunc(EntityIds.SubClassB,PostUpdateCreateSubClassB);
            _RegisterPostScheduleSystemFunc(EntityIds.SubClassBC,PostUpdateCreateSubClassBC);
            _RegisterPostScheduleSystemFunc(EntityIds.Enemy,PostUpdateCreateEnemy);
            _RegisterPostScheduleSystemFunc(EntityIds.Bullet,PostUpdateCreateBullet);
            _RegisterPostScheduleSystemFunc(EntityIds.BulletEmitter,PostUpdateCreateBulletEmitter); 
        }

    }
}                                                                                                                                                                                                                                                                                     