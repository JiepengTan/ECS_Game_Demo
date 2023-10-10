// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using GamesTan.UnsafeECSDefine;

namespace  GamesTan.UnsafeECSDefine {
	
    public class OnSkillFire: IEvent{
   	   	public SkillData SkillData;
    }
  	public class OnSkillDone: IEvent{
   	   	public SkillData SkillData;
   	   	public int SS1;
   	   	public string SS4;
    }
    public class OnSkillDone2: OnSkillFire{
        public SkillData SkillData2;
        public int SS2;
        public int SS3;
        public string SS4;
    }
    public class OnSkillDon22222{
        public SkillData SkillDa333ta2;
    }
}