using System;
using GamesTan.ECS.Game;
using UnityEngine;

namespace Lockstep.Game {
    public class UnityInputManager : MonoBehaviour {
        private Camera _mainCam;
        public void Update() {
            
            if (_mainCam == null) {
                _mainCam = Camera.main;
            }

            var input = GameInputService.CurGameInput;
            input.Dir = 0;
            input.SkillId.ClearAll();
            
            if (Input.GetMouseButton(0)) return;
            
            var forward = _mainCam.transform.forward;
            var right = _mainCam.transform.right;
            forward.y = 0;
            right.y = 0;
            forward = forward.normalized;
            right = right.normalized;

            Vector3 vec = Vector3.zero;
            if (Input.GetKey(KeyCode.D)) vec += right ;
            if (Input.GetKey(KeyCode.A)) vec -= right ;
            if (Input.GetKey(KeyCode.W)) vec += forward ;
            if (Input.GetKey(KeyCode.S)) vec -= forward ;
            if (vec.magnitude > 0.01f) {
                input.DirPressed = true;
            }

            vec = vec.normalized ;

            var deg = (Mathf.Atan2(vec.z, vec.x) * Mathf.Rad2Deg) % 360;
            if (deg < 0) {
                deg += 360;
            }
            var degInt = (int)(deg * 100);
            input.Dir = (ushort)degInt;
        }
    }
}