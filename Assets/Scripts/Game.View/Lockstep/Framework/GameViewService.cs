using GamesTan.Rendering;

namespace Lockstep.Game {
    public class GameViewService : BaseViewService {
        public override void OnFrameStart() {
            RenderWorld.Instance.RendererData.OnFrameStart();
        }

        public override void OnFrameEnd() {
            RenderWorld.Instance.RendererData.OnFrameEnd();
        }
    }
}