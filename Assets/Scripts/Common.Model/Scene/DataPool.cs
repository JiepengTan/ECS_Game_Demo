using System;
using System.Collections.Generic;
using System.Text;
using GamesTan.ECS;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
using UnityEngine;

namespace GamesTan.Spatial {
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

}