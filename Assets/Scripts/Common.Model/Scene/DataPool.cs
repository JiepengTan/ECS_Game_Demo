using System;
using System.Collections.Generic;
using System.Text;
using GamesTan.ECS;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
using UnityEngine;
using TItem = Gamestan.Spatial.GridExtLink;

namespace Gamestan.Spatial {
    public struct DataPtr {
        public static DataPtr DefaultObject = new DataPtr();
        public const int SlotBitCount = 20;
        public const uint SlotMask = 0x0FFFFF;
        public const int MaxVersion = 0xFFF;
        public const int MaxSlotId = 1 << SlotBitCount;

        public const int DataBitCount = 20;
        public const uint DataMask = 0x0FFFFF;

        public int Version {
            get => (int) (_InternalData >> DataBitCount) ;
            set => _InternalData = (uint)value << SlotBitCount | ((uint)SlotId & SlotMask);
        }

        public uint SlotId {
            get => _InternalData & SlotMask;
            set => _InternalData = (uint)Version << SlotBitCount | ((uint)value & SlotMask);
        }

        public uint DataHead {
            get => _InternalData2 >> DataBitCount;
            set => _InternalData2 = (uint)value << DataBitCount | ((uint)DataBody & DataMask);
        }

        public uint DataBody {
            get => (uint)(_InternalData2 & DataMask);
            set => _InternalData2 = (uint)(byte)DataHead << DataBitCount | ((uint)value & DataMask);
        }

        internal UInt32 _InternalData;
        internal UInt32 _InternalData2;

        public DataPtr(int slotId,int version) {
            DebugUtil.Assert(slotId < 1 << SlotBitCount, "EntityId out of range " + slotId);
            _InternalData = (uint)version << SlotBitCount | ((uint)slotId & SlotMask);
            _InternalData2 = 0;
        }

        public override string ToString() {
            return $" typeId{DataHead} SlotId:{SlotId} Version:{Version} Count:{DataBody}";
        }

        public override int GetHashCode() {
            return (int)_InternalData;
        }
    }

    // TODO thread safe
    public unsafe class DataPool {
        private int _typeId;
        private TItem* _ary = null;
        private DataPtr* _freeList = null;
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
            DebugUtil.Assert(_capacity < DataPtr.MaxSlotId, "Pool size too big !" + _capacity);
            _ary = (TItem*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TItem>() * capacity);
            _freeList = (DataPtr*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<DataPtr>() * capacity);

            for (int i = 0; i < capacity; i++) {
                // Don't initialize chunk.
                _freeList[i] = new DataPtr( i, -1);
                _ary[i] = new TItem();
                _ary[i].__Data = _freeList[i];
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

        public DataPtr Alloc() {
            if (_capacity == 0) {
                Init(_typeId);
            }

            if (_length == _capacity) {
                var oldCap = _capacity;
                _capacity = (int)(_capacity * 1.4f);
                if (_capacity > 10000) Debug.LogWarning($"{GetType().Name} Realloc {_capacity}");
                DebugUtil.Assert(_capacity < DataPtr.MaxSlotId, "Pool size too big !" + _capacity);
                _ary = (TItem*)UnsafeUtility.Realloc(_ary, UnsafeUtility.SizeOf<TItem>() * oldCap,
                    UnsafeUtility.SizeOf<TItem>() * _capacity);
                _freeList = (DataPtr*)UnsafeUtility.Realloc(_freeList, UnsafeUtility.SizeOf<DataPtr>() * oldCap,
                    UnsafeUtility.SizeOf<DataPtr>() * _capacity);
                for (int i = oldCap; i < _capacity; i++) {
                    // Don't initialize chunk.
                    _freeList[i] = new DataPtr( i, -1);
                    _ary[i] = new TItem();
                    _ary[i].__Data = _freeList[i];
                }
            }

            DebugUtil.Assert(_freeList[_length].Version < 0, "freeList version should <0");
            var ret = _freeList[_length];
            ret.Version = DataPtr.MaxSlotId - (ret.Version+1);
            _freeList[_length] = ret;
            _ary[ret.SlotId] = new TItem(); // reset the data
            _ary[ret.SlotId].__Data = ret;
            _length++;
            _maxUsedSlot = Mathf.Max(_length, _maxUsedSlot);
            return ret;
        }

        public void QueueFree(DataPtr item) {
            if (_ary == null) {
                Debug.LogError(GetType().Name + " Not init or has destroyed");
                return;
            }

            TItem* ptr = GetData(item);
            DebugUtil.Assert(ptr != null, $"{GetType().Name}Try to free a destroied entity {item}");
            if (ptr == null) {
                return;
            }

            _length--;
            ptr->__Data.Version = -ptr->__Data.Version;
            _freeList[_length] = ptr->__Data;
        }

        public TItem* GetData(DataPtr entity) {
            if (_ary == null) {
                Debug.LogError(GetType().Name + " Not init or has destroyed");
                return null;
            }

            if (entity.SlotId < 0 || entity.SlotId > _capacity) {
                throw new Exception($"{GetType().Name} Index Out of range ");
            }

            var ptr = &_ary[entity.SlotId];
            if (ptr->__Data.Version <= 0) {
                //return null; // small version can =0
            }

            return ptr;
        }

        public override string ToString() {
            if (_ary == null) return "null";
            return DebugGetFreeListString(8) + "\n=======\n" + DebugGetItemListString(8);
        }

        string DebugGetFreeListString(int count) {
            StringBuilder sb = new StringBuilder();
            var info = DebugGetFreeListData(0, count);
            foreach (var item in info) {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        string DebugGetItemListString(int count) {
            StringBuilder sb = new StringBuilder();
            var info = DebugGetItemListData(0, count);
            foreach (var item in info) {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        TItem[] DebugGetItemListData(int startIdx, int count) {
            var ret = new TItem[count];
            fixed (TItem* ptr = ret) {
                NativeUtil.Copy(ptr, _ary, sizeof(TItem) * count);
            }

            return ret;
        }

        DataPtr[] DebugGetFreeListData(int startIdx, int count) {
            var ret = new DataPtr[count];
            fixed (DataPtr* ptr = ret) {
                NativeUtil.Copy(ptr, _freeList, sizeof(DataPtr) * count);
            }

            return ret;
        }
    }
}