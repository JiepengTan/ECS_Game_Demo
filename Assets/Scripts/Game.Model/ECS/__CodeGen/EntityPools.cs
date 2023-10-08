using System.Collections;
using TItem = GamesTan.ECS.Game.Enemy;


namespace GamesTan.ECS.Game {
    public unsafe class NativePoolEnemy :NativeEntityPool<Enemy>{
    }
    public unsafe class NativePoolBullet :NativeEntityPool<Bullet>{
    }
}