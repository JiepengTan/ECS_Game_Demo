using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class DebugTestEntityView : MonoBehaviour {
        public GameEcsWorld World;
        public EntityData Entity;

        public void Update() {
            var entity = World.GetEnemy(Entity);
            if (entity != null) {
                transform.position = entity->Pos3;
                transform.eulerAngles = entity->Deg3;
            }
        }
    }
}