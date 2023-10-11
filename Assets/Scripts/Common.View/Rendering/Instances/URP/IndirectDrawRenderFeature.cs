using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    [CreateAssetMenu]
    public class IndirectDrawRenderFeature : ScriptableRendererFeature {
        private List<IndirectDrawRenderPass> _passes = new List<IndirectDrawRenderPass>();

        public override void Create() {
            _passes.Clear();
            _passes.Add(new IndirectRenderPass_PrepareCulling().Init(RenderPassEvent.BeforeRenderingShadows));
            _passes.Add(new IndirectRenderPass_Sort().Init(RenderPassEvent.BeforeRenderingShadows));
            _passes.Add(new IndirectRenderPass_DrawShadow().Init(RenderPassEvent.AfterRenderingShadows));
            _passes.Add(new IndirectRenderPass_DrawOpacity().Init(RenderPassEvent.AfterRenderingOpaques));
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            var runtime = IndirectRendererRuntimeData.Instance;
            if (runtime == null) {
                return;
            }
            var config = runtime.Config;
            foreach (var pass in _passes) {
                pass.SetData(config, runtime);
                renderer.EnqueuePass(pass);
            }
        }

    }
}