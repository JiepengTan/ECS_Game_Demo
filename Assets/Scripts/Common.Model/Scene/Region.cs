using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GamesTan.ECS;
using JetBrains.Annotations;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using EntityStorageData = System.UInt64;


namespace Gamestan.Spatial {
    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Grid {
        public const int ArySize = 7;
        public const int Width = 2;
        public const int MemSize = (ArySize + 1) * 8; // 64

        [FieldOffset(0)] public int Count;
        [FieldOffset(4)] public UInt32 ExtDataPtr; // 同一个格子内部太多物体的时候，使用外链进行存储
        [FieldOffset(8)] public fixed EntityStorageData Entities[ArySize];
        public bool IsLocalFull => Count == ArySize;

        // TODO  完成数据的存储
        public EntityData GetEntity(uint idx) {
            if (idx > Count) return EntityData.DefaultObject;
            if (IsLocalFull) {
                Debug.LogError("TODO implements ");
                return EntityData.DefaultObject;
            }

            return (EntityData)(Entities[idx]);
        }

        public void Add(EntityData entityData) {
            if (IsLocalFull) {
                Debug.LogError("TODO implements ");
                return;
            }

            Entities[Count] = (EntityStorageData)entityData;
            Count++;
        }

        public void Remove(EntityData entityData) {
            var data = (EntityStorageData)entityData;
            int count = Count <= ArySize ? Count : ArySize;
            for (int i = 0; i < Count; i++) {
                if (Entities[i] == data) {
                    Count--;
                    Entities[i] = Entities[Count];
                    Entities[Count] = EntityData.DefaultObjectIntData;
                    return;
                }
            }

            if (Count > ArySize) {
                // TODO
            }
        }

        public override string ToString() {
            return base.ToString();
        }
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GridExtLink {
        [FieldOffset(0)] public DataPtr __Data;

        [FieldOffset(8)] public fixed EntityStorageData Entities[15];
        // TODO implement
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Chunk {
        public const int GridScaler = 8; // sqrt( 4KB/Grid.GridMemSize)  = sqrt(64) = 8

        public const int SizeX = GridScaler;
        public const int SizeY = GridScaler;

        public const int Width = GridScaler * Grid.Width; // 16
        public const int MemSize = GridScaler * GridScaler * Grid.MemSize;

        public const int RowMemSize = GridScaler * Grid.MemSize;

        [FieldOffset(0)] public fixed byte Data[MemSize];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Grid* GetGrid(int2 localCoord) {
            Debug.Assert(localCoord.x>=0 && localCoord.y>=0 && localCoord.x < SizeX && localCoord.y < SizeY, " coord out of range " + localCoord.ToString());
            var offset = (localCoord.y * SizeX + localCoord.x) * Grid.MemSize;
            fixed (void* ptr = &this.Data[offset])
                return (Grid*)ptr;
        }
    }


    public unsafe class Region {
        public class ChunkInfo {
            public int EntityCount;

            public Chunk* Ptr;

            // left bottom corner
            public int2 WorldPos => Coord * Chunk.Width;
            public int2 Coord;

            public override string ToString() {
                return $"WorldCoord:{WorldPos} EntityCount{EntityCount}";
            }

            public Grid* GetGrid(int2 worldPos) {
                Debug.Assert(Ptr != null, "Chunk Ptr == null,has free memory ? " + Coord);
                var localPos = worldPos - WorldPos;
                var localGridCoord = localPos / Grid.Width;
                return Ptr->GetGrid(localGridCoord);
            }
        }

        private Dictionary<int2, ChunkInfo> _coord2Data = new Dictionary<int2, ChunkInfo>();
        private Stack<ChunkInfo> _freeList = new Stack<ChunkInfo>();

        public void DoAwake(int initSizeKB = 512) {
            Debug.Assert(Grid.MemSize == sizeof(Grid),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            Debug.Assert(Chunk.MemSize == sizeof(Chunk),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            int capacity = initSizeKB * 1024 / Chunk.MemSize;
            int totalSize = UnsafeUtility.SizeOf<Chunk>() * capacity;
            var chunks = (Chunk*)UnsafeUtility.Malloc(totalSize);
            _coord2Data.EnsureCapacity(initSizeKB);
            UnsafeUtility.MemClear(chunks, totalSize);
            for (int i = 0; i < capacity; i++) {
                var chunkPtr = &chunks[i];
                var chunk = new ChunkInfo();
                chunk.Ptr = chunkPtr;
                _freeList.Push(chunk);
            }
        }

        public void Destroy() {
            foreach (var ptr in _freeList) {
                UnsafeUtility.Free((void*)ptr.Ptr);
                ptr.Ptr = null;
            }

            _freeList.Clear();
            foreach (var chunk in _coord2Data.Values) {
                var ptr = chunk.Ptr;
                if (ptr != null) {
                    UnsafeUtility.Free(ptr);
                    chunk.Ptr = null;
                }
            }

            _coord2Data.Clear();
        }

        private ChunkInfo AllocChunk() {
            ChunkInfo info = null;
            if (_freeList.Count > 0) {
                info = _freeList.Pop();
            }
            else {
                var ptr = UnsafeUtility.Malloc(Chunk.MemSize);
                UnsafeUtility.MemClear(ptr, Chunk.MemSize);
                info = new ChunkInfo();
                info.Ptr = (Chunk*)ptr;
            }

            info.EntityCount = 0;
            info.Coord = new int2();
            return info;
        }

        private void FreeChunk(ChunkInfo chunk) {
            _freeList.Push(chunk);
        }

        private void TryRemoveChunk(ChunkInfo chunk) {
            if (chunk.EntityCount == 0) {
                FreeChunk(chunk);
                _coord2Data.Remove(chunk.Coord);
            }
        }

        private ChunkInfo GetOrAddChunk(int2 worldPos) {
            var chunkCoord = worldPos / Chunk.Width;
            ChunkInfo chunkInfo = null;
            if (!_coord2Data.TryGetValue(chunkCoord, out chunkInfo)) {
                chunkInfo = AllocChunk();
                chunkInfo.Coord = chunkCoord;
            }

            return chunkInfo;
        }

        public void Update(EntityData data, ref int2 lastCoord, float3 pos) {
            var pos2 = new float2(pos.x, pos.z);
            var worldPos = (int2)math.floor(pos2);
            var gridCoord = worldPos / Grid.Width;
            bool isNeedUpdate = false;
            if (!gridCoord.Equals(lastCoord)) {
                var gridCenter = (lastCoord + new int2(1, 1));
                var diff = math.abs(pos2 - gridCenter);
                isNeedUpdate = math.any(diff > Grid.Width);
            }

            if (isNeedUpdate) {
                var lastChunkCoord = lastCoord / Chunk.GridScaler;
                var curChunkCoord = gridCoord / Chunk.GridScaler ;
                var isNeedUpdateChunk = lastChunkCoord.Equals(curChunkCoord);
                if (isNeedUpdateChunk) {
                    // move chunk
                    var lastPos = lastCoord*Grid.Width;
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    lastGrid->Remove(data);
                    
                    var curPos = gridCoord*Grid.Width;
                    var curChunk = GetOrAddChunk(curPos);
                    var curGrid = curChunk.GetGrid(curPos);
                    curGrid->Add(data);
                }
                else {
                    // move grid
                    var lastPos = lastCoord*Grid.Width;
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    lastGrid->Remove(data);
                    
                    var curPos = gridCoord*Grid.Width;
                    var curGrid = lastChunk.GetGrid(curPos);
                    curGrid->Add(data);
                }
            }
        }

        public int2 AddEntity(EntityData data, float3 pos) {
            var worldPos = (int2)math.floor(new float2(pos.x, pos.z));
            var chunkInfo = GetOrAddChunk(worldPos);
            var grid = chunkInfo.GetGrid(worldPos);
            grid->Add(data);
            chunkInfo.EntityCount++;
            return worldPos / Grid.Width;
        }

        public void RemoveEntity(EntityData data, float3 pos) {
            var worldPos = (int2)math.floor(new float2(pos.x, pos.z));
            var chunkInfo = GetOrAddChunk(worldPos);
            var grid = chunkInfo.GetGrid(worldPos);
            grid->Remove(data);
            chunkInfo.EntityCount--;
            TryRemoveChunk(chunkInfo);
        }
    }
}