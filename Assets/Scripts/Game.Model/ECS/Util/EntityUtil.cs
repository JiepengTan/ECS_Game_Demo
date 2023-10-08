using GamesTan.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe static class EntityUtil {
        public static EntityData CreateBullet(this GameEcsWorld world) {
            var services = world.Services;
            var entityMgr = world.EntityManager;
            var entity = entityMgr.AllocEnemy();
            var entityPtr = entityMgr.GetEnemy(entity);
            entityPtr->TransformData.Scale = new float3(1, 1, 1);
            entityPtr->AssetData.PrefabId = services.RandomValue() > 0.3 ? 10001 : 10003;
            entityPtr->AssetData.InstancePrefabIdx = RenderWorld.Instance.GetInstancePrefabIdx(entityPtr->AssetData.PrefabId);
            if (services.IsCreateView) {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var view = obj.AddComponent<EntityViewDebugTest>();
                view.Entity = entity;
                view.World = world;
                obj.name = $"{services.GlobalViewId++}_UnitID_{entity.SlotId}_PrefabID{entityMgr.GetEnemy(entity)->AssetData.PrefabId}";
                obj.transform.SetParent(services.ViewRoot);
                entityPtr->BasicData.GObjectId = obj.GetInstanceID();
                services.Id2View.Add(obj.GetInstanceID(), obj);
            }

            return entityPtr->__Data;
        }
        public static EntityData CreateEnemy(this GameEcsWorld world) {
            var services = world.Services;
            var entityMgr = world.EntityManager;
            var entity = entityMgr.AllocEnemy();
            var entityPtr = entityMgr.GetEnemy(entity);
            entityPtr->TransformData.Scale = new float3(1, 1, 1);
            entityPtr->AssetData.PrefabId = services.RandomValue() > 0.3 ? 10001 : 10003;
            entityPtr->AssetData.InstancePrefabIdx = RenderWorld.Instance.GetInstancePrefabIdx(entityPtr->AssetData.PrefabId);
            if (services.IsCreateView) {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var view = obj.AddComponent<EntityViewDebugTest>();
                view.Entity = entity;
                view.World = world;
                obj.name = $"{services.GlobalViewId++}_UnitID_{entity.SlotId}_PrefabID{entityMgr.GetEnemy(entity)->AssetData.PrefabId}";
                obj.transform.SetParent(services.ViewRoot);
                entityPtr->GObjectId = obj.GetInstanceID();
                services.Id2View.Add(obj.GetInstanceID(), obj);
            }

            return entityPtr->__Data;
        }

        public static void DestroyEnemy(this GameEcsWorld world, EntityData unit) {
            var entityMgr = world.EntityManager;
            var services = world.Services;
            var ptr = entityMgr.GetEnemy(unit);
            world.WorldRegion.RemoveEntity(unit,ptr->PhysicData.GridCoord);
            if (services.IsCreateView) {
                if (services.Id2View.TryGetValue(ptr->GObjectId, out var go)) {
                    GameObject.Destroy(go);
                    services.Id2View.Remove(ptr->GObjectId);
                    
                }
            }

            entityMgr.FreeEnemy(unit);
        }
    }
}