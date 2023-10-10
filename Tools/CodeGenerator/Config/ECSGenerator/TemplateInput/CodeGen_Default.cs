    public unsafe partial struct __default {
        //enum 
#_ME_FOR #ENUM
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR
        //IRef 
#_ME_FOR #REF
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR  
        //IEntity 
#_ME_FOR #ENTITY
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR  
        //IAsset 
#_ME_FOR typeof IAsset
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR 
        //IComponent  
#_ME_FOR #COMPONENT
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR 
        //Bitset  
#_ME_FOR typeof(IBitset) 
        public static #CLS_NAME #CLS_NAME;
#_ME_ENDFOR 
    }