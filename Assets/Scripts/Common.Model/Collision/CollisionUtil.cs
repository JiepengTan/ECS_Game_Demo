using Lockstep.Math;
using Unity.Mathematics;

namespace GamesTan {
    public static class CollisionUtil {
        public static bool IsCollision(LVector3 pos1, LFloat radius1, LVector3 pos2, LFloat radius2) {
            var posDiff = pos1 - pos2;
            var distSqr = posDiff.x * posDiff.x + posDiff.z * posDiff.z;
            var radiusSum = radius1 + radius2;
            return distSqr < radiusSum * radiusSum;
        }
    }
}