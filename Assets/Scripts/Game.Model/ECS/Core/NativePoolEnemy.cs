using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
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
                _ary[i] = new TItem();
                _ary[i].__EntityData = _freeList[i];
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
            _freeList[_length] = ret;
            _ary[ret.SlotId].__EntityData =ret;
            _length++;
            return ret;
        }

        public void QueueFree(EntityData item) {
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