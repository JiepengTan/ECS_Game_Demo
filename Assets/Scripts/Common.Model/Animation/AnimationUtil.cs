using Lockstep.Math;

namespace GamesTan.ECS {
    public class AnimationUtil {
        public static int GetAnimation(int prefabId, int animId, LFloat timer) {
            // TODO load from config
            return animId * 100 + (int)(timer % 3 * 30);
        }
    }
}