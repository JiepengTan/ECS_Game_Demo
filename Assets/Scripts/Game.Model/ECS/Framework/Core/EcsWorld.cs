
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Util;
using NetMsg.Common;
using GamesTan.ECS;

namespace GamesTan.ECS.Game {
    public partial class EcsWorld : World {
        protected Context _context;
        protected Systems _systems;
        public EcsWorld(IServiceContainer services){
            _serviceContainer = services;
            _context = new Context();
            _context._entityService = services.GetService<IEntityService>();
            _serviceContainer.GetService<IGlobalStateService>().Contexts = _context;
            _serviceContainer.GetService<IGlobalStateService>().World = this;
            var factory = services.GetService<IECSFactoryService>();
            _systems = factory.CreateSystems(_context, _serviceContainer) as Systems;
            _systems.DoAwake(_context, _serviceContainer);
            _context.DoAwake(_systems,_serviceContainer);
            Debug.Log("UnsafeWorld Constructor");
            //temp code
        }


        protected override void DoSimulateAwake(IServiceContainer serviceContainer, ManagerContainer mgrContainer){
            InitReference(serviceContainer, mgrContainer);
            DoAwake(serviceContainer);
            DoStart();
        }

        protected override void DoSimulateStart(){
            Debug.Log("DoSimulateStart");
            ProfilerUtil.BeginSample("UnsafeECS DoInitialize");
            _context.DoInitialize();
            ProfilerUtil.EndSample();
        }

        protected override void DoStep(bool isNeedGenSnap){
            ProfilerUtil.BeginSample("UnsafeECS Update");
            _context.DoLogicUpdate();
            ProfilerUtil.EndSample();
        }
        protected override void DoRenderUpdate(){
            ProfilerUtil.BeginSample("UnsafeECS Update");
            _context.DoRenderUpdate();
            ProfilerUtil.EndSample();
        }
        

        protected override void DoBackup(int tick){
            ProfilerUtil.BeginSample("_context.Backup");
            _context.Backup(tick);
            ProfilerUtil.EndSample();
            //Profiler.BeginSample("_context.DoCleanUselessSnapshot");
            //DoCleanUselessSnapshot(tick -1);
            //Profiler.EndSample();
        }
        protected override void DoRollbackTo(int tick, int missFrameTick, bool isNeedClear = true){
            _context.RollbackTo(tick,missFrameTick,isNeedClear);
        }
        protected override void DoCleanUselessSnapshot(int checkedTick){
            _context.CleanUselessSnapshot(checkedTick);
        }
        protected override void DoProcessInputQueue(byte actorId, InputCmd cmd){
            _context.ProcessInputQueue(actorId,cmd);
        }

    }
}