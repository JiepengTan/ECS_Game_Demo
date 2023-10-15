using Lockstep.Game;

namespace GamesTan.ECS.Game {
    public class GameSystems : Systems {
        public GameSystems(BaseContext context, IServiceContainer services) {
            Add(new GameLogicSystems(context, services));
            Add(new GameRendererSystems(context, services));
        }
    }

    public class GameLogicSystems : Systems {
        public GameLogicSystems(BaseContext context, IServiceContainer services){ 
            // create
            Add(new SysTestEnemyCreate());
            Add(new SysTestBulletCreate());
            // init
            Add(new SysTestEnemyAwake());
            Add(new SysTestBulletAwake());
            // normal update
            Add(new SysTestEnemyUpdateAI());
            Add(new SysTestEnemyUpdateAnimation());
            Add(new SysTestBulletUpdateCollision());
        }


    }

    public class GameRendererSystems : Systems {
        public GameRendererSystems(BaseContext context, IServiceContainer services) {
            // render update
            Add(new SysTestUpdateSkinRender(){IsRenderSystem = true});
            Add(new SysTestUpdateMeshRender(){IsRenderSystem = true});
            IsRenderSystem = true;
        }
    }
}