using GamesTan.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe static class EntityUtil {
        public static EntityData CreateEnemy(this GameEcsWorld world) {
            var services = world.Services;
            var entity = world.AllocEnemy();
            var entityPtr = world.GetEnemy(entity);
            entityPtr->Scale = new float3(1, 1, 1);
            entityPtr->PrefabId = services.RandomValue() > 0.3 ? 10001 : 10003;
            entityPtr->InstancePrefabIdx = RenderWorld.Instance.GetInstancePrefabIdx(entityPtr->PrefabId);
            if (services.IsCreateView) {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var view = obj.AddComponent<EntityViewDebugTest>();
                view.Entity = entity;
                view.World = world;
                obj.name = $"{services.GlobalViewId++}_UnitID_{entity.SlotId}_PrefabID{world.GetEnemy(entity)->PrefabId}";
                obj.transform.SetParent(services.ViewRoot);
                entityPtr->GObjectId = obj.GetInstanceID();
                services.Id2View.Add(obj.GetInstanceID(), obj);
            }

            return entityPtr->__Data;
        }

        public static void DestroyEnemy(this GameEcsWorld World, EntityData unit) {
            var ptr = World.GetEnemy(unit);
            var Services = World.Services;
            if (Services.IsCreateView) {
                if (Services.Id2View.TryGetValue(ptr->GObjectId, out var go)) {
                    GameObject.Destroy(go);
                    Services.Id2View.Remove(ptr->GObjectId);
                    
                }
            }

            World.FreeEnemy(unit);
        }
    }
}