using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectDrawRenderPass : ScriptableRenderPass {
        protected IndirectRendererConfig Config;
        protected IndirectRendererRuntimeData RuntimeData;

        public void Clear() {
            RuntimeData = null;
        }
        public IndirectDrawRenderPass SetData( IndirectRendererConfig config,
            IndirectRendererRuntimeData runtimeData) {
            Config = config;
            RuntimeData = runtimeData;
            return this;
        }
        public IndirectDrawRenderPass Init(RenderPassEvent evt) {
            renderPassEvent = evt;
            return this;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (RuntimeData== null || !RuntimeData.m_isInitialized || !RuntimeData.m_isEnabled)
                return;
            OnExecute(context, ref renderingData);
        }

        protected virtual void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
        }
    }
}