using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GamesTan.ECS {
    public unsafe struct EntityRef {
        public static EntityRef DefaultObject = new EntityRef();

        // 无效数据格式
        public static UInt64 DefaultObjectIntData = 0;

        public const int SlotBitCount = 20;
        public const int TypeIdBitCount = 32 - SlotBitCount;
        public const int MaxSlotId = 1 << SlotBitCount;

        /// <summary> TypeId   /// </summary>
        public uint TypeId => _InternalData >> SlotBitCount;

        /// <summary> SlotId   /// </summary>
        public uint SlotId => _InternalData & 0x0FFFFF;

        /// <summary> Version   /// </summary>
        public int Version {
            get => (int)_InternalData2;
            set => _InternalData2 = (UInt32)value;
        }

        /// <summary> internalInfo   /// </summary>
        public UInt32 _InternalData;

        public UInt32 _InternalData2;


        public EntityRef(int typeId, int slotId, int version) {
            DebugUtil.Assert(typeId < 1 << TypeIdBitCount, "TypeId out of range " + typeId);
            DebugUtil.Assert(slotId < 1 << SlotBitCount, "EntityId out of range " + slotId);
            _InternalData = (uint)typeId << SlotBitCount | (uint)slotId;
            _InternalData2 = (UInt32)version;
        }

        private EntityRef(UInt32 data1, UInt32 data2) {
            _InternalData = data1;
            _InternalData2 = data2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator EntityRef (UInt64 data) {
            return new EntityRef((UInt32)(data>>32),(UInt32)(data &0xFFFFFFFF));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator UInt64 (EntityRef data) {
            return (UInt64)data._InternalData << 32 | data._InternalData2;
        }

        public override string ToString() {
            return $" typeId{TypeId} SlotId:{SlotId} Version:{Version} _data:{_InternalData}";
        }

        public static bool operator ==(EntityRef lhs, EntityRef rhs) {
            return lhs._InternalData == rhs._InternalData;
        }
        public static bool operator != (EntityRef lhs, EntityRef rhs) {
            return !(lhs == rhs);
        }

        public override int GetHashCode() {
            return (int)_InternalData;
        }
    }
}