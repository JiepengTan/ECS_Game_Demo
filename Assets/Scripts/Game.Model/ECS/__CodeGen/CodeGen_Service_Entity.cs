
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
using Unity.Burst;                                                                               
using Lockstep.Math;                                                                             
using Unity.Mathematics;                                                                                                                                                                            
namespace GamesTan.ECS.Game {  
    public unsafe partial interface IEntityService {
        void OnEnemyCreated(Context context, Enemy* entity);
        void OnEnemyDestroy(Context context, Enemy* entity);
        void OnBulletCreated(Context context, Bullet* entity);
        void OnBulletDestroy(Context context, Bullet* entity);
        void OnBulletEmitterCreated(Context context, BulletEmitter* entity);
        void OnBulletEmitterDestroy(Context context, BulletEmitter* entity); 
    }

    public unsafe partial class PureEntityService :IEntityService {
      public void OnEnemyCreated(Context context, Enemy* entity){}
      public void OnEnemyDestroy(Context context, Enemy* entity){}
      public void OnBulletCreated(Context context, Bullet* entity){}
      public void OnBulletDestroy(Context context, Bullet* entity){}
      public void OnBulletEmitterCreated(Context context, BulletEmitter* entity){}
      public void OnBulletEmitterDestroy(Context context, BulletEmitter* entity){} 
    }
}                                                                                                                                                                                                                                                                                     