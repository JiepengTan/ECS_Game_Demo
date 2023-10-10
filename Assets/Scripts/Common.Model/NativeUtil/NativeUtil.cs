// Copyright 2019 谭杰鹏. //https://github.com/JiepengTan 
#define ENABLE_NATIVE_MEM_LOG
#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

namespace Lockstep.UnsafeECS {
    public class NativeUtil
    {
        [Conditional("ENABLE_NATIVE_MEM_LOG")]
        static void Log(string msg) {
            Debug.Log(msg);
        }
        [Conditional("ENABLE_NATIVE_MEM_LOG")]
        static void LogError(string msg) {
            Debug.LogError(msg);
        }

        public static void FreeAll() {
            var remainSum = s_ptr2Size.Values.Sum();
            if (remainSum != 0) {
                Debug.LogError($"Some Memory leak ! LeakCount= {remainSum/1024.0f /1024}MB   ");
            }
            foreach (var pair in s_ptr2Size) {
                Debug.LogError($"Memory leak ptr = {pair.Key} size = { pair.Value}" );
                Marshal.FreeHGlobal((IntPtr)pair.Key);
            }
            s_ptr2Size.Clear();
        }

        private static Dictionary<long, int> s_ptr2Size = new Dictionary<long, int>();
        private static HashSet<IntPtr> _ptrs = new HashSet<IntPtr>();
        public static void Free(IntPtr ptr) {
            _ptrs.Remove(ptr);
#if DEBUG
            int size = 0;
            s_ptr2Size.TryGetValue((long)ptr,out size);
            s_ptr2Size.Remove((long)ptr);
            Log($"Free ptr={(long)ptr} size={size/1024}KB  TotalCount { s_ptr2Size.Values.Sum() / 1024 /1024} MB");
#endif
            Marshal.FreeHGlobal(ptr);
        }

        public static IntPtr Alloc(int size){
            var ptr= Marshal.AllocHGlobal(size);
            _ptrs.Add(ptr);
#if DEBUG
            s_ptr2Size[(long)ptr] = size;
            Log($"Alloc ptr={(long)ptr} size={size/1024}KB TotalCount { s_ptr2Size.Values.Sum() / 1024 /1024} MB ");
#endif
            return ptr;
        }

        public static unsafe void Zero(byte* ptr, int size){
            for (; size >= 4; size -= 4) {
                *(int*) ptr = 0;
                ptr += 4;
            }
            for (; size > 0; --size) {
                *ptr = 0;
            }
        }

        public static unsafe void Copy(void* dest, void* src, int size){
            Copy((byte*) dest, (byte*) src, size);
        }

        public static unsafe void Copy(byte* dest, byte* src, int size)
        {
            for (; size >= 4; size -= 4)
            {
                *(int*) dest = *(int*) src;
                dest += 4;
                src += 4;
            }
            for (; size > 0; --size)
            {
                *dest = *src;
                ++dest;
                ++src;
            }
        }
        public static unsafe bool Compare(byte* dest, byte* src, int size)
        {
            for (; size >= 4; size -= 4)
            {
                if (*(int*) dest != *(int*) src) {
                    return false;
                }
                dest += 4;
                src += 4;
            }
            for (; size > 0; --size)
            {
                if(*dest != *src) return false;
                ++dest;
                ++src;
            }
            return true;
        }
        public static void NullPointer()
        {
            throw new NullReferenceException("Method invoked on null pointer.");
        }
        public static void ArrayOutOfRange(string msg ="")
        {
            throw new ArgumentOutOfRangeException("Array index out of range " + msg);
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct Tuple
    {
        public static Tuple<T0, T1> Create<T0, T1>(T0 item0, T1 item1)
        {
            return new Tuple<T0, T1>(item0, item1);
        }

        public static Tuple<T0, T1, T2> Create<T0, T1, T2>(T0 item0, T1 item1, T2 item2)
        {
            return new Tuple<T0, T1, T2>(item0, item1, item2);
        }

        public static Tuple<T0, T1, T2, T3> Create<T0, T1, T2, T3>(
            T0 item0,
            T1 item1,
            T2 item2,
            T3 item3)
        {
            return new Tuple<T0, T1, T2, T3>(item0, item1, item2, item3);
        }
    }
}