using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GamesTan.ECS;
using JetBrains.Annotations;
using GamesTan.ECSInternal;
using Lockstep.Math;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

using EntityIdType = System.Int64;
namespace GamesTan.Spatial {
    [System.Serializable]
    public unsafe class Region {
        public static Region Instance { get; private set; }
        public static bool IsDebugMode = false;

        private const int AllockChunkPageSizeKB = 128;// 每次申请的内存大小，太小会导致频繁申请
        
        [SerializeField] private int _totalEntityCount;
        [SerializeField] private int _extraGridUsedCount = 0;
        [SerializeField] private int _totalChunkCount = 0;
        public List<ChunkInfo> _debugChunkList = new List<ChunkInfo>();
        
        private Dictionary<LVector2Int, ChunkInfo> _chunkCoord2Info = new Dictionary<LVector2Int, ChunkInfo>();
        private Stack<ChunkInfo> _freeChunkList = new Stack<ChunkInfo>();

        private Grid* _extraGrids = null;
        private Stack<UInt32> _freeExtraGrids = new Stack<UInt32>();
        private int _extGridsCapacity;
        public int TotalUsedChunkCount => _chunkCoord2Info.Count;
        public int TotalChunkCount => _chunkCoord2Info.Count + _freeChunkList.Count;

        public void DoAwake(int initSizeKB = 1024) {
            _totalEntityCount = 0;
            _extraGridUsedCount = 0;
            _totalChunkCount = 0;
            _debugChunkList.Clear();
            initSizeKB = math.max(initSizeKB, 128);
            Debug.Assert(Grid.MemSize == sizeof(Grid),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            Debug.Assert(Chunk.MemSize == sizeof(Chunk),
                "Grid size is diff with GridMemSize , but some code is dependent on it");
            {
                _chunkCoord2Info.EnsureCapacity(initSizeKB);
                AllocChunks(initSizeKB);
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

        private void AllocChunks(int sizeKb) {
            int totalSize = sizeKb * 1024;
            int capacity = totalSize / UnsafeUtility.SizeOf<Chunk>();
            var chunks = (Chunk*)UnsafeUtility.Malloc(totalSize);
            UnsafeUtility.MemClear(chunks, totalSize);
            for (int i = 0; i < capacity; i++) {
                var chunkPtr = &chunks[i];
                var chunk = new ChunkInfo();
                chunk.Ptr = chunkPtr;
                chunk.Region = this;
                chunk.IsNeedFree = i == 0; // only the first chunk need to free
                _freeChunkList.Push(chunk);
                _totalChunkCount++;
            }
        }

        public void DoDestroy() {
            foreach (var chunk in _freeChunkList) {
                if (chunk.IsNeedFree && chunk.Ptr != null) {
                    UnsafeUtility.Free((void*)chunk.Ptr);
                    chunk.Ptr = null;
                }

                chunk.Ptr = null;
            }

            _freeChunkList.Clear();
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

        private static List<EntityIdType> _collisionResult = new List<EntityIdType>();
        public List<EntityIdType> QueryCollision(LVector3 pos, float radius) {
            var centerWorldPos = FloorWorldPos(pos);
            var centerGridCoord = WorldPos2GridCoord(centerWorldPos);
            var width = ((int)(math.ceil(radius +1 )))>> Grid.WidthBit;
            width = math.max(1, width);
            _collisionResult.Clear();
            // TODO check faster
            for (int x = -width; x <= width; x++) {
                for (int y = -width; y <= width; y++) {
                    var gridCoord = new LVector2Int(x, y) + centerGridCoord;
                    var worldPos = GridCoord2WorldPos(gridCoord);
                    var chunkInfo = GetOrAddChunk(worldPos);
                    if(chunkInfo.EntityCount ==0) continue;
                    var grid = chunkInfo.GetGrid(worldPos);
                    grid->GetEntities(_collisionResult);
                }
            }

            return _collisionResult;
        }

        public void Update(EntityIdType data, ref LVector2Int coord, LVector3 pos) {
            Instance = this;
            var pos2 = new LVector2(pos.x, pos.z);
            var worldPos = LMath.FloorToInt(pos2);
            var newCoord = WorldPos2GridCoord(worldPos);
            if (!newCoord.Equals(coord)) {
                var gridCenter = (coord + new LVector2Int(1, 1));
                var diff = LMath.Abs(pos2 - (LVector2)gridCenter);
                if (!(diff.x > Grid.Width || diff.y > Grid.Width))
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

        public LVector2Int AddEntity(EntityIdType data,ref LVector2Int coord, LVector3 pos) {
            //if (IsDebugMode) Debug.Log($"AddEntity {data} pos:{pos}");
            var worldPos = FloorWorldPos(pos);
            coord = WorldPos2GridCoord(worldPos);
            var chunkInfo = GetOrAddChunk(worldPos);
            chunkInfo.AddEntity(data, worldPos);
            _totalEntityCount++;
            return WorldPos2GridCoord(worldPos);
        }

        public bool RemoveEntity(EntityIdType data, LVector2Int gridCoord) {
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
            if (_freeChunkList.Count == 0) {
                AllocChunks(AllockChunkPageSizeKB);
            }
            info = _freeChunkList.Pop();
            info.EntityCount = 0;
            info.Coord = new LVector2Int();
            return info;
        }

        public void FreeChunk(ChunkInfo chunkInfo) {
            UnsafeUtility.MemClear(chunkInfo.Ptr, Chunk.MemSize);
            _freeChunkList.Push(chunkInfo);
            _chunkCoord2Info.Remove(chunkInfo.Coord);
#if UNITY_EDITOR
            _debugChunkList.Remove(chunkInfo);
#endif
        }

        private ChunkInfo GetOrAddChunk(LVector2Int worldPos) {
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
            Debug.Assert(extraGridPtr != 0, "Invalid extraGridPtr == 0 ,should not free it!");
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
            Debug.Assert(!isInvalid, "Invalid ExtraGrid Index");
            if (isInvalid)
                return null;
            return &_extraGrids[extraGridPtr];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int GridCoord2ChunkCoord(LVector2Int gridCoord) {
            return gridCoord >> Chunk.GridScalerBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int ChunkCoord2GridCoord(LVector2Int chunkCoord) {
            return chunkCoord << Chunk.GridScalerBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int WorldPos2ChunkCoord(LVector2Int worldPos) {
            return worldPos >> Chunk.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int ChunkCoord2WorldPos(LVector2Int chunkCoord) {
            return chunkCoord << Chunk.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int WorldPos2GridCoord(LVector2Int worldPos) {
            return worldPos >> Grid.WidthBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int GridCoord2WorldPos(LVector2Int gridCoord) {
            return gridCoord << Grid.WidthBit;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LVector2Int FloorWorldPos(LVector3 pos) {
            return new LVector2Int(LMath.FloorToInt(pos.x), LMath.FloorToInt(pos.z));
        }

    }
}