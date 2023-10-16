
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
using Lockstep.Math;
using Unity.Burst;
using Unity.Collections;
#if false
namespace GamesTan.ECS.Game {
        

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public unsafe class UpdateViewStateSystem : JobComponentSystem {

        private void CopyState<T>(ref NativeEntityArray<T> ary,ref NativeArray<T> entityAry )where T : unmanaged, IEntity{
            if (ary.Length != 0) {
                if (entityAry.Length == ary._EntityAry.Length) {
                    NativeArray<T>.Copy(ary._EntityAry, entityAry);
                }
                else {
                    if (entityAry.Length != 0) {
                        entityAry.Dispose();
                    }
                    entityAry = new NativeArray<T>(ary._EntityAry, Allocator.Persistent);
                }
            }
            else {
                if (entityAry.Length != 0) {
                    entityAry.Dispose();
                }
            }
        }

        public static bool isInited = false;
        

        protected override JobHandle OnUpdate(JobHandle inputDeps){
            if (Context.Instance.HasInit) {
        
            }

            return inputDeps;
        }

        protected override void OnCreate(){
        
        }
    }
}
#endif                                                                                                                                                                                                                                                                                     