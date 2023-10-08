using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugTest : MonoBehaviour {
        public GameEcsWorld World;
        public EntityData Entity;

        public void Update() {
            var entity = World.GetEnemy(Entity);
            if (entity != null) {
                transform.position = entity->Pos;
                transform.eulerAngles = entity->Rot;
            }
        }
    }
}