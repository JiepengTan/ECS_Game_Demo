    public unsafe partial class Frame  {
        public void DestroyEntity(EntityRef entity_ref) {
            _destroy.Enqueue(entity_ref);
        }
        public unsafe void DestroyEntity(Entity* entity){
            if ((IntPtr) entity == IntPtr.Zero)
                Log.Error("can't destroy null pointer");
            else if (!entity->_active)
            Log.Error("entity already destroyed");
            else
            this._destroy.Enqueue(entity->EntityRef);
        }
        void EntityCreate(Entity* entity){
            Debug.Assert(entity->_active == false);
            entity->_ref._version += 1;
            entity->_active = true;
        }
        void EntityDestroy(Entity* entity){
            Debug.Assert(entity->_active);
            entity->_ref._version += 1;
            entity->_active = false;
        }
        public void AddEvent(EventBase baseEvent)
        {
            baseEvent._tail = _events;
            _events = baseEvent;
        }

        public unsafe Entity* GetEntity(EntityRef entity_ref)
        {   
            switch (entity_ref.EnumType) {
#_ME_FOR #ENTITY
                case EEntityTypes.#CLS_NAME:
                    return (Entity*) this.Get#CLS_NAME((EntityRef#CLS_NAME) entity_ref);
#_ME_ENDFOR    
                default:
                    return (Entity*) null;
            }
        }               
        public unsafe Buffer<EntityFilter> GetAllEntity()
        {
            Buffer<EntityFilter> buffer = Buffer<EntityFilter>.Alloc(#ALL_SIZE);
#_ME_FOR #ENTITY
            #CLS_NAME* ptr#CLS_NAME = this._entities->Get#CLS_NAME(0);
            var idx#CLS_NAME = 0;
            while (idx#CLS_NAME < #SIZE)
            {
                if (ptr#CLS_NAME->_entity._active)
                    buffer.Items[buffer.Count++].Entity = &ptr#CLS_NAME->_entity;
                ++idx#CLS_NAME;
                ++ptr#CLS_NAME;
            }
#_ME_ENDFOR
            return buffer;
        }   

        private unsafe void DestroyEntityInternal(Entity* entity)
        {
            if ((IntPtr) entity == IntPtr.Zero || !entity->_active)
                return;
            switch (entity->_ref.EnumType)
            {
#_ME_FOR #ENTITY
                case EEntityTypes.#CLS_NAME:
                    this.Destroy#CLS_NAMEInternal((#CLS_NAME*) entity);
                    break;
#_ME_ENDFOR
            }
        }
}