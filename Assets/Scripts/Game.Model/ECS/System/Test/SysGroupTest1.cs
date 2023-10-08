namespace GamesTan.ECS.Game {

    public class SysGroupTest1 : BaseGameSystemGroup {
        public SysGroupTest1() {
            // create
            AddSystem(new SysTestCreateEnemy());
            // init
            AddSystem(new SysTestEnemyAwake());
            // normal update
            AddSystem(new SysTestEnemyUpdateAI());
            AddSystem(new SysTestEnemyUpdateAnimation());
            // destroy
            AddSystem(new SysTestDestroyEnemy());
            // render update
            AddSystem(new SysTestEnemyUpdateRender());
        }
    }
}