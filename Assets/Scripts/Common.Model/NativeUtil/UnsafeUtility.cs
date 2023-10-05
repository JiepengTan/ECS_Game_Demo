// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

#define USING_UNITY_MEM_FUNC
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lockstep.UnsafeECS;
using Lockstep.Util;

namespace Lockstep.InternalUnsafeECS {
    public static class UnsafeUtility
    {
        public static long TotalAllocSize;
        public static unsafe void* Malloc(long size, int alignment =1, Allocator allocator = Allocator.Persistent){
            var remain = size % alignment;
            if (remain != 0) {
                size = size + (alignment - remain);
            }

            TotalAllocSize += size;
            return UnsafeECS.NativeUtil.Alloc((int) size).ToPointer();
        }
        public static unsafe void* Realloc(void* rawPtr,int rawSize, long size, int alignment =1, Allocator allocator = Allocator.Persistent){
            if (size <= rawSize) {
                throw new Exception("ReAlloc size invalid !");
            }
            var newPtr = Malloc((int) size,alignment,allocator);
            TotalAllocSize -= rawSize;
            MemCpy(newPtr,rawPtr,rawSize);
            Free(rawPtr,allocator);
            return newPtr;
        }
        public static unsafe void Free(void* memory, Allocator allocator = Allocator.Persistent){
            if (memory == null) {
                throw new Exception("Try to free a null pointer");
            }

            UnsafeECS.NativeUtil.Free(new IntPtr(memory));
        }

        public static unsafe void MemCpy<T>(T[] dst,T[] src)where T : unmanaged
        {
            if (dst.Length != src.Length) 
                throw new Exception($"MemCpy Array Length is NOT equal {src.Length} != {dst.Length}");
            var totalSize = sizeof(T) * dst.Length;
            fixed (T* ptrSrc = &src[0])
            fixed (T* ptrDst = &dst[0])
                UnsafeUtility.MemCpy(ptrDst,  ptrSrc, totalSize);
              
        }
        public static unsafe void MemCpy(void* destination, void* source, long size){
            Profiler.BeginSample($"MemCpy  kb"  );//{(size/1000.0f)}
#if USING_UNITY_MEM_FUNC
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(destination, source, size);
#else
            UnsafeECS.NativeUtil.Copy(destination, source, (int) size);
#endif
            Profiler.EndSample();
        }


        public static unsafe void MemClear(void* destination, long size){
            Profiler.BeginSample($"MemClear {(size/1000.0f)} kb"  );
#if USING_UNITY_MEM_FUNC
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(destination, size);
#else
            UnsafeECS.NativeUtil.Zero((byte*) destination, (int) size);
#endif
            Profiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe bool MemCmp(void* ptr1, void* ptr2, long size){
            if (ptr1 == ptr2) return true;
#if USING_UNITY_MEM_FUNC
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCmp(ptr1, ptr2, size) == 0;
#else
            return UnsafeECS.NativeUtil.Compare((byte*) ptr1, (byte*) ptr2, (int) size);
#endif
        }


        public static unsafe void CopyPtrToStructure<T>(void* ptr, out T output) where T : unmanaged{
            output = *(T*) ptr;
        }

        public static unsafe void CopyStructureToPtr<T>(ref T input, void* ptr) where T : unmanaged{
            *(T*) ptr = input;
        }

        public static unsafe T ReadArrayElement<T>(void* source, int index) where T : unmanaged{
            return ((T*) source)[index];
        }
        public static unsafe ref T ReadArrayElementRef<T>(void* source, int index) where T : unmanaged{
            return ref ((T*) source)[index];
        }
        public static unsafe T ReadArrayElementWithStride<T>(void* source, int index, int stride) where T : unmanaged{
            return *(T*) ((IntPtr) source + index * stride);
        }

        public static unsafe void WriteArrayElement<T>(void* destination, int index, T value) where T : unmanaged{
            ((T*) destination)[index] = value;
        }

        public static unsafe void WriteArrayElementWithStride<T>(
            void* destination,
            int index,
            int stride,
            T value) where T : unmanaged{
            *(T*) ((IntPtr) destination + index * stride) = value;
        }

        public static unsafe void* AddressOf<T>(ref T output) where T : unmanaged{
            throw new Exception("TODO AddressOf check the code is right!!");
            var ss = __makeref(output);
            TypedReference tr = __makeref(output);
            return (void*) (**(IntPtr**) (&tr));
            //fixed (byte** ptr = &output) {
            //    return (void*) *ptr;
            //}
        }

        public static unsafe int SizeOf<T>() where T : unmanaged{
            return sizeof(T);
        }

        public static int AlignOf<T>() where T : struct{
            return 4;
        }

        public static bool IsValidAllocator(Allocator allocator){
            return allocator > Allocator.None;
        }


#if false
#endif
    }
}