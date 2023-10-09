using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugTest : MonoBehaviour {
        public GameEcsWorld World;
        public EntityData Entity;

        public bool IsControlByEntity = true;
        public void Update() {
            var entity = World.EntityManager.GetEnemy(Entity);
            if (entity != null) {
                if (IsControlByEntity) {
                    transform.position = entity->TransformData.Pos;
                    transform.eulerAngles = entity->TransformData.Rot;
                    transform.localScale = entity->TransformData.Scale;
                }
                else {
                    entity->TransformData.Pos = transform.position;
                    entity->TransformData.Rot = transform.eulerAngles;
                    entity->TransformData.Scale = transform.localScale;
                }
            }
        }
    }
}