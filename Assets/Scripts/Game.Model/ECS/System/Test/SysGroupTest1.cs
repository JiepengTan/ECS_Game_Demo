namespace GamesTan.ECS.Game {

    public class SysGroupTest1 : BaseGameSystemGroup {
        public SysGroupTest1() {
            AddSystem(new SysTestCreateEnemy());
            AddSystem(new SysTestEnemyAwake());
            AddSystem(new SysTestEnemyUpdatePos());
            AddSystem(new SysTestEnemyUpdateAnimation());
            AddSystem(new SysTestDestroyEnemy());
            AddSystem(new SysTestRenderUpload());
            
        }
    }
}