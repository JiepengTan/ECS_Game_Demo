using Lockstep.Math;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugTest : MonoBehaviour {
        public Context Context;
        public EntityRef Entity;

        public bool IsControlByEntity = true;
        public void Update() {
            var entity = Context.GetEnemy(Entity);
            if (entity != null) {
                transform.position = entity->TransformData.Position.ToVector3();
                transform.eulerAngles = entity->TransformData.Rotation.ToVector3();
                transform.localScale = entity->TransformData.Scale.ToVector3();
            }
        }
    }
}