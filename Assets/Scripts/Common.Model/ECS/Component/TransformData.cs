using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.ECS {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct TransformData :IComponent{
        public float3 Position;
        public float3 Rotation;
        public float3 Scale;
        
        public override string ToString() {
            return $"pos{Position} rot:{Rotation} scale:{Scale}";
        }
    }
}