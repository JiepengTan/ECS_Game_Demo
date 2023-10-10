#_ME_FOR #GAME_COMPONENT
    [StructLayoutAttribute(LayoutKind.Sequential)]
    [System.Serializable]
    public unsafe partial struct #CLS_NAME {
#_ME_FOR #GET_FIELDS
        public #FIELD_TYPE #FIELD_NAME;
#_ME_ENDFOR 
        public override Int32 GetHashCode() {
            unchecked {
                var hash = 7;
#_ME_FOR #GET_FIELDS
                hash = hash * 37 +#FIELD_NAME.GetHashCode();
#_ME_ENDFOR  
                return hash;
            }
        }
    }
#_ME_ENDFOR  