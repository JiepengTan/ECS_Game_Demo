
#_ME_FOR #ENTITY 
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct #CLS_NAME :IEntity {
        /// <summary> Entity Data   /// </summary>
        public EntityRef __Data;
        public EntityRef __EntityData {
            get => __Data;
            set => __Data = value;
        }
        public bool IsValid => __Data.Version > 0;

#_ME_FOR #GET_FIELDS
        public #FIELD_TYPE #FIELD_TYPE;
#_ME_ENDFOR 

        /// <summary> GameObject Id   /// </summary>
        public int GObjectId {
            get => BasicData.GObjectId;
            set => BasicData.GObjectId = value;
        }
        
        /// <summary> 是否已经释放   /// </summary>
        public bool IsAlreadyStart {
            get => BasicData.StatusData.Is(0);
            set => BasicData.StatusData.Set(0, value);
        }

        /// <summary> 旋转   /// </summary>
        public float DegY {
            get => TransformData.Rotation.y;
            set => TransformData.Rotation.y = value;
        }

        public float3 Forward {
            get {
                float deg = math.radians(-DegY + 90);
                return new float3(math.cos(deg), 0, math.sin(deg));
            }
        }

        public float2 Forward2 {
            get {
                float deg = math.radians(-DegY + 90);
                return new float2(math.cos(deg), math.sin(deg));
            }
        }
        public float Radius {
            get => PhysicData.Radius;
            set => PhysicData.Radius = value;
        }
        public float3 Pos3 {
            get => TransformData.Position;
            set => TransformData.Position = value;
        }
        public float3 Rot3 {
            get => TransformData.Rotation;
            set => TransformData.Rotation = value;
        }
        public float3 Scale3 {
            get => TransformData.Scale;
            set => TransformData.Scale = value;
        }
        public float Scale {
            get => TransformData.Scale.x;
            set {
                TransformData.Scale = new float3(2, 2, 2) * value;
                PhysicData.Radius = value;
            }
        }

    }
#_ME_ENDFOR 


