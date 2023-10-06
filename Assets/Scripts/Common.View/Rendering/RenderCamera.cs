using System;
using UnityEngine;

namespace GamesTan.Rendering {
    public class RenderCamera : MonoBehaviour {
        public Camera Camera;
        public void Start() {
            if (Camera == null) {
                Camera = GetComponentInChildren<Camera>();
            }

            WorldRenderer.Instance.SetMainCamera(Camera);
        }

        private void OnDestroy() {
            WorldRenderer.Instance.OnMainCameraDestroy(Camera);
        }

        void OnWillRenderObject() {
            WorldRenderer.Instance.OnCameraWillRenderObject(Camera);
        }
        void OnPreCull() {
            WorldRenderer.Instance.OnCameraPreCull(Camera);
        }
        void OnPreRender() {
            WorldRenderer.Instance.OnCameraPreRender(Camera);
        }
        void OnRenderObject() {
            WorldRenderer.Instance.OnCameraRenderObject(Camera);
        }
        void OnPostRender() {
            WorldRenderer.Instance.OnCameraPostRender(Camera);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            WorldRenderer.Instance.OnCameraRenderImage(Camera,source,destination);
            
        }
    }
}