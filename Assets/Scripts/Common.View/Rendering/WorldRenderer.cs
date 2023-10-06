using System;
using UnityEngine;

namespace GamesTan.Rendering {
    public unsafe class WorldRenderer : MonoBehaviour {
        private static WorldRenderer _instance;
        public static WorldRenderer Instance => _instance;

        public Camera MainCamera { get; private set; }

        private IndirectRenderer _indirectRenderer;
        private HiZBuffer _hiZBuffer;

        private void Awake() {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _indirectRenderer = GetComponentInChildren<IndirectRenderer>();
            _hiZBuffer = GetComponentInChildren<HiZBuffer>();
        }

        private void OnDestroy() {
            _instance = null;
        }

        public void SetMainCamera(Camera cam) {
            MainCamera = cam;
        }

        public void OnMainCameraDestroy(Camera cam) {
            if (MainCamera == cam)
                MainCamera = null;
        }

        public void OnCameraWillRenderObject(Camera cam) {
        }

        public void OnCameraPreCull(Camera cam) {
            if (_indirectRenderer != null) _indirectRenderer.OnCameraPreCull();
        }

        public void OnCameraPreRender(Camera cam) {
            if (_hiZBuffer != null) _hiZBuffer.OnCameraPreRender();
        }

        public void OnCameraRenderObject(Camera cam) {
        }

        public void OnCameraPostRender(Camera cam) {
        }

        public void OnCameraRenderImage(Camera cam, RenderTexture source, RenderTexture destination) {
            bool hasDeal = false;
            if (_hiZBuffer != null) hasDeal |= _hiZBuffer.OnCameraRenderImage(source, destination);
            if (!hasDeal) {
                Graphics.Blit(source, destination);
            }
        }
    }
}