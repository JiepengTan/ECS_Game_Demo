using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using GamesTan.ECS;

namespace Gamestan.Spatial {
    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Grid {
        public const int ArySize = 15;
        public const int WidthBit = 1;
        public const int Width = 1 << WidthBit;
        public const int MemSize = (ArySize + 1) * 8; // 64

        [FieldOffset(0)] public int Count;
        [FieldOffset(4)] public UInt32 NextGridPtr; // 同一个格子内部太多物体的时候，使用外链进行存储
        [FieldOffset(8)] public fixed UInt64 Entities[ArySize];
        public bool IsLocalFull => Count >= ArySize;
        public bool HasExtData => NextGridPtr == 0;


        static List<EntityData> _debugTempDatas = new List<EntityData>();

        public List<EntityData> ToDebugList() {
            _debugTempDatas.Clear();
            fixed (void* ptr = &this.Count) {
                Grid* curGrid = (Grid*)ptr;
                for (int i = 0; i < Count; i++) {
                    var offset = i % Grid.ArySize;
                    if (offset == 0 && i != 0) {
                        curGrid = Region.Instance.GetExtraGrid(curGrid->NextGridPtr);
                    }

                    if (curGrid == null) {
                        break;
                    }

                    _debugTempDatas.Add((EntityData)curGrid->Entities[offset]);
                }
            }

            return _debugTempDatas;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            DumpString(sb);
            return sb.ToString();
        }

        public void DumpString(StringBuilder sb, bool isOneLine = false) {
            var datas = ToDebugList();
            sb.Append("Count: " + Count + "  ");
            if (!isOneLine) {
                sb.AppendLine();
            }

            for (int i = 0; i < datas.Count; i++) {
                sb.Append(i + ": " + datas[i].ToString());
                if (!isOneLine) {
                    sb.AppendLine();
                }
            }
        }
    }
}