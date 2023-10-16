using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using NetMsg.Common;

namespace Lockstep.Game {		
    public partial interface IGameStateService : IService  { 
        bool IsPlaying{get; set; }   
        bool IsGameOver{get; set; }   
        byte LocalEntityId{get; set; }   
        int CurEnemyCount{get; set; }   
        int CurScore{get; set; }   
        LFloat CurScale{get; set; }                                                                                                    
    }

    public class GameInputService : IInputService {
        public static GamePlayerInput CurGameInput = new GamePlayerInput();

        public void Execute(InputCmd cmd, object entity){
            var input = new Deserializer(cmd.content).Parse<GamePlayerInput>();
            var playerInput = entity as GamePlayerInput;
            playerInput.SkillId = input.SkillId;
            playerInput.Deg = input.Deg;
            //Debug.Log("InputUV  " + input.inputUV);
        }

        public List<InputCmd> GetInputCmds(){
#if !UNITY_EDITOR
            CurGameInput.Deg = ((ushort)(short)1);
#endif
            return new List<InputCmd>() {
                new InputCmd() {
                    content = CurGameInput.ToBytes()
                }
            };
        }

        public List<InputCmd> GetDebugInputCmds(){
            return new List<InputCmd>() {
                new InputCmd() {
                    content = new GamePlayerInput() {
                        Deg = (ushort)LRandom.Range(0,4),
                        SkillId = (ushort)LRandom.Range(0,3)
                    }.ToBytes()
                }
            };
        }
    }
}