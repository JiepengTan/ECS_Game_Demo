using System.Collections;
using System.Collections.Generic;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using TItem = GamesTan.ECS.Game.Enemy;
using TItemRef = GamesTan.ECS.Game.EnemyRef;
using uint32_t = System.UInt32;

namespace GamesTan.ECS.Game
{
    public unsafe class NativeEntityPool
    {
        public TItem* _ary = null;
        uint32_t* free_list_chunks = null;
        public int _capacity;
        public int _length;
        public HashSet<int> _freeItems;

        public void Init(int capacity)
        {
            _ary = (TItem*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TItem>() * capacity);
            free_list_chunks = (uint32_t*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<uint32_t>() * capacity);
            _capacity = capacity;
            _length = 0;

            for (uint32_t i = 0; i < capacity; i++)
            {
                // Don't initialize chunk.
                free_list_chunks[i] = i;
            }
        }

        public TItem* Create()
        {
            return &_ary[_length++];
        }

        public void QueueFree(TItem* item)
        {
            _freeItems.Add(item->EntityId);
        }

        public void FlushFreeItems()
        {
        }

        public TItem* GetArray()
        {
            return _ary;
        }
    }
}