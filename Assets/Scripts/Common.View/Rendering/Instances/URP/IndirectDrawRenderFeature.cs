using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    [CreateAssetMenu]
    public class IndirectDrawRenderFeature : ScriptableRendererFeature {
        public IndirectRendererConfig Config;
        private IndirectRendererRuntimeData _runtime;
        public RenderPassEvent evt;
        private IndirectDrawRenderPass pass;
        private bool reinit = false;

        public IndirectDrawRenderFeature() {
            reinit = true;
        }

        public override void Create() {
            //Create resources
            CleanUp();
            _runtime = new IndirectRendererRuntimeData(Config);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (reinit) {
                reinit = false;
                Create();
            }

            pass = new IndirectDrawRenderPass(RenderPassEvent.AfterRenderingShadows, Config, _runtime);
            renderer.EnqueuePass(pass);
        }

        public void CleanUp() {
            //Clean up
            if (_runtime != null) 
                _runtime.ReleaseBuffers();
            _runtime = null;
        }

        public void OnDisable() {
            CleanUp();
            reinit = true;
        }
    }
}