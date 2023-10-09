using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GamesTan.ECS;
using JetBrains.Annotations;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;


namespace Gamestan.Spatial {
    [System.Serializable]
    public unsafe class Region {
#if UNITY_EDITOR
        public List<ChunkInfo> _debugChunkList = new List<ChunkInfo>();
#endif
        private Dictionary<int2, ChunkInfo> _chunkCoord2Info = new Dictionary<int2, ChunkInfo>();
        private Stack<ChunkInfo> _freeList = new Stack<ChunkInfo>();

        public static Region Instance;
        private Grid* _extraGrids = null;
        private Stack<UInt32> _freeExtraGrids = new Stack<UInt32>();
        private int _extGridsCapacity;
        public int TotalChunkCount => _chunkCoord2Info.Count;
        [SerializeField] private int _totalEntityCount;
        [SerializeField]  private int _extraGridUsedCount = 0;

        public static bool IsDebugMode = false;

        public void DoAwake(int initSizeKB = 1024) {
            _totalEntityCount = 0;
            _extraGridUsedCount = 0;
            _debugChunkList.Clear();
            initSizeKB = math.max(initSizeKB, 128);
            DebugUtil.Assert(Grid.MemSize == sizeof(Grid),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            DebugUtil.Assert(Chunk.MemSize == sizeof(Chunk),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            {
                int totalSize = initSizeKB * 1024;
                int capacity = totalSize / UnsafeUtility.SizeOf<Chunk>();
                var chunks = (Chunk*)UnsafeUtility.Malloc(totalSize);
                _chunkCoord2Info.EnsureCapacity(initSizeKB);
                UnsafeUtility.MemClear(chunks, totalSize);
                for (int i = 0; i < capacity; i++) {
                    var chunkPtr = &chunks[i];
                    var chunk = new ChunkInfo();
                    chunk.Ptr = chunkPtr;
                    chunk.Region = this;
                    chunk.IsNeedFree = i == 0; // only the first chunk need to free
                    _freeList.Push(chunk);
                }
            }
            {
                int totalSize = initSizeKB * 128; //   1/8 size of AllChunkSize
                _extGridsCapacity = totalSize / UnsafeUtility.SizeOf<Grid>();
                _extraGrids = (Grid*)UnsafeUtility.Malloc(totalSize);
                for (int i = _extGridsCapacity - 1; i >= 0; --i) {
                    _freeExtraGrids.Push((UInt32)(i));
                }

                // 第一个空间弃用，NextGridPtr ==0 标记为无效指针，方便debug
                _freeExtraGrids.Pop();
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
            foreach (var chunk in _chunkCoord2Info.Values) {
                if (chunk.IsNeedFree && chunk.Ptr != null) {
                    UnsafeUtility.Free((void*)chunk.Ptr);
                    chunk.Ptr = null;
                }
            }

            _chunkCoord2Info.Clear();
            _debugChunkList.Clear();

            if (_extraGrids != null) {
                UnsafeUtility.Free(_extraGrids);
                _extraGrids = null;
            }

            _extGridsCapacity = 0;
            _freeExtraGrids.Clear();
        }


        public void Update(EntityData data, ref int2 coord, float3 pos) {
            Instance = this;
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
                var isCrossChunk = !lastChunkCoord.Equals(curChunkCoord);
                if (IsDebugMode) {
                    Debug.Log($"Update Entity {data} fromCoord {coord} => {newCoord} isCrossChunk = {isCrossChunk}");
                }
                if (isCrossChunk) {
                    // move chunk
                    var lastPos = GridCoord2WorldPos(coord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    var curPos = GridCoord2WorldPos(newCoord);
                    var curChunk = GetOrAddChunk(curPos);
                    lastChunk.MoveEntity(data, lastPos, curChunk, curPos);
                    if (lastChunk.EntityCount == 0) {
                        FreeChunk(lastChunk);
                    }
                }
                else {
                    // move grid
                    var lastPos = GridCoord2WorldPos(coord);
                    var curPos = GridCoord2WorldPos(newCoord);
                    var lastChunk = GetOrAddChunk(lastPos);
                    lastChunk.MoveEntity(data, lastPos, curPos);
                }

                coord = newCoord;
            }
        }

        public int2 AddEntity(EntityData data,ref int2 coord, float3 pos) {
            //if (IsDebugMode) Debug.Log($"AddEntity {data} pos:{pos}");
            var worldPos = (int2)math.floor(new float2(pos.x, pos.z));
            coord = WorldPos2GridCoord(worldPos);
            var chunkInfo = GetOrAddChunk(worldPos);
            chunkInfo.AddEntity(data, worldPos);
            _totalEntityCount++;
            return WorldPos2GridCoord(worldPos);
        }

        public bool RemoveEntity(EntityData data, int2 gridCoord) {
            //if (IsDebugMode) Debug.Log($"RemoveEntity {data} gridCoord:{gridCoord}");
            var worldPos = GridCoord2WorldPos(gridCoord);
            var chunkInfo = GetOrAddChunk(worldPos);
            var isSucc = chunkInfo.RemoveEntity(data, worldPos);
            if (isSucc) {
                _totalEntityCount--;
                if (chunkInfo.EntityCount == 0) {
                    FreeChunk(chunkInfo);
                }
            }

            return isSucc;
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
                info.Region = this;
            }

            info.EntityCount = 0;
            info.Coord = new int2();
            return info;
        }

        public void FreeChunk(ChunkInfo chunkInfo) {
            UnsafeUtility.MemClear(chunkInfo.Ptr, Chunk.MemSize);
            _freeList.Push(chunkInfo);
            _chunkCoord2Info.Remove(chunkInfo.Coord);
#if UNITY_EDITOR
            _debugChunkList.Remove(chunkInfo);
#endif
        }

        private ChunkInfo GetOrAddChunk(int2 worldPos) {
            var chunkCoord = WorldPos2ChunkCoord(worldPos);
            ChunkInfo chunkInfo = null;
            if (!_chunkCoord2Info.TryGetValue(chunkCoord, out chunkInfo)) {
                chunkInfo = AllocChunk();
                chunkInfo.Coord = chunkCoord;
                _chunkCoord2Info[chunkCoord] = chunkInfo;
#if UNITY_EDITOR
                _debugChunkList.Add(chunkInfo);
#endif
            }

            return chunkInfo;
        }

        public UInt32 AllocExtGrid() {
            if (_freeExtraGrids.Count == 0) {
                var rawCap = _extGridsCapacity;
                _extGridsCapacity *= 2;
                int rawSize = UnsafeUtility.SizeOf<Chunk>() * rawCap;
                int totalSize = UnsafeUtility.SizeOf<Chunk>() * _extGridsCapacity;
                UnsafeUtility.Realloc(_extraGrids, rawSize, totalSize);
                for (int i = _extGridsCapacity - 1; i >= rawCap; --i) {
                    _freeExtraGrids.Push((UInt32)i);
                }
            }

            var id =  _freeExtraGrids.Pop();
            if (IsDebugMode) {
                Debug.Log($"AllocExtGrid {id}  remain:{_freeExtraGrids.Count}"  );
            }

            _extraGridUsedCount++;
            return id;
        }

        public void FreeExtraGrid(UInt32 extraGridPtr) {
            DebugUtil.Assert(extraGridPtr != 0, "Invalid extraGridPtr == 0 ,should not free it!");
            if (extraGridPtr == 0) {
                Debug.LogError("Invalid extraGridPtr == 0 ,should not free it!");
                return;
            }

            _freeExtraGrids.Push(extraGridPtr);
            if (IsDebugMode) {
                Debug.Log($"FreeExtraGrid {extraGridPtr}  remain:{_freeExtraGrids.Count}"  );
            }

            _extraGridUsedCount--;
        }

        public Grid* GetExtraGrid(UInt32 extraGridPtr) {
            var isInvalid = extraGridPtr == 0 || extraGridPtr >= _extGridsCapacity;
            DebugUtil.Assert(!isInvalid, "Invalid ExtraGrid Index");
            if (isInvalid)
                return null;
            return &_extraGrids[extraGridPtr];
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