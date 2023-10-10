using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe class EntityViewDebugBullet : MonoBehaviour {
        public GameEcsWorld World;
        public EntityRef Entity;

        public bool IsControlByEntity = true;

        private Camera mainCam;
        public void Update() {
            var entity = World.EntityManager.GetBullet(Entity);
            if (entity != null) {
                transform.localScale = entity->TransformData.Scale;
                if (IsControlByEntity) {
                    transform.position = entity->TransformData.Position;
                    transform.eulerAngles = entity->TransformData.Rotation;
                }
                else {
                    entity->TransformData.Position = transform.position;
                    entity->TransformData.Rotation = transform.eulerAngles;
                }
            }
            
            if (Input.GetMouseButton(0)) return;
            if (mainCam == null) {
                mainCam = Camera.main;
            }

            var forward = mainCam.transform.forward;
            var right = mainCam.transform.right;
            forward.y = 0;
            right.y = 0;
            forward = forward.normalized;
            right = right.normalized;

            Vector3 vec = Vector3.zero;
            if (Input.GetKey(KeyCode.D)) vec += right ;
            if (Input.GetKey(KeyCode.A)) vec -= right ;
            if (Input.GetKey(KeyCode.W)) vec += forward ;
            if (Input.GetKey(KeyCode.S)) vec -= forward ;
            vec = vec.normalized * Speed;
            transform.position += vec * Time.deltaTime;
        }

        public float Speed = 40;
    }
}