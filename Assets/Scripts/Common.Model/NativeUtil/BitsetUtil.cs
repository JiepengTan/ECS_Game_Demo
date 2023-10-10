// Copyright 2019 谭杰鹏. //https://github.com/JiepengTan 

using System;
using System.Runtime.CompilerServices;

namespace Lockstep.InternalUnsafeECS {
    public unsafe partial struct BitsetUtil {
        public static int FirstBit(UInt64 val){
            if (val == 0L) return -1;
            var half = (val & 0xffffffffL);
            if (half != 0L) return FirstBit((UInt32) half);
            return 32 + FirstBit((UInt32) (val >> 32));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FirstBit(UInt32 val){
            var half = (val & 0xffffu);
            if (half != 0u) return FirstBit((UInt16) half);
            return 16 + FirstBit((UInt16) (val >> 16));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FirstBit(UInt16 val){
            var half = (byte)(val & 0xffu);
            if (half != 0) return FirstBit((byte) half);
            return 8 + FirstBit((byte) (val >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FirstBit(byte val){
            int offset = 0;
            var half = (byte) (val & 0xf);
            if (half == 0) {
                half = (byte) (val >> 4);
                offset = 4;
            }
            for (byte i = 0; i < 4; i++) {
                if ((half & (1 >> (i))) != 0) {
                    return i + offset;
                }
            }
            return -1;
        }
    }
}