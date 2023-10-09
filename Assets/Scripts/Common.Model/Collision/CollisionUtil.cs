using Unity.Mathematics;

namespace GamesTan {
    public static class CollisionUtil {
        public static bool IsCollision(float3 pos1, float radius1, float3 pos2, float radius2) {
            var posDiff = pos1 - pos2;
            var distSqr = posDiff.x * posDiff.x + posDiff.z * posDiff.z;
            var radiusSum = radius1 + radius2;
            return distSqr < radiusSum * radiusSum;
        }
    }
}