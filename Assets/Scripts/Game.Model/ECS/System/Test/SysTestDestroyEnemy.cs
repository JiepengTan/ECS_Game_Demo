using System.Collections.Generic;

namespace GamesTan.ECS.Game {
    public unsafe partial class SysTestDestroyEnemy : BaseGameSystem {
        private List<EntityRef> _testUnits => Services.AllUnits;

        public override void Update(float dt) {
            if (Services.DebugOnlyOneEntity) return;
            if (Services.DeleteOrNewCountPerFrame == 0) return;
            int count = Services.RandomRange(0, Services.DeleteOrNewCountPerFrame);
            for (int i = 0; i < count; i++) {
                bool isDelete = Services.RandomValue() < Services.DeleteProbability;
                if (isDelete && _testUnits.Count > 0) {
                    var idx = Services.RandomRange(0, _testUnits.Count);
                    var unit = _testUnits[idx];
                    World.DestroyEnemy(unit);
                    _testUnits[idx] = _testUnits[_testUnits.Count - 1];
                    _testUnits.RemoveAt(_testUnits.Count - 1);
                }
            }
            Services.CurEnemyCount = EntityManager.EnemyPool.Count;
        }
    }
}