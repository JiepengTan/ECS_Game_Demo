using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugTest : MonoBehaviour {
        public GameEcsWorld World;
        public EntityData Entity;

        public void Update() {
            var entity = World.EntityManager.GetEnemy(Entity);
            if (entity != null) {
                transform.position = entity->TransformData.Pos;
                transform.eulerAngles = entity->TransformData.Rot;
            }
        }
    }
}