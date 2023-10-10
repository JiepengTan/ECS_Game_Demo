// https://github.com/JiepengTan  

namespace GamesTan.UnsafeECSDefine {

    /// BuildIn Service 
    public interface IBuildInService{}
    /// 需要进行Service 属性展开的类型
    public interface INeedServiceProperty{}
    public interface ICanRaiseEvent{}
    public interface IBuildInComponent : IComponent { }


    public class GameDefine
    {
        public const int PackSize = 4;
        public static System.Type[] AllBuildInComponentTypes = new[] {
            typeof(Transform2D),
            typeof(NavMeshAgent),
            typeof(CollisionAgent),
            typeof(Prefab),
            typeof(Animator),
        };
    }

    public class CollisionShape { }


    public class Animator : IBuildInComponent {
        public int Pad;
    }

    public class CollisionAgent : IBuildInComponent {
        public CollisionShape Collider;
        public bool IsTrigger;
        public int Layer;
        public bool IsEnable;
         public bool IsSleep;
        public float Mass;
        public float AngularSpeed;
        public Vector3 Speed;
    }

    public class NavMeshAgent : IBuildInComponent { 
        public int Pad;
    }

    public class Prefab : IBuildInComponent { 
        public int AssetId;
    }

    public class Transform2D : IBuildInComponent { 
        public Vector2 Position;
        public float Deg;
        public float Scale;
        
    }

    public class Transform3D : IBuildInComponent {
        public Vector3 Position;
        public Vector3 Forward;
        public float Scale;
    }
}