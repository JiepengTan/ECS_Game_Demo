using Unity.Mathematics;
using UnityEngine;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestEnemyUpdateAnimation : BaseGameSystem {
        public override void Update(float dt) {
            var pool = World.EnemyPool;
            var count = pool.MaxUsedSlot;
            var ptrAry = pool.GetData();
            for (int i = 0; i < count; i++) {
                ref var item = ref ptrAry[i];
                if (item.IsValid) {
                    ref var internalData = ref item.AnimInternalData;
                    internalData.Timer += dt;
                    internalData.LerpTimer += dt;
                    for (int idx = 0; idx < 4; idx++) {
                        if (idx == 0 && internalData.Timer[idx] > 3) {
                            internalData.Timer[idx] = 0;
                            internalData.LerpTimer[idx] = 0;
                            internalData.AnimId2[idx] = internalData.AnimId1[idx];
                            internalData.AnimId1[idx] = Services.RandomRange(0, 3);
                        }
                    }
                    ref var animInfo = ref item.AnimInfo;
                    //TODO 处理其他的动画
                    animInfo.AnimInfo0 = new float4(1, 0, AnimationUtil.GetAnimation(item.PrefabId,internalData.AnimId1[0], internalData.Timer[0]),0);
                    animInfo.AnimInfo1 = float4.zero;
                    animInfo.AnimInfo2 = float4.zero;
                    animInfo.AnimInfo3 = float4.zero;
                    //Debug.Log(animInfo.AnimInfo0.ToString());
                }
            }
        }
    }
}