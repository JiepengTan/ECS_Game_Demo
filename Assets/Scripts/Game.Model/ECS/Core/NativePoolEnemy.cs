using System;
using System.Collections;
using System.Text;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
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
        private int _maxUsedSlot;

        public int MaxUsedSlot => _maxUsedSlot;
        public int Capacity => _capacity;
        public int Count => _length;
        public int TypeId => _typeId;

        public bool IsEmpty => _length == 0;
        public bool IsFull => _length == _capacity - 1;
        public TItem* GetData() {
            return _ary;
        }
        public ref TItem GetData(int idx) {
            return ref _ary[idx];
        }
        public void Init(int typeId, int capacity = 128) {
            _typeId = typeId;
            _capacity = capacity;
            _length = 0;
            Debug.Assert(_capacity < EntityData.MaxSlotId,"Pool size too big !" + _capacity);
            _ary = (TItem*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TItem>() * capacity);
            _freeList = (EntityData*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<EntityData>() * capacity);

            for (int i = 0; i < capacity; i++) {
                // Don't initialize chunk.
                _freeList[i] = new EntityData(_typeId, i, -1);
                _ary[i] = new TItem();
                _ary[i].__EntityData = _freeList[i];
            }
        }

        public void Destroy() {
            if (_ary != null) {
                UnsafeUtility.Free(_ary);
                _ary = null;
            }
            if (_freeList != null) {
                UnsafeUtility.Free(_freeList);
                _freeList = null;
            }
            _length = 0;
            _capacity = 0;
        }

        public EntityData Alloc() {
            if (_capacity == 0) {
                Init(_typeId);
            }
            if (_length == _capacity) {
                var oldCap = _capacity;
                _capacity = (int)(_capacity * 1.4f);
                Debug.LogWarning($"{GetType().Name} Realloc {_capacity}" );
                Debug.Assert(_capacity < EntityData.MaxSlotId,"Pool size too big !" + _capacity);
                _ary = (TItem*)UnsafeUtility.Realloc(_ary, UnsafeUtility.SizeOf<TItem>() * oldCap,UnsafeUtility.SizeOf<TItem>() * _capacity);
                _freeList = (EntityData*)UnsafeUtility.Realloc(_freeList,UnsafeUtility.SizeOf<EntityData>() * oldCap,UnsafeUtility.SizeOf<EntityData>() * _capacity);
                for (int i = oldCap; i < _capacity; i++) {
                    // Don't initialize chunk.
                    _freeList[i] = new EntityData(_typeId, i, -1);
                    _ary[i] = new TItem();
                    _ary[i].__EntityData = _freeList[i];
                }
            }

            Debug.Assert(_freeList[_length].Version < 0, "freeList version should <0");
            _freeList[_length].Version--; // neg-- == pos++
            var ret = _freeList[_length];
            ret.Version = -ret.Version;
            _freeList[_length] = ret;
            _ary[ret.SlotId] = new TItem();// reset the data
            _ary[ret.SlotId].__EntityData =ret;
            _length++;
            _maxUsedSlot = Mathf.Max(_length,_maxUsedSlot);
            return ret;
        }

        public void QueueFree(EntityData item) {
            if (_ary == null) {
                Debug.LogError(  GetType().Name+" Not init or has destroyed");
                return;
            }

            TItem* ptr = GetData(item);
            Debug.Assert(ptr != null, $"{GetType().Name}Try to free a destroied entity {item}" );
            if (ptr == null) {
                return;
            }
            _length--;
            ptr->__EntityData.Version = -ptr->__EntityData.Version;
            _freeList[_length] = ptr->__EntityData;
        }

        public TItem* GetData(EntityData entity) {
            if (_ary == null) {
                Debug.LogError(  GetType().Name+" Not init or has destroyed");
                return null;
            }
            if (entity.SlotId < 0 || entity.SlotId > _capacity) {
                throw new Exception($"{GetType().Name} Index Out of range " );
            }

            var ptr = &_ary[entity.SlotId];
            if (ptr->__EntityData.Version <= 0) {
                return null;
            }

            return ptr;
        }

        public override string ToString() {
            if (_ary == null) return "null";
            return DebugGetFreeListString(8) +"\n=======\n"+ DebugGetItemListString(8);
        }

        string DebugGetFreeListString(int count) {
            StringBuilder sb = new StringBuilder();
            var info = DebugGetFreeListData(0,count);
            foreach (var item in info) {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }
        string DebugGetItemListString(int count) {
            StringBuilder sb = new StringBuilder();
            var info = DebugGetItemListData(0,count);
            foreach (var item in info) {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }
        TItem[] DebugGetItemListData(int startIdx,int count) {
            var ret = new TItem[count];
            fixed (TItem* ptr = ret) {
                NativeUtil.Copy(ptr,_ary,sizeof(TItem)*count);
            }
            return ret;
        }
        EntityData[] DebugGetFreeListData(int startIdx,int count) {
            var ret = new EntityData[count];
            fixed (EntityData* ptr = ret) {
                NativeUtil.Copy(ptr,_freeList,sizeof(EntityData)*count);
            }
            return ret;
        }
    }
}