﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestCreateEnemy : BaseGameSystem {
        private List<EntityData> _testUnits => Services._testUnits;

        public override void Update(float dt) {
            if(Services.FreshCountPerFrame ==0) return;
            int count = Services.RandomRange(0, Services.FreshCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (!isDelete) {
                    var entity = World.CreateEnemy();
                    _testUnits.Add(entity);
                    if (Services.IsShowLog) Debug.Log("CreateOne " + EnemyPool.Count + entity + "  \n" + EnemyPool.ToString());
                    if (Services.IsCreateGo) {
                        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        var view = obj.AddComponent<DebugTestEntityView>();
                        view.Entity = entity;
                        view.World = World;
                        obj.name = $"{Services.CurGoId++}_UnitID_{entity.SlotId}";
                        obj.transform.SetParent(Services.transform);
                        World.GetEnemy(entity)->GObjectId = obj.GetInstanceID();
                        Services._id2Go.Add(obj.GetInstanceID(), obj);
                    }
                }
            }
        }
    }

    public unsafe partial class SysTestDestroyEnemy : BaseGameSystem {
        private List<EntityData> _testUnits => Services._testUnits;

        public override void Update(float dt) {
            if(Services.FreshCountPerFrame ==0) return;
            int count = Services.RandomRange(0, Services.FreshCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (isDelete && _testUnits.Count > 0) {
                    var idx = Services.RandomRange(0, _testUnits.Count);
                    var unit = _testUnits[idx];
                    var ptr = World.GetEnemy(unit);
                    if (Services.IsCreateGo) {
                        if (Services._id2Go.TryGetValue(ptr->GObjectId, out var go)) {
                            GameObject.Destroy(go);
                            Services._id2Go.Remove(ptr->GObjectId);
                        }
                    }
                    World.DestroyEnemy(unit);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                    if (Services.IsShowLog) Debug.Log("DestroyOne " + EnemyPool.Count + unit + "  \n" + EnemyPool.ToString());
                }
            }

            Services.CurEnemyCount = World.EnemyPool.Count;
        }
    }
}