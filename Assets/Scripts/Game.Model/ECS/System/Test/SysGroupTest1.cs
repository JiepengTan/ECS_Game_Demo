namespace GamesTan.ECS.Game {

    public class SysGroupTest1 : BaseGameSystemGroup {
        public SysGroupTest1() {
            // create
            AddSystem(new SysTestCreateEnemy());
            AddSystem(new SysTestCreateBullet());
            // init
            AddSystem(new SysTestEnemyAwake());
            AddSystem(new SysTestBulletAwake());
            // normal update
            AddSystem(new SysTestEnemyUpdateAI());
            AddSystem(new SysTestBulletUpdateCollision());
            AddSystem(new SysTestEnemyUpdateAnimation());
            // destroy
            AddSystem(new SysTestDestroyEnemy());
            // render update
            AddSystem(new SysTestEnemyUpdateRender());
        }
    }
}