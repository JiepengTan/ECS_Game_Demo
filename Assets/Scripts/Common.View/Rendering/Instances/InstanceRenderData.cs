using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GamesTan.ECS;
using GamesTan.ECS.Game;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public struct RendererData {
    public int prefabIdx;
    public float3 pos;
    public float3 rot;
    public float3 scale;
    public AnimRenderData anim;

    public RendererData(int prefabIdx, float3 pos, float3 rot, float3 scale,  AnimRenderData anim) {
        this.prefabIdx = prefabIdx;
        this.rot = rot;
        this.pos = pos;
        this.scale = scale;
        this.anim = anim;
    }
}




public unsafe class InstanceRenderData {
    public const int MIN_CAPACITY = 4096; // 最小大小为4096
    public int Capacity;
    public int Count;
    
    public List<float3> prefabSize = new List<float3>();
    public List<List<RendererData>> RenderData = new List<List<RendererData>>();
    private List<int> _configs = new List<int>();
    private List<int> _counts = new List<int>();
    
    public List<int> InstanceCounter = new List<int>();

    public bool isDirty;

    public Vector3[] positions;
    public Vector3[] rotations;
    public Vector3[] scales;

    public InstanceBound[] bounds;

    public SortingData[] sortingData;
    
    public AnimRenderData[] animData;


    public Action OnLayoutChangedEvent;

    public void OnFrameStart() {
        foreach (var info in RenderData) {
            info.Clear();
        }
    }

    [BurstCompile]
    public void OnFrameEnd() {
        var totalCount =0;
        for (int i = 0; i < RenderData.Count; i++) {
            var info = RenderData[i];
            totalCount += info.Count;
            _counts[i] = info.Count;
        }
        ResetLayout(_configs,_counts);

        int offset = 0;
        for (int prefabIdx = 0; prefabIdx < RenderData.Count; prefabIdx++) {
            var info = RenderData[prefabIdx];
            var count = info.Count;
            for (int instanceIdx = 0; instanceIdx < count; instanceIdx++) {
                var item = info[instanceIdx];
                var curIdx = offset + instanceIdx;
                positions[curIdx] = item.pos;
                rotations[curIdx] = item.rot;
                scales[curIdx] = item.scale;
                
                InstanceBound bound = new InstanceBound();
                bound.boundsCenter = item.pos;
                bound.boundsExtents = item.scale * prefabSize[item.prefabIdx];// TODO correct bound size
                bounds[curIdx] = bound;
                sortingData[curIdx].drawCallInstanceIndex =(((uint)prefabIdx  << 16) + ((uint)instanceIdx));
                animData[curIdx] = item.anim;
            }
            offset+= info.Count;
        }
        // TODO make sure the sortingdata is correct
        for (int curIdx = totalCount; curIdx < rotations.Length; curIdx++) {
            sortingData[curIdx].drawCallInstanceIndex =((((uint)0 ) << 16) + ((uint)curIdx));
        }
        // mark other entity's scale to 0 =>  invalid instance
        var uselessCount = rotations.Length - totalCount;
        if (uselessCount > 0) {
            fixed (InstanceBound* ptr = &bounds[0]) {
                UnsafeUtility.MemClear(ptr+offset,sizeof(InstanceBound) * uselessCount);
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
    public void ResetLayout(List<int> configs, List<int> counts,bool isInit = false) {
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
                Capacity = MIN_CAPACITY;// 
            }
            while (Capacity<totalCount) {
                Capacity *=2;
            }
            ResetCapacity(Capacity);
        }
    }

    private void ResetCapacity(int capacity) {
        Debug.LogWarning("OnRenderResetCapacity" + capacity);
        Capacity = capacity;
        positions = new Vector3[capacity];
        scales = new Vector3[capacity];
        rotations = new Vector3[capacity];

        bounds = new InstanceBound[capacity];
        sortingData = new SortingData[capacity];
        animData = new AnimRenderData[capacity];
        
    }
}