using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugBullet : MonoBehaviour {
        public GameEcsWorld World;
        public EntityRef Entity;

        public bool IsControlByEntity = true;

        private Camera mainCam;

        public float Speed = 40;
    }
}