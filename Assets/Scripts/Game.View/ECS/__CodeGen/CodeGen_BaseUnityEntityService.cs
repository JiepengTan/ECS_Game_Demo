
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
    using Unity.Mathematics;
    using UnityEngine;
    using Lockstep.Game;
    using Debug = Lockstep.Logging.Debug;
    public abstract unsafe partial class BaseUnityEntityService : BaseService,IEntityService {
      
        protected Dictionary<int, BaseEntityView> _id2EntityView = new Dictionary<int, BaseEntityView>();

        public void OnEntityCreated(Context f, GamesTan.ECS.Entity* pEntity){
            if (pEntity == null) {
                Debug.LogError("OnEntityCreated null");
                return;
            }

            var pPrefab = EntityUtil.GetAssetData(pEntity);
            if (pPrefab == null) return;

            var assetId = pPrefab->AssetId;
            if (assetId == 0) return;
           
            if (!EntityViewUtil.HasView(pEntity)) {
                return;
            }
            var transform = EntityUtil.GetTransform3D(pEntity);
            if (transform == null) return;
            
            //bind view
            Debug.Assert(!_id2EntityView.ContainsKey(pEntity->_localId));
            var view = EntityViewUtil.BindEntityView(pEntity,assetId,transform);
            view.BindEntity(pEntity);
            view.OnBindEntity();
            _id2EntityView[pEntity->_localId] = view;
        }

        public void OnEntityDestroy(Context f, GamesTan.ECS.Entity* pEntity){
            if (_id2EntityView.TryGetValue(pEntity->LocalId, out var view)) {
                // ReSharper disable once Unity.NoNullPropogation
                view?.OnUnbindEntity();
                view?.UnbindEntity();
                _id2EntityView.Remove(pEntity->LocalId);
            }
        }
        public virtual void OnPClassACreated(Context context, PClassA* entity){}
        public virtual void OnPClassADestroy(Context context, PClassA* entity){}
        public virtual void OnSubClassACreated(Context context, SubClassA* entity){}
        public virtual void OnSubClassADestroy(Context context, SubClassA* entity){}
        public virtual void OnSubClassBCreated(Context context, SubClassB* entity){}
        public virtual void OnSubClassBDestroy(Context context, SubClassB* entity){}
        public virtual void OnSubClassBCCreated(Context context, SubClassBC* entity){}
        public virtual void OnSubClassBCDestroy(Context context, SubClassBC* entity){}
        public virtual void OnEnemyCreated(Context context, Enemy* entity){}
        public virtual void OnEnemyDestroy(Context context, Enemy* entity){}
        public virtual void OnBulletCreated(Context context, Bullet* entity){}
        public virtual void OnBulletDestroy(Context context, Bullet* entity){}
        public virtual void OnBulletEmitterCreated(Context context, BulletEmitter* entity){}
        public virtual void OnBulletEmitterDestroy(Context context, BulletEmitter* entity){} 
    }
}                                                                                                                                                                                                                                                                                     