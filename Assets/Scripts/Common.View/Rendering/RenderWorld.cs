using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamesTan.Rendering {
    public unsafe class RenderWorld : MonoBehaviour {
        private static RenderWorld _instance;
        public static RenderWorld Instance => _instance;

        public Camera MainCamera { get; private set; }

        private IndirectRenderer _indirectRenderer;
        private HiZBuffer _hiZBuffer;
        public InstanceRenderData RendererData =>_indirectRenderer.RendererData;
        public int GetInstancePrefabIdx(int prefabId) =>_indirectRenderer.GetInstancePrefabIdx(prefabId);
        
        private void Awake() {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _indirectRenderer = GetComponentInChildren<IndirectRenderer>();
            _hiZBuffer = GetComponentInChildren<HiZBuffer>();
            _indirectRenderer.DoAwake();
        }

        

        private void Update() {
            _hiZBuffer.DoUpdate();
            _indirectRenderer.DoUpdate();
        }

        private void OnDestroy() { 
            _indirectRenderer.DoDestroy();
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