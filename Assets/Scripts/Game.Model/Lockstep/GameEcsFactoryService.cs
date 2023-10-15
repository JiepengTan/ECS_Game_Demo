using Lockstep.Game;
using GamesTan.ECS.Game;
using GamesTan.ECS;

namespace GamesTan.ECS.Game {
    public class GameEcsFactoryService : IECSFactoryService {
        private static Context _lastInstance;
        public object CreateSystems(object contexts, IServiceContainer services){
            return new GameSystems(contexts as Context,services) ;
        }
        
        public object CreateContexts(){
            var  ctx = _lastInstance == null ? Context.Instance : new Context();
            _lastInstance = ctx;
            return ctx;
        }
    }
}