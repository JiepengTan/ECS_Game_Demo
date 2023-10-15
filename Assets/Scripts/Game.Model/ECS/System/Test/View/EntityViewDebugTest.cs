using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugTest : MonoBehaviour {
        public Context Context;
        public EntityRef Entity;

        public bool IsControlByEntity = true;
        public void Update() {
            var entity = Context.GetEnemy(Entity);
            if (entity != null) {
                if (IsControlByEntity) {
                    transform.position = entity->TransformData.Position;
                    transform.eulerAngles = entity->TransformData.Rotation;
                    transform.localScale = entity->TransformData.Scale;
                }
                else {
                    entity->TransformData.Position = transform.position;
                    entity->TransformData.Rotation = transform.eulerAngles;
                    entity->TransformData.Scale = transform.localScale;
                }
            }
        }
    }
}