
using GamesTan.ECS;
using Lockstep.Math;
using Lockstep.Serialization;
using Unity.Mathematics;
using UnityEngine;

namespace Lockstep.Game {
    public partial class GamePlayerInput : BaseFormater,IBaseComponent {
        public static GamePlayerInput Empty = new GamePlayerInput();
        public ushort Dir;
        public Bitset32 SkillId;

        public bool DirPressed {
            get => SkillId.Is(0);
            set => SkillId.Set(0, value);
        }

        public LVector3 DirVec {
            get {
                var deg =  (int)Dir * LMath.Deg2Rad * new LFloat(null, 10);
                return new LVector3(LMath.Cos(deg), 0,LMath.Sin( deg));
            }
        }
        public override bool Equals(object obj){
            if (ReferenceEquals(this,obj)) return true;
            var other = obj as GamePlayerInput;
            return Equals(other);
        }
        
        public override void Serialize(Serializer writer){
            writer.Write(Dir);
            writer.Write(SkillId.bits);
        }

        public void Reset(){
            Dir = 0;
            SkillId.bits = 0;
        }


        public override void Deserialize(Deserializer reader){
            Dir = reader.ReadUInt16();
            SkillId.bits = reader.ReadUInt32();
        }

        public bool Equals(GamePlayerInput other){
            if (other == null) return false;
            if (Dir != other.Dir) return false;
            if (SkillId.bits != other.SkillId.bits) return false;
            return true;
        }

        public GamePlayerInput Clone(){
            var tThis = this;
            return new GamePlayerInput() {
                SkillId = tThis.SkillId,
                Dir = tThis.Dir,
            };
        }
    }
}