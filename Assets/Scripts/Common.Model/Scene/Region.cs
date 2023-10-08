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
        public const int ArySize = 15;
        public const int WidthBit = 1;
        public const int Width = 1 << WidthBit;
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
            for (int i = 0; i < count; i++) {
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
    }

    public unsafe class ChunkInfo {
        public int EntityCount;

        public Chunk* Ptr;

        // left bottom corner
        public int2 WorldPos => Coord * Chunk.Width;
        public int2 Coord;

        public override string ToString() {
            return $"WorldCoord:{WorldPos} EntityCount{EntityCount}";
        }

        public Grid* GetGrid(int2 worldPos) {
            DebugUtil.Assert(Ptr != null, "Chunk Ptr == null,has free memory ? " + Coord);
            var localPos = worldPos - WorldPos;
            var localGridCoord = Region.WorldPos2GridCoord(localPos);
            return Ptr->GetGrid(localGridCoord);
        }
    }

    public unsafe class Region {
        private Dictionary<int2, ChunkInfo> _coord2Data = new Dictionary<int2, ChunkInfo>();
        private Stack<ChunkInfo> _freeList = new Stack<ChunkInfo>();

        public int TotalChunkCount => _coord2Data.Count;
        public int TotalEntityCount;

        public void DoAwake(int initSizeKB = 1024) {
            DebugUtil.Assert(Grid.MemSize == sizeof(Grid),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            DebugUtil.Assert(Chunk.MemSize == sizeof(Chunk),
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
            var chunkCoord = WorldPos2ChunkCoord(worldPos);
            ChunkInfo chunkInfo = null;
            if (!_coord2Data.TryGetValue(chunkCoord, out chunkInfo)) {
                chunkInfo = AllocChunk();
                chunkInfo.Coord = chunkCoord;
            }

            return chunkInfo;
        }


        public void Update(EntityData data, ref int2 coord, float3 pos) {
            var pos2 = new float2(pos.x, pos.z);
            var worldPos = (int2)math.floor(pos2);
            var newCoord = WorldPos2GridCoord(worldPos);
            if (!newCoord.Equals(coord)) {
                var gridCenter = (coord + new int2(1, 1));
                var diff = math.abs(pos2 - gridCenter);
                if (!math.any(diff > Grid.Width))
                    return;
                var lastChunkCoord = GridCoord2ChunkCoord(coord);
                var curChunkCoord = GridCoord2ChunkCoord(newCoord);
                coord = newCoord;
                var isNeedUpdateChunk = lastChunkCoord.Equals(curChunkCoord);
                if (isNeedUpdateChunk) {
                    // move chunk
                    var lastPos = GridCoord2WorldPos(coord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    lastGrid->Remove(data);

                    var curPos = GridCoord2WorldPos(newCoord);
                    var curChunk = GetOrAddChunk(curPos);
                    var curGrid = curChunk.GetGrid(curPos);
                    curGrid->Add(data);
                }
                else {
                    // move grid
                    var lastPos = GridCoord2WorldPos(coord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    lastGrid->Remove(data);

                    var curPos = GridCoord2WorldPos(newCoord);
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
            TotalEntityCount++;
            return WorldPos2GridCoord(worldPos);
        }

        public void RemoveEntity(EntityData data, int2 gridCoord) {
            var worldPos = GridCoord2WorldPos(gridCoord);
            var chunkInfo = GetOrAddChunk(worldPos);
            var grid = chunkInfo.GetGrid(worldPos);
            grid->Remove(data);
            chunkInfo.EntityCount--;
            TryRemoveChunk(chunkInfo);
            TotalEntityCount--;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GridCoord2ChunkCoord(int2 gridCoord) {
            return gridCoord >> Chunk.GridScalerBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ChunkCoord2GridCoord(int2 chunkCoord) {
            return chunkCoord << Chunk.GridScalerBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldPos2ChunkCoord(int2 worldPos) {
            return worldPos >> Chunk.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ChunkCoord2WorldPos(int2 chunkCoord) {
            return chunkCoord << Chunk.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldPos2GridCoord(int2 worldPos) {
            return worldPos >> Grid.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GridCoord2WorldPos(int2 gridCoord) {
            return gridCoord << Grid.WidthBit;
        }
    }
}