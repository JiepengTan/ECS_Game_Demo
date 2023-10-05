using System;
using System.Collections;
using System.Collections.Generic;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using TItem = GamesTan.ECS.Game.Enemy;


namespace GamesTan.ECS.Game {
    // TODO thread safe
    public unsafe class NativePoolEnemy {
        private int _typeId;
        private TItem* _ary = null;
        private EntityData* _freeList = null;
        private int _capacity;
        private int _length;

        public int Capacity => _capacity;
        public int Count => _length;
        public int TypeId => _typeId;

        public bool IsEmpty => _length == 0;
        public bool IsFull => _length == _capacity - 1;
        public TItem* GetData() {
            return _ary;
        }

        public void Init(int typeId, int capacity) {
            _typeId = typeId;
            _ary = (TItem*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TItem>() * capacity);
            _freeList = (EntityData*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<EntityData>() * capacity);
            _capacity = capacity;
            _length = 0;

            for (int i = 0; i < capacity; i++) {
                // Don't initialize chunk.
                _freeList[i] = new EntityData(_typeId, i, -1);
            }
        }

        public EntityData Alloc() {
            if (_length == _capacity) {
                throw new Exception(" Memory out of range " + GetType().Name);
            }

            Debug.Assert(_freeList[_length].Version < 0, "freeList version should <0");
            _freeList[_length].Version--; // neg-- == pos++
            var ret = _freeList[_length];
            ret.Version = -ret.Version;
            _ary[_length].__EntityData = ret;
            _length++;
            return ret;
        }

        public void QueueFree(EntityData item) {
            TItem* ptr = GetData(item);
            Debug.Assert(ptr != null, "Try to free a destroied entity" + GetType().Name);
            if (ptr == null) {
                return;
            }

            ptr->__EntityData.Version = -ptr->__EntityData.Version;
            _length--;
            _freeList[_length] = ptr->__EntityData;
        }

        public TItem* GetData(EntityData entity) {
            if (entity.SlotId < 0 || entity.SlotId > _capacity) {
                throw new Exception(" Index Out of range " + GetType().Name);
            }

            var ptr = &_ary[entity.SlotId];
            if (ptr->__EntityData.Version <= 0) {
                return null;
            }

            return ptr;
        }
    }
}