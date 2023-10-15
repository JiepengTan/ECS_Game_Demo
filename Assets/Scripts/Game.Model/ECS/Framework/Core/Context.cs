using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using GamesTan.ECS;
using NetMsg.Common;

namespace GamesTan.ECS.Game {
    public unsafe partial class Context {
        protected override void OnAwake(){ }

        protected override void OnDestroy(){
        }
        protected override void OnProcessInputQueue(byte actorId, InputCmd cmd){
            TempFields.InputCmds[actorId] = cmd;
        }
        
    }
}