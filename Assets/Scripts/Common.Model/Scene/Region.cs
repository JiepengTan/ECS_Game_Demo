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
        [FieldOffset(4)] public UInt32 NextGridPtr; // 同一个格子内部太多物体的时候，使用外链进行存储
        [FieldOffset(8)] public fixed EntityStorageData Entities[ArySize];
        public bool IsLocalFull => Count >= ArySize;
        public bool HasExtData => NextGridPtr == 0;


        public override string ToString() {
            return base.ToString();
        }
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

        public bool IsNeedFree;

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

        private Grid* _extraGrids = null;
        private Stack<UInt32> _freeExtraGrids = new Stack<UInt32>();
        private int _extGridsCapacity;
        public int TotalChunkCount => _coord2Data.Count;
        public int TotalEntityCount;


        public void DoAwake(int initSizeKB = 1024) {
            initSizeKB = math.max(initSizeKB, 128);
            DebugUtil.Assert(Grid.MemSize == sizeof(Grid),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            DebugUtil.Assert(Chunk.MemSize == sizeof(Chunk),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            {
                int totalSize = initSizeKB * 1024 ;
                int capacity = totalSize / UnsafeUtility.SizeOf<Chunk>() ;
                var chunks = (Chunk*)UnsafeUtility.Malloc(totalSize);
                _coord2Data.EnsureCapacity(initSizeKB);
                UnsafeUtility.MemClear(chunks, totalSize);
                for (int i = 0; i < capacity; i++) {
                    var chunkPtr = &chunks[i];
                    var chunk = new ChunkInfo();
                    chunk.Ptr = chunkPtr;
                    chunk.IsNeedFree = i == 0; // only the first chunk need to free
                    _freeList.Push(chunk);
                }
            }
            {
                int totalSize  = initSizeKB * 128; //   1/8 size of AllChunkSize
                _extGridsCapacity = totalSize / UnsafeUtility.SizeOf<Grid>();
                _extraGrids = (Grid*)UnsafeUtility.Malloc(totalSize);
                for (int i = _extGridsCapacity-1; i >=0; --i) {
                    _freeExtraGrids.Push((UInt32)(i));
                }
            }
        }

        public void DoDestroy() {
            foreach (var chunk in _freeList) {
                if (chunk.IsNeedFree && chunk.Ptr != null) {
                    UnsafeUtility.Free((void*)chunk.Ptr);
                    chunk.Ptr = null;
                }

                chunk.Ptr = null;
            }

            _freeList.Clear();
            foreach (var chunk in _coord2Data.Values) {
                if (chunk.IsNeedFree && chunk.Ptr != null) {
                    UnsafeUtility.Free((void*)chunk.Ptr);
                    chunk.Ptr = null;
                }
            }

            _coord2Data.Clear();

            if (_extraGrids != null) {
                UnsafeUtility.Free(_extraGrids);
                _extraGrids = null;
            }

            _extGridsCapacity = 0;
            _freeExtraGrids.Clear();
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
                var isNeedUpdateChunk = lastChunkCoord.Equals(curChunkCoord);
                if (isNeedUpdateChunk) {
                    // move chunk
                    var lastPos = GridCoord2WorldPos(coord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    var succ = RemoveGridEntity(lastGrid, data);
                    DebugUtil.Assert(succ, "Remove EntityFailed!");
                    var curPos = GridCoord2WorldPos(newCoord);
                    var curChunk = GetOrAddChunk(curPos);
                    var curGrid = curChunk.GetGrid(curPos);
                    AddGridEntity(curGrid, data);
                }
                else {
                    // move grid
                    var lastPos = GridCoord2WorldPos(coord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    var lastGrid = lastChunk.GetGrid(lastPos);
                    var succ = RemoveGridEntity(lastGrid, data);
                    DebugUtil.Assert(succ, "Remove EntityFailed!");

                    var curPos = GridCoord2WorldPos(newCoord);
                    var curGrid = lastChunk.GetGrid(curPos);
                    AddGridEntity(curGrid, data);
                }

                coord = newCoord;
            }
        }


        public int2 AddEntity(EntityData data, float3 pos) {
            var worldPos = (int2)math.floor(new float2(pos.x, pos.z));
            var chunkInfo = GetOrAddChunk(worldPos);
            var grid = chunkInfo.GetGrid(worldPos);
            AddGridEntity(grid, data);
            chunkInfo.EntityCount++;
            TotalEntityCount++;
            return WorldPos2GridCoord(worldPos);
        }

        public void RemoveEntity(EntityData data, int2 gridCoord) {
            var worldPos = GridCoord2WorldPos(gridCoord);
            var chunkInfo = GetOrAddChunk(worldPos);
            var grid = chunkInfo.GetGrid(worldPos);
            if (RemoveGridEntity(grid, data)) {
                chunkInfo.EntityCount--;
                if (chunkInfo.EntityCount == 0) {
                    FreeChunk(chunkInfo);
                }
                TotalEntityCount--;
            }
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
            _coord2Data.Remove(chunk.Coord);
        }

        private ChunkInfo GetOrAddChunk(int2 worldPos) {
            var chunkCoord = WorldPos2ChunkCoord(worldPos);
            ChunkInfo chunkInfo = null;
            if (!_coord2Data.TryGetValue(chunkCoord, out chunkInfo)) {
                chunkInfo = AllocChunk();
                chunkInfo.Coord = chunkCoord;
                _coord2Data[chunkCoord] = chunkInfo;
            }

            return chunkInfo;
        }
        
        private UInt32 AllocExtGrid() {
            if (_freeExtraGrids.Count == 0) {
                var rawCap = _extGridsCapacity;
                _extGridsCapacity *= 2;
                int rawSize = UnsafeUtility.SizeOf<Chunk>() * rawCap;
                int totalSize = UnsafeUtility.SizeOf<Chunk>() * _extGridsCapacity;
                UnsafeUtility.Realloc(_extraGrids, rawSize, totalSize);
                for (int i = _extGridsCapacity - 1; i >=rawCap; --i) {
                    _freeExtraGrids.Push((UInt32)i);
                }
            }
            return _freeExtraGrids.Pop();
        }

        private void FreeExtraGrid(UInt32 extraGridPtr) {
            _freeExtraGrids.Push(extraGridPtr);
        }

        private Grid* GetExtraGrid(UInt32 extraGridPtr) {
            DebugUtil.Assert(extraGridPtr < _extGridsCapacity, "Invalid ExtraGrid Index");
            if (extraGridPtr >= _extGridsCapacity)
                return null;
            return &_extraGrids[extraGridPtr];
        }

        private void AddGridEntity(Grid* grid, EntityData entityData) {
            int count = grid->Count;
            if (count >= Grid.ArySize) {
                var offset = count % Grid.ArySize;
                Grid* lastGrid = grid;
                for (int i = Grid.ArySize; i < count; i+=Grid.ArySize) {
                    lastGrid = GetExtraGrid(lastGrid->NextGridPtr);
                }
                if (offset == 0) {
                    lastGrid->NextGridPtr = AllocExtGrid();
                }
                lastGrid->Entities[offset] = (EntityStorageData)entityData;
                grid->Count++;
                return;
            }

            grid->Entities[grid->Count] = (EntityStorageData)entityData;
            grid->Count++;
        }

        private bool RemoveGridEntity(Grid* grid, EntityData entityData) {
            var entity = (EntityStorageData)entityData;
            int count = grid->Count;
            if (count >= Grid.ArySize) {
                //1. 找到匹配的 Grid*, 和对应的 Entity下标
                Grid* matchGrid = null;
                int matchOffset = 0;
                Grid* curGrid = grid;
                for (int i = 0; i < count; i++) {
                    var offset = i % Grid.ArySize;
                    if (offset ==0 && i != 0) {
                        curGrid = GetExtraGrid(curGrid->NextGridPtr);
                    }
                    if (curGrid->Entities[offset] == entity) {
                        matchGrid = curGrid;
                        matchOffset = offset;
                        break;
                    }
                }

                if (matchGrid == null) 
                    return false; // no match
                
                //2. 找到最后一个EntityData 
                EntityStorageData lastEntity = 0;
                curGrid = grid;
                Grid* preGrid = null;
                for (int i = 0; i < count; i++) {
                    var offset = i % Grid.ArySize;
                    if (offset == 0 && i != 0) {
                        preGrid = curGrid;
                        curGrid = GetExtraGrid(curGrid->NextGridPtr);
                    }
                    if (i == count - 1) {
                        lastEntity = curGrid->Entities[offset];
                    }
                }
                //3. 覆盖数据，count--;
                matchGrid->Entities[matchOffset] = lastEntity;
                
                //4. 如果最后一个 Grid 空了，记得释放 Grid
                if (count % Grid.ArySize == 1) {
                    FreeExtraGrid(preGrid->NextGridPtr);
                }
                return true;
            }
            
            for (int i = 0; i < count; i++) {
                if (grid->Entities[i] == entity) {
                    grid->Count--;
                    grid->Entities[i] = grid->Entities[grid->Count];
                    grid->Entities[grid->Count] = EntityData.DefaultObjectIntData;
                    return true;
                }
            }
            return false;
        }

        private EntityData GetGridEntity(Grid* grid, uint idx) {
            if (idx > grid->Count) return EntityData.DefaultObject;
            if (grid->IsLocalFull) {
                Debug.LogError("TODO implements ");
                return EntityData.DefaultObject;
            }

            return (EntityData)(grid->Entities[idx]);
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