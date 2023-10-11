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
        private InstanceRenderData _rendererData = new InstanceRenderData();
        public InstanceRenderData RendererData => _rendererData;

        [Header("Instances Config")]
        public InstanceConfig InstanceConfig;
        public List<Vector2Int> InitInstanceCount = new List<Vector2Int>();
        public Dictionary<int, int> _prefabId2InstancePrefabIdx = new Dictionary<int, int>();
        private void Awake() {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _indirectRenderer = GetComponentInChildren<IndirectRenderer>();
            _hiZBuffer = GetComponentInChildren<HiZBuffer>();
            var dict = new Dictionary<int, int>();
            foreach (var info in InitInstanceCount) {
                dict[info.x] = info.y;
            }
            var configs = InstanceConfig.Infos.Where(a => dict.ContainsKey(a.prefabId)).ToList();
            var counts = configs.Select(a => dict[a.prefabId]).ToList();
            int totalCount = counts.Sum();
            _rendererData.ResetLayout(configs.Select(a=>a.prefabId).ToList(),counts,true);
            _indirectRenderer.DoAwake(_rendererData,configs);
            for (int i = 0; i < configs.Count; i++) {
                _prefabId2InstancePrefabIdx[configs[i].prefabId] = i;
            }
        }

        public int GetInstancePrefabIdx(int prefabId) {
            if (_prefabId2InstancePrefabIdx.TryGetValue(prefabId, out var idx))
                return idx;
            return -1;
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