#_ME_FOR #BUILDIN_COMPS; rename(#BUILIN_NAME = #CLS_NAME)
        public unsafe Buffer<#BUILIN_NAMEFilter> GetAll#BUILIN_NAME()
        {
            Buffer<#BUILIN_NAMEFilter> buffer = Buffer<#BUILIN_NAMEFilter>.Alloc(#ALL_SIZE);
#_ME_FOR #ENTITY; if(HasField(#CLS_NAME,#BUILIN_NAME))
            #CLS_NAME* #CLS_NAMEPtr = this._entities->Get#CLS_NAME(0);
            var idx#CLS_NAME = #SIZE;
            while (idx#CLS_NAME >= 0)
            {
                if (#CLS_NAMEPtr->_entity._active)
                {
                  buffer.Items[buffer.Count].Entity = &#CLS_NAMEPtr->_entity;
                  buffer.Items[buffer.Count].#BUILIN_NAME = &#CLS_NAMEPtr->#BUILIN_NAME;
                  ++buffer.Count;
                }
                --idx#CLS_NAME;
                ++#CLS_NAMEPtr;
            }
#_ME_ENDFOR 
            return buffer;
        }
#_ME_ENDFOR 