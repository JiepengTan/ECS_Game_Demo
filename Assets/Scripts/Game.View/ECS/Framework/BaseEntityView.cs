using Lockstep.Game;
using Lockstep.Math;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class BaseEntityView : MonoBehaviour {
        protected const float _LerpVal = 0.5f;
    	public void Update(){ DoUpdate(Time.deltaTime); }
        public virtual void BindEntity(Entity* entityPtr){ }
        public virtual void OnBindEntity(){ }

        public virtual void OnUnbindEntity(){ }
        public virtual void UnbindEntity(){GameObject.Destroy(gameObject);}
        public virtual void RebindEntity(Entity* newEntityPtr){ }
        public virtual void DoUpdate(float deltaTime){}
        protected virtual void OnBind(){ }

        public virtual void OnSkillFire(LFloat range){}
        public virtual void OnSkillDone(LFloat range){}

        protected void UpdatePosRot(ref Transform3D  transform3D){
            var targetPos = transform3D.Position.ToVector3();
            transform.position = Vector3.Lerp(transform.position, targetPos, _LerpVal);
            transform.rotation =  Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform3D.Rotation.ToVector3()), _LerpVal);
            transform.localScale =  transform3D.Scale.ToVector3(); 
        }
    }
}