using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using GamesTan.ECS;
using GamesTan.ECS.Game;
using GamesTan.ECSInternal;
using Lockstep.Math;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.Rendering {
    // Preferrably want to have all buffer structs in power of 2...
    // 6 * 4 bytes = 24 bytes
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct InstanceBound {
        public float3 boundsCenter; // 3
        public float3 boundsExtents; // 6

        public override string ToString() {
            return $"center:{boundsCenter} ext:{boundsExtents}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IndirectMatrix {
        public Vector4 row0; // 4
        public Vector4 row1; // 8
        public Vector4 row2; // 12
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SortingData {
        public uint drawCallInstanceIndex; // 1
        public uint distanceToCam; // 2
        public uint threadDispatchID; //3

        public override string ToString() {
            return $"dist:{distanceToCam} instanceId:{drawCallInstanceIndex}  tid:{threadDispatchID}";
        }
    };


    [System.Serializable]
    public class IndirectRenderingMesh {
        public Mesh mesh;
        public Material material;
        public MaterialPropertyBlock lod00MatPropBlock;
        public MaterialPropertyBlock lod01MatPropBlock;
        public MaterialPropertyBlock lod02MatPropBlock;
        public MaterialPropertyBlock shadowLod00MatPropBlock;
        public MaterialPropertyBlock shadowLod01MatPropBlock;
        public MaterialPropertyBlock shadowLod02MatPropBlock;
        public uint numOfVerticesLod00;
        public uint numOfVerticesLod01;
        public uint numOfVerticesLod02;
        public uint numOfIndicesLod00;
        public uint numOfIndicesLod01;
        public uint numOfIndicesLod02;
        public Bounds originalBounds;
    }

    public struct RendererData {
        public int prefabIdx;

        public Transform3D trans;
        public AnimRenderData anim;
        public InstanceBound bound; // TODO 

        public RendererData(int prefabIdx, Transform3D trans, AnimRenderData anim) {
            this.prefabIdx = prefabIdx;
            this.anim = anim;
            this.trans = trans;
            this.bound = new InstanceBound();
        }
    }
    public unsafe class InstanceRenderData {
        public const int MIN_CAPACITY = 128; // min size is 128
        public int Capacity;

        public List<float3> prefabSize = new List<float3>();
        public List<List<RendererData>> RenderData = new List<List<RendererData>>();
        private List<int> _configs = new List<int>();
        private List<int> _counts = new List<int>();


        public bool isDirty;

        public Transform3DForRendering[] transformData;

        public InstanceBound[] bounds;

        public SortingData[] sortingData;

        public AnimRenderDataForRendering[] animData;


        public Action OnLayoutChangedEvent;

        public void OnFrameStart() {
            foreach (var info in RenderData) {
                info.Clear();
            }
        }

        [BurstCompile]
        public void OnFrameEnd() {
            var totalCount = 0;
            for (int i = 0; i < RenderData.Count; i++) {
                var info = RenderData[i];
                totalCount += info.Count;
                _counts[i] = info.Count;
            }

            ResetLayout(_configs, _counts);

            int offset = 0;
            for (int prefabIdx = 0; prefabIdx < RenderData.Count; prefabIdx++) {
                var info = RenderData[prefabIdx];
                var count = info.Count;
                for (int instanceIdx = 0; instanceIdx < count; instanceIdx++) {
                    var item = info[instanceIdx];
                    var curIdx = offset + instanceIdx;
                    transformData[curIdx].From(ref item.trans);

                    InstanceBound bound = new InstanceBound();
                    bound.boundsCenter = item.trans.Position.ToVector3();
                    bound.boundsExtents = item.trans.Scale.ToVector3() * prefabSize[item.prefabIdx]; // TODO correct bound size
                    bounds[curIdx] = bound;
                    sortingData[curIdx].drawCallInstanceIndex = (((uint)prefabIdx << 24) + ((uint)curIdx));
                    animData[curIdx].From(ref item.anim);
                }

                offset += info.Count;
            }

            var capacity = sortingData.Length;
            // TODO make sure the sortingdata is correct
            for (int curIdx = totalCount; curIdx < capacity; curIdx++) {
                sortingData[curIdx].drawCallInstanceIndex = ((((uint)0) << 24) + ((uint)curIdx));
            }

            // mark other entity's scale to 0 =>  invalid instance
            var uselessCount = capacity - totalCount;
            if (uselessCount > 0) {
                fixed (InstanceBound* ptr = &bounds[0]) {
                    UnsafeUtility.MemClear(ptr + offset, sizeof(InstanceBound) * uselessCount);
                }
            }

            OnLayoutChangedEvent?.Invoke();
            isDirty = true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRenderData(RendererData data) {
            RenderData[data.prefabIdx].Add(data);
        }


        [BurstCompile]
        public void ResetLayout(List<int> configs, List<int> counts, bool isInit = false) {
            this._configs = configs;
            this._counts = counts;
            for (int i = RenderData.Count; i < configs.Count; i++) {
                RenderData.Add(new List<RendererData>());
            }

            for (int i = prefabSize.Count; i < configs.Count; i++) {
                prefabSize.Add(float3.zero);
            }

            int offset = 0;
            for (int i = 0; i < configs.Count; i++) {
                offset += counts[i];
            }

            var totalCount = offset;
            if (totalCount > Capacity) {
                if (Capacity == 0) {
                    Capacity = MIN_CAPACITY; // 
                }

                while (Capacity < totalCount) {
                    Capacity *= 2;
                }

                ResetCapacity(Capacity);
            }
        }

        private void ResetCapacity(int capacity) {
            Debug.LogWarning("OnRenderResetCapacity" + capacity);
            Capacity = capacity;
            transformData = new Transform3DForRendering[capacity];

            bounds = new InstanceBound[capacity];
            sortingData = new SortingData[capacity];
            animData = new AnimRenderDataForRendering[capacity];
        }
    }
}