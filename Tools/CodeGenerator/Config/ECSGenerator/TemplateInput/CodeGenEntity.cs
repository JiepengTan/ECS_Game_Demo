
#_ME_FOR #ENTITY 
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = Define.PackSize)]
    public unsafe partial struct #CLS_NAME :IEntity {
        public const Int32 MAX_COUNT = 4;
        internal Entity _entity;
        //#CALL(GetFileds(#CLS_NAME))
        //#GET_FIELDS(#CLS_NAME)
#_ME_FOR #GET_FIELDS
        public #FIELD_TYPE #FIELD_TYPE;
#_ME_ENDFOR 
        public EntityRef EntityRef =>this._entity._ref;
        public int EntityIndex =>this._entity._ref._index;
        public bool IsActive =>this._entity._active;

        #region Fields Getter Setter

#_ME_FOR #GET_FIELDS;use(#FIELD_NAME) 
    #_ME_FOR  #GET_FIELDS_OF(#FIELD_NAME)
        public #FIELD_TYPE #FIELD_NAME {
            get => Fields.#FIELD_NAME;
            set => Fields.#FIELD_NAME = value;
        }
    #_ME_ENDFOR 
#_ME_ENDFOR 
        #endregion
    }
#_ME_ENDFOR 


