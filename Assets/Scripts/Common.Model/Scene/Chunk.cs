using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using GamesTan.ECS;
using Unity.Mathematics;

namespace Gamestan.Spatial {
    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Chunk {
        public const int GridScalerBit = 3;
        public const int GridScaler = 1 << GridScalerBit; // sqrt( 4KB/Grid.GridMemSize)  = sqrt(64) = 8

        public const int SizeX = GridScaler;
        public const int SizeY = GridScaler;

        public const int WidthBit = 3 + Grid.WidthBit;
        public const int Width = 1 << WidthBit; // 16
        public const int MemSize = GridScaler * GridScaler * Grid.MemSize;

        public const int RowMemSize = GridScaler * Grid.MemSize;

        [FieldOffset(0)] public fixed byte Data[MemSize];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Grid* GetGrid(int2 localCoord) {
            DebugUtil.Assert(localCoord.x >= 0 && localCoord.y >= 0 && localCoord.x < SizeX && localCoord.y < SizeY,
                " coord out of range " + localCoord.ToString());
            var offset = (localCoord.y * SizeX + localCoord.x) * Grid.MemSize;
            fixed (void* ptr = &this.Data[offset])
                return (Grid*)ptr;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < SizeY; y++) {
                for (int x = 0; x < SizeX; x++) {
                    var coord = new int2(x, y);
                    var grid = GetGrid(coord);
                    sb.Append(coord + ": ");
                    grid->DumpString(sb, true);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}