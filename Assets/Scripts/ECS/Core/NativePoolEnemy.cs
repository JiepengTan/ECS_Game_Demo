using System;
using System.Collections;
using System.Collections.Generic;
using Lockstep.InternalUnsafeECS;
using Unity.Mathematics;
using UnityEngine;
using TItem = GamesTan.ECS.Game.Enemy;


namespace GamesTan.ECS.Game
{
    public unsafe class NativePoolEnemy
    {
        public TItem* _ary = null;
        EntityData* _freeList = null;
        public int _capacity;
        public int _length;

        public void Init(int capacity)
        {
            _ary = (TItem*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<TItem>() * capacity);
            _freeList = (EntityData*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<EntityData>() * capacity);
            _capacity = capacity;
            _length = 0;

            for (int i = 0; i < capacity; i++)
            {
                // Don't initialize chunk.
                _freeList[i] = new EntityData(i,0);
            }
        }

        public EntityData Alloc()
        {
            return _freeList[_length++];
        }

        public void QueueFree(EntityData item)
        {
            TItem* ptr = GetData(item);
            if(ptr == null) return;
            if(ptr->IsMemFree) return;
            ptr->IsMemFree = true;
        }

        public void FlushFreeItems()
        {
            
        }
        public TItem* GetData(EntityData entity)
        {
            return _ary;
        }
        public TItem* GetData()
        {
            return _ary;
        }
    }
}