namespace Lockstep.UnsafeECS.Game { 

    public enum EEntityType : int {
        None,
#_ME_FOR #ENTITY
        #CLS_NAME,
#_ME_ENDFOR 
        EnumCount,
    }

    public static class EntityIds {
#_ME_FOR #ENTITY
      	public const int #CLS_NAME = (int)(EEntityType.#CLS_NAME);
#_ME_ENDFOR 
      	public const int TotalEntityTypeCount = ((int)(EEntityType.EnumCount) -1);
    }

    public enum EComponentType : int {
        None,
#_ME_FOR #COMPONENT
        #CLS_NAME,
#_ME_ENDFOR 
        EnumCount
    }

    public class CompoenntIds {
#_ME_FOR #COMPONENT
      public const int #CLS_NAME = (int)(EComponentType.#CLS_NAME);
#_ME_ENDFOR 
      public const int TotalComponentTypeCount = ((int)(EComponentType.EnumCount) -1);
    }



#_ME_FOR #ENUM 
    public enum #CLS_NAME : int {
#_ME_FOR #GET_FIELDS
        #FIELD_NAME = #FIELD_VALUE,
#_ME_ENDFOR 
    }
#_ME_ENDFOR 


#_ME_FOR #COMPONENT ;rename(#COMP_NAME = #CLS_NAME)
    public enum E_EntityOf#COMP_NAME{
    #_ME_FOR #ENTITY;filter(HasTypeField(#CLS_NAME,#COMP_NAME));rename(#ENTITY_NAME = #CLS_NAME)
        #ENTITY_NAME,
    #_ME_ENDFOR
    }
#_ME_ENDFOR
#_ME_FOR #COMPONENT ;rename(#COMP_NAME = #CLS_NAME)
    public enum E_FieldOf#COMP_NAME{
    #_ME_FOR #GET_FIELDS
        #FIELD_NAME,
    #_ME_ENDFOR
    }
#_ME_ENDFOR
}