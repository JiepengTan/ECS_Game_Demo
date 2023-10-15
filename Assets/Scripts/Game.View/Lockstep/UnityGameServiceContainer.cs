

using Lockstep.Game;

namespace GamesTan.ECS.Game {
    public class UnityGameServiceContainer : ServiceContainer {
        public UnityGameServiceContainer() : base() {
            //basic service 
            RegisterService(new EventRegisterService());
            RegisterService(new ManagerContainer());
            RegisterService(new SimulatorService());
            
            //game service
            RegisterService(new GameGlobalStateService());
            RegisterService(new GameEcsFactoryService());
            RegisterService(new GameViewService());
            RegisterService(new UnityGameEntityService());
            RegisterService(new GameInputService());
            
            //Code Gen service
            // unity service 
        }
    }
}