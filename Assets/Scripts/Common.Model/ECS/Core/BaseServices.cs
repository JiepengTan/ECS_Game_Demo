using System;

namespace GamesTan.ECS {
    public unsafe class BaseServices {
        public float DeltaTime;
        public float TimeSinceLevelLoad;
        public int Frame;
        private System.Random Rand;

        public void DoAwake(int seed = 0) {
            Rand = new Random(seed);
        }
        public virtual void DoDestroy() {
        }

        public int RandomRange(int start, int endExclude) {
            return Rand.Next(start, endExclude -1);
        }

        public float RandomValue() {
            return (float)Rand.NextDouble();
        }

    }
}