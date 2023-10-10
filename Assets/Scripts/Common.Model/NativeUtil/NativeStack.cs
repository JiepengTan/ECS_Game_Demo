// Copyright 2019 谭杰鹏. //https://github.com/JiepengTan 

using System;
using System.Collections.Generic;
using Lockstep.UnsafeECS;

namespace Lockstep.InternalUnsafeECS {
    public unsafe struct NativeStack {
        private NativeArray<int> _array;
        private int _arraySize;
        private int _count;
        public int Count => _count;

        public unsafe NativeStack(int length = 16, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) {
            if (length <= 0) {
                throw new Exception("NativeStack Length should > 0");
            }

            _array = new Lockstep.UnsafeECS.NativeArray<int>(length, allocator, NativeArrayOptions.ClearMemory);
            _arraySize = length;
            _count = 0;
        }

        public void InitIndexStack() {
            var ptr = _array.GetPointer(0);
            for (int i = _arraySize - 1; i >= 0; i++, ++ptr) {
                *ptr = i;
            }
        }

        public void Push(int idx) {
            if (_count >= _arraySize) {
                _arraySize = _arraySize * 2;
                _array.Realloc(_arraySize, NativeArrayOptions.UninitializedMemory);
            }

            _array[_count++] = idx;
        }

        public int Pop() {
            if (_count == 0) {
                throw new Exception("Pop failed! stack is Empty");
            }

            return _array[--_count];
        }

        public bool IsEmpty() {
            return _count == 0;
        }

        public unsafe void Dispose() {
            _arraySize = -1;
            _array.Dispose();
        }
    }
}