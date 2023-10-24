using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using GamesTan.ECS;
using NetMsg.Common;

namespace GamesTan.ECS.Game {
    public unsafe partial class Context {
        private GameGlobalStateService _globalState;

        protected override void OnAwake() {
            _globalState =  _services.GetService<IGlobalStateService>() as GameGlobalStateService;
        }

        protected override void OnDestroy(){
        }
        protected override void OnProcessInputQueue(byte actorId, InputCmd cmd) {
            _globalState.InputCmds[actorId].FromBytes(cmd.content);
        }
        
    }
}