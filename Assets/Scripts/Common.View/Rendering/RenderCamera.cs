using System;
using UnityEngine;

namespace GamesTan.Rendering {
    public class RenderCamera : MonoBehaviour {
        public Camera Camera;
        public void Start() {
            if (Camera == null) {
                Camera = GetComponentInChildren<Camera>();
            }

            RenderWorld.Instance.SetMainCamera(Camera);
        }

        private void OnDestroy() {
            RenderWorld.Instance?.OnMainCameraDestroy(Camera);
        }

        void OnWillRenderObject() {
            RenderWorld.Instance.OnCameraWillRenderObject(Camera);
        }
        void OnPreCull() {
            RenderWorld.Instance.OnCameraPreCull(Camera);
        }
        void OnPreRender() {
            RenderWorld.Instance.OnCameraPreRender(Camera);
        }
        void OnRenderObject() {
            RenderWorld.Instance.OnCameraRenderObject(Camera);
        }
        void OnPostRender() {
            RenderWorld.Instance.OnCameraPostRender(Camera);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            RenderWorld.Instance.OnCameraRenderImage(Camera,source,destination);
            
        }
    }
}