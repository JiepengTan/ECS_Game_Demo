﻿//#define DEBUG_REGION 
using System;
using System.Diagnostics;
using GamesTan.ECS;
using Lockstep.Math;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using EntityIdType = System.Int64;// TODO use int for entityID

namespace GamesTan.Spatial {
    [System.Serializable]
    public unsafe class ChunkInfo {
        public int EntityCount;
        public Vector2Int DebugCoord;
        public Chunk* Ptr;
        [NonSerialized] public LVector2Int _coord;
        [NonSerialized] public bool IsNeedFree;

        // left bottom corner
        public LVector2Int WorldPos => _coord * Chunk.Width;

        public LVector2Int Coord {
            get => _coord;
            set {
                _coord = value;
                DebugCoord = new Vector2Int(value.x,value.y);
            }
        }

        [NonSerialized] public Region Region;

        public override bool Equals(object obj) {
            var chunk = obj as ChunkInfo;
            return chunk._coord == _coord;
        }

        public override int GetHashCode() {
            return _coord.x*10000+_coord.y;
        }

        public override string ToString() {
            return $"WorldCoord:{WorldPos} EntityCount{EntityCount} \n {(Ptr == null ? "null" : Ptr->ToString())}";
        }

        public Grid* GetGrid(LVector2Int worldPos) {
            var localPos = worldPos - WorldPos;
            var localGridCoord = Region.WorldPos2GridCoord(localPos);
            return Ptr->GetGrid(localGridCoord);
        }

        [Conditional("DEBUG_REGION")]
        public void DebugDump(string msg = "") {
            if (Region.IsDebugMode) {
                Debug.Log(msg + "  " + ToString());
            }
        }

        public EntityIdType GetGridEntity(Grid* grid, uint idx) {
            if (idx > grid->Count) return Entity.InvalidEntityId;
            if (grid->IsLocalFull) {
                Debug.LogError("TODO implements ");
                return  Entity.InvalidEntityId;
            }

            return (EntityIdType)(grid->Entities[idx]);
        }

        public void MoveEntity(EntityIdType data, LVector2Int lastPos, ChunkInfo newChunk, LVector2Int newPos) {
            DebugDump($"MoveEntity Before CrossChunk {_coord}=>{newChunk._coord} worldPos:{lastPos} =>{newPos} entity:{data}   newChunk {newChunk}");
            var lastGrid = GetGrid(lastPos);
            var succ = RemoveGridEntity(lastGrid, data);
            if (succ) {
                var curGrid = newChunk.GetGrid(newPos);
                newChunk.AddGridEntity(curGrid, data);
                EntityCount--;
                newChunk.EntityCount++;
                DebugDump($"MoveEntity After CrossChunk {_coord}=>{newChunk._coord} worldPos:{lastPos} =>{newPos} entity:{data}   newChunk {newChunk}");
                return;
            }

            Debug.Assert(succ, $"MoveEntity Failed CrossChunk {_coord}=>{newChunk._coord} worldPos:{lastPos} =>{newPos} entity:{data}  {this} newChunk {newChunk} ");
        }

        public void MoveEntity(EntityIdType data, LVector2Int lastPos, LVector2Int newPos) {
            DebugDump($"MoveEntity Before InChunk {_coord} worldPos:{lastPos} =>{newPos}  entity:{data} ");
            var lastGrid = GetGrid(lastPos);
            var succ = RemoveGridEntity(lastGrid, data);
            if (succ) {
                var curGrid = GetGrid(newPos);
                AddGridEntity(curGrid, data);
                DebugDump($"MoveEntity After InChunk {_coord} worldPos:{lastPos} =>{newPos}  entity:{data} ");
                return;
            }
            Debug.Assert(succ, $"MoveEntity Failed InChunk {_coord} worldPos:{lastPos} =>{newPos}  entity:{data} {this}");
        }

        public bool RemoveEntity(EntityIdType data, LVector2Int worldPos) {
            var grid = GetGrid(worldPos);
            if (RemoveGridEntity(grid, data)) {
                EntityCount--;
                DebugDump($"RemoveEntity {data} gridCoord:{worldPos} succ");
                return true;
            }

            Debug.Log($"RemoveEntity {data} gridCoord:{worldPos} Faild");
            return false;
        }

        public void AddEntity(EntityIdType data, LVector2Int worldPos) {
            var grid = GetGrid(worldPos);
            AddGridEntity(grid, data);
            EntityCount++;
            DebugDump($"AddEntity {data} pos:{worldPos}");
        }

        public void AddGridEntity(Grid* grid, EntityIdType entityData) {
            int count = grid->Count;
            if (count >= Grid.ArySize) {
                var offset = count % Grid.ArySize;
                Grid* lastGrid = grid;
                for (int i = Grid.ArySize; i < count; i += Grid.ArySize) {
                    lastGrid = Region.GetExtraGrid(lastGrid->NextGridPtr);
                }

                if (offset == 0) {
                    lastGrid->NextGridPtr = Region.AllocExtGrid();
                    lastGrid = Region.GetExtraGrid(lastGrid->NextGridPtr);
                }

                lastGrid->Entities[offset] = (Int64)entityData;
                grid->Count++;
                return;
            }

            grid->Entities[grid->Count] = (Int64)entityData;
            grid->Count++;
        }

        public bool RemoveGridEntity(Grid* grid, EntityIdType entityData) {
            if (Region.IsDebugMode) {
                Debug.Log($"RemoveGridEntity Before  remainCount:{grid->Count} ");
            }
            var entity = (Int64)entityData;
            int count = grid->Count;
            int matchIdx = 0;
            if (count >= Grid.ArySize) {
                //1. 找到匹配的 Grid*, 和对应的 Entity下标
                Grid* matchGrid = null;
                int matchOffset = 0;
                Grid* curGrid = grid;
                for (int i = 0; i < count; i++) {
                    var offset = i % Grid.ArySize;
                    if (offset == 0 && i != 0) {
                        curGrid = Region.GetExtraGrid(curGrid->NextGridPtr);
                    }

                    if (curGrid->Entities[offset] == entity) {
                        matchGrid = curGrid;
                        matchOffset = offset;
                        matchIdx = i;
                        break;
                    }
                }

                if (matchGrid == null)
                    return false; // no match

                //2. 找到最后一个EntityData 
                Int64 lastEntity = 0;
                curGrid = grid;
                Grid* preGrid = null;
                for (int i = 0; i < count; i++) {
                    var offset = i % Grid.ArySize;
                    if (offset == 0 && i != 0) {
                        preGrid = curGrid;
                        curGrid = Region.GetExtraGrid(curGrid->NextGridPtr);
                    }

                    if (i == count - 1) {
                        lastEntity = curGrid->Entities[offset];
                    }
                }

                //3. 覆盖数据，count--;
                matchGrid->Entities[matchOffset] = lastEntity;
                grid->Count--;

                //4. 如果最后一个 Grid 空了，记得释放 Grid
                if (count % Grid.ArySize == 1) {
                    Region.FreeExtraGrid(preGrid->NextGridPtr);
                    preGrid->NextGridPtr = 0;
                }

                if (Region.IsDebugMode) {
                    Debug.Log($"RemoveGridEntity After {matchIdx} remainCount:{grid->Count} ");
                }

                return true;
            }

            for (int i = 0; i < count; i++) {
                if (grid->Entities[i] == entity) {
                    matchIdx = i;
                    grid->Count--;
                    grid->Entities[i] = grid->Entities[grid->Count];
                    grid->Entities[grid->Count] = Entity.InvalidEntityId;
                    if (Region.IsDebugMode) {
                        Debug.Log($"RemoveGridEntity Local After {matchIdx} remainCount:{grid->Count} ");
                    }
                    return true;
                }
            }

            return false;
        }
    }
}