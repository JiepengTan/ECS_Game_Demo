namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestBulletUpdatePos : BaseGameExecuteSystem {
        [CustomSystem]
        public void Execute(Bullet* entity) {
            var input = GlobalState.InputCmds[0];
            if (input.DirPressed) {
                entity->TransformData.Position += input.DirVec * 30*deltaTime ;
            }
        }
    }
}