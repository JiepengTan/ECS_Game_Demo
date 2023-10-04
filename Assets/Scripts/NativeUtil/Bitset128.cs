using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lockstep.NativeUtil
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe partial struct BitSet32
    {
        public const int BitsSize = 32;
        public UInt32 bits;
        public void Set( Int32 bit) {
            bits |= (1u<<(bit));
        }
        public void Clear(Int32 bit) {
            bits &= ~(1u<<(bit));
        }
        public void Set( Int32 bit,bool isTrue) {
            if (isTrue)
            {
                bits |= (1u<<(bit));
            }
            else
            {
                bits &= ~(1u<<(bit));
            }
        }
        public Boolean Is(Int32 bit) {
            return (bits&(1u<<(bit))) != 0u;
        }
        public void ClearAll()
        {
            bits = 0;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < BitsSize; i++)
            {
                sb.Append(Is(i) + ",");
            }
            sb.Append("]");
            return base.ToString();
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe partial struct BitSet64 {
        public const int BitsSize = 64;
        public UInt64 bits;
        public void Set( Int32 bit) {
            bits |= (1ul<<(bit));
        }
        public void Clear(Int32 bit) {
            bits &= ~(1ul<<(bit));
        }
        public void Set( Int32 bit,bool isTrue) {
            if (isTrue)
            {
                bits |= (1ul<<(bit));
            }
            else
            {
                bits &= ~(1ul<<(bit));
            }
        }
        public Boolean Is(Int32 bit) {
            return (bits&(1ul<<(bit))) != 0ul;
        }
        public void ClearAll()
        {
            bits = 0;
        }        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < BitsSize; i++)
            {
                sb.Append(Is(i) + ",");
            }
            sb.Append("]");
            return base.ToString();
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe partial struct BitSet128 {
        public const Int32 BitsSize = 128;
        private const int Int32ArraySize = BitsSize / 32;
        public fixed uint bits[Int32ArraySize];
        public void Set( Int32 bit,bool isTrue)
        {
            if (isTrue)
                Set(bit);
            else
            {
                Clear(bit);
            }
        }
        public void Set(Int32 bit) {
            fixed (uint* ptr = bits) {
                ptr[bit/32] |= (1u<<(bit%32));
            }
        }
        public void Clear( Int32 bit) {
            fixed (uint* ptr = bits) {
                ptr[bit/32] &= ~(1u<<(bit%32));
            }
        }
        public Boolean Is(Int32 bit) {
            fixed (uint* ptr = bits) {
                return (ptr[bit/32]&(1u<<(bit%32))) != 0u;
            }
        }
        public void ClearAll()
        {
            for (int i = 0; i < Int32ArraySize; i++)
            {
                bits[i] =0;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < BitsSize; i++)
            {
                sb.Append(Is(i) + ",");
            }
            sb.Append("]");
            return base.ToString();
        }
    }
}

