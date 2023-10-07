using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gamestan.Spatial {
    public unsafe class Region {
        public const int GridMemSize = 4*8;
        public const int GridSize = 2;
        public const int GridCountPerChunkEdge  = 8 ;// sqrt( 2KB/GridMemSize)  = sqrt(64) = 8
        public const int ChunkSize = GridCountPerChunkEdge  * GridSize ; // 16
        public const int ChunkMemSize = 2048;//GridCountPerChunkEdge*GridCountPerChunkEdge*GridMemSize;

        [System.Serializable]
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct Grid {
            [FieldOffset(0)] 
            public UInt32 CountPtr;              
            [FieldOffset(4)] 
            public fixed ushort EntityIds[14];  
            
            public int Count => (int)(CountPtr & 0xFF);
            // 同一个格子内部太多物体的时候，使用外链进行存储
            public UInt32 LinkPtr => CountPtr >> 8; 
        }
        
        [System.Serializable]
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct Chunk {
            //2K size
            public const int SizeX = GridCountPerChunkEdge;
            public const int SizeY = GridCountPerChunkEdge;
            [FieldOffset(0)] 
            public fixed Int32 Data[ChunkMemSize/sizeof(Int32)];
            
            public Grid* GetGrid(int2 localCoord) {
                var offset = localCoord.x * SizeX + localCoord.y;
                Debug.Assert(offset>0 && offset < SizeX*SizeY,"coord out of range " + localCoord.ToString());
                fixed(void* ptr = &this.Data[0])
                    return (Grid*)ptr;
            }
        }
        public class ChunkInfo {
            public int2 WorldPos => Coord * ChunkSize;
            public int EntityCount;
            public int2 Coord;
            public Chunk* ChunkPtr;
            public override string ToString() {
                return $"WorldCoord:{WorldPos} EntityCount{EntityCount}";
            }
        }
        
        public int[] LayerSize;
        public int LayerCount => LayerSize.Length;
        public const int InitChunkSize = 32;
        private Dictionary<int2, ChunkInfo> _coord2Data = new Dictionary<int2, ChunkInfo>();
        private Stack<IntPtr> _freeList = new Stack<IntPtr>();
        public void DoAwake() {
            Debug.Assert(GridMemSize == sizeof(Grid),"Grid size is diff with GridMemSize , but some code is dependent on it");
            Debug.Assert(ChunkMemSize == sizeof(Chunk),"Grid size is diff with GridMemSize , but some code is dependent on it");
            int capacity = 1024*1024/ChunkMemSize;// 1MB 
            int totalSize = UnsafeUtility.SizeOf<Chunk>() * capacity;
            var chunks = (Chunk*)UnsafeUtility.Malloc(totalSize);
            UnsafeUtility.MemClear(chunks,totalSize);
            for (int i = 0; i < capacity; i++) {
                var chunkPtr = &chunks[i];
                _freeList.Push((IntPtr)(chunkPtr));
            }
        }

        public void Destroy() {
            foreach (var ptr in _freeList) {
                UnsafeUtility.Free((void*)ptr);
            }
            _freeList.Clear();
            foreach (var pair in _coord2Data) {
                var ptr = pair.Value.ChunkPtr;
                if (ptr != null) {
                    UnsafeUtility.Free(ptr);
                    pair.Value.ChunkPtr = null;
                }
            }
            _coord2Data.Clear();
        }
        
        IntPtr AllocChunk() {
            if (_freeList.Count > 0) {
                return _freeList.Pop();
            }
            var ptr= UnsafeUtility.Malloc(ChunkMemSize);
            UnsafeUtility.MemClear(ptr,ChunkMemSize);
            return (IntPtr)ptr;
        }

        void FreeChunk(IntPtr ptr) {
            _freeList.Push(ptr);
        }

    }
}