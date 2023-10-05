using System;
using UnityEngine;

namespace GamesTan.ECS {
    public struct EntityData {

        public const int SlotBitCount = 20;
        public const int TypeIdBitCount = 32-SlotBitCount;
        public const int MaxSlotId = 1<<SlotBitCount;
        /// <summary> TypeId   /// </summary>
        public uint TypeId=> _InternalData >> SlotBitCount ;

        /// <summary> SlotId   /// </summary>
        public uint SlotId=> _InternalData &0x0FFFFF;

        
        /// <summary> internalInfo   /// </summary>
        public UInt32 _InternalData;
        /// <summary> Version   /// </summary>
        public int Version;

        public bool IsAlive => Version < 0;
        public EntityData(int typeId, int slotId, int version) {
            Debug.Assert(typeId <1<<TypeIdBitCount,"TypeId out of range " +typeId );
            Debug.Assert(slotId <1<<SlotBitCount,"EntityId out of range "+ slotId );
            _InternalData = (uint)typeId <<SlotBitCount | (uint)slotId;
            Version = version;
        }

        public override string ToString() {
            return $" typeId{TypeId} SlotId:{SlotId} Version:{Version}";
        }

        public override int GetHashCode() {
            return (int)_InternalData;
        }
        
    }
}