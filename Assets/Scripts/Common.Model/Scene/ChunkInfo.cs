using System;
using System.Diagnostics;
using GamesTan.ECS;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

namespace Gamestan.Spatial {
    public unsafe class ChunkInfo {
        public int EntityCount;

        public Chunk* Ptr;

        // left bottom corner
        public int2 WorldPos => Coord * Chunk.Width;
        public int2 Coord;

        public bool IsNeedFree;

        public Region Region;

        public override string ToString() {
            return $"WorldCoord:{WorldPos} EntityCount{EntityCount} \n {(Ptr == null ? "null" : Ptr->ToString())}";
        }

        public Grid* GetGrid(int2 worldPos) {
            DebugUtil.Assert(Ptr != null, "Chunk Ptr == null,has free memory ? " + Coord);
            var localPos = worldPos - WorldPos;
            var localGridCoord = Region.WorldPos2GridCoord(localPos);
            return Ptr->GetGrid(localGridCoord);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public void DebugDump(string msg = "") {
            if (Region.IsDebugMode) {
                Debug.Log(msg + "  " + ToString());
            }
        }

        public EntityData GetGridEntity(Grid* grid, uint idx) {
            if (idx > grid->Count) return EntityData.DefaultObject;
            if (grid->IsLocalFull) {
                Debug.LogError("TODO implements ");
                return EntityData.DefaultObject;
            }

            return (EntityData)(grid->Entities[idx]);
        }

        public void MoveEntity(EntityData data, int2 lastPos, ChunkInfo newChunk, int2 newPos) {
            DebugDump(
                $"MoveEntity CrossChunk {Coord}=>{newChunk.Coord} entity:{data} worldPos:{lastPos} =>{newPos}   newChunk {newChunk}");
            var lastGrid = GetGrid(lastPos);
            var succ = RemoveGridEntity(lastGrid, data);
            if (succ) {
                var curGrid = newChunk.GetGrid(newPos);
                newChunk.AddGridEntity(curGrid, data);
                EntityCount--;
                newChunk.EntityCount++;
                return;
            }

            DebugUtil.Assert(succ, "MoveEntity EntityFailed!");
            RemoveGridEntity(lastGrid, data);
        }

        public void MoveEntity(EntityData data, int2 lastPos, int2 newPos) {
            DebugDump($"MoveEntity InChunk {Coord}  entity:{data} worldPos:{lastPos} =>{newPos} ");
            var lastGrid = GetGrid(lastPos);
            var succ = RemoveGridEntity(lastGrid, data);
            if (succ) {
                var curGrid = GetGrid(newPos);
                AddGridEntity(curGrid, data);
                return;
            }

            DebugUtil.Assert(succ, "MoveEntity EntityFailed!");
            RemoveGridEntity(lastGrid, data);
        }

        public bool RemoveEntity(EntityData data, int2 worldPos) {
            var grid = GetGrid(worldPos);
            if (RemoveGridEntity(grid, data)) {
                EntityCount--;
                DebugDump($"RemoveEntity {data} gridCoord:{worldPos} succ");
                return true;
            }

            Debug.Log($"RemoveEntity {data} gridCoord:{worldPos} Faild");
            return false;
        }

        public void AddEntity(EntityData data, int2 worldPos) {
            var grid = GetGrid(worldPos);
            AddGridEntity(grid, data);
            EntityCount++;
            DebugDump($"AddEntity {data} pos:{worldPos}");
        }

        public void AddGridEntity(Grid* grid, EntityData entityData) {
            int count = grid->Count;
            if (count >= Grid.ArySize) {
                var offset = count % Grid.ArySize;
                Grid* lastGrid = grid;
                for (int i = Grid.ArySize; i < count; i += Grid.ArySize) {
                    lastGrid = Region.GetExtraGrid(lastGrid->NextGridPtr);
                }

                if (offset == 0) {
                    lastGrid->NextGridPtr = Region.AllocExtGrid();
                }

                lastGrid->Entities[offset] = (UInt64)entityData;
                grid->Count++;
                return;
            }

            grid->Entities[grid->Count] = (UInt64)entityData;
            grid->Count++;
        }

        public bool RemoveGridEntity(Grid* grid, EntityData entityData) {
            var entity = (UInt64)entityData;
            int count = grid->Count;
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
                        break;
                    }
                }

                if (matchGrid == null)
                    return false; // no match

                //2. 找到最后一个EntityData 
                UInt64 lastEntity = 0;
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

                //4. 如果最后一个 Grid 空了，记得释放 Grid
                if (count % Grid.ArySize == 1) {
                    Region.FreeExtraGrid(preGrid->NextGridPtr);
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
    }
}