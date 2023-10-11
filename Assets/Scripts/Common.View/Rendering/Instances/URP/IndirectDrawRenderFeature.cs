using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    [CreateAssetMenu]
    public class IndirectDrawRenderFeature : ScriptableRendererFeature {
        public IndirectRendererConfig Config;
        public int maxCount = 10000;
        public Mesh mesh;
        public Material mat;
        public RenderPassEvent evt;

        private IndirectDrawRenderPass pass;
        private ComputeBuffer cbDrawArgs;
        private ComputeBuffer cbPoints;
        private int[] args;
        private bool reinit = false;

        public IndirectDrawRenderFeature() {
            reinit = true;
        }

        public override void Create() {
            if (mesh == null) {
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
                GameObject.Destroy(gameObject);
            }

            //args
            if (args == null) {
                args = new int[] {
                    (int)mesh.GetIndexCount(0),
                    1,
                    (int)mesh.GetIndexStart(0),
                    (int)mesh.GetBaseVertex(0),
                    0
                };
            }

            //Create resources
            CleanUp();
            if (cbDrawArgs == null) {
                cbDrawArgs =
                    new ComputeBuffer(1, args.Length * 4, ComputeBufferType.IndirectArguments); //each int is 4 bytes
                cbDrawArgs.SetData(args);
            }

            if (cbPoints == null) {
                cbPoints = new ComputeBuffer(maxCount, 12,
                    ComputeBufferType.Append); //pointBuffer is 3 floats so 3*4bytes = 12, see shader
                mat.SetBuffer("pointBuffer", cbPoints); //Bind the buffer wwith material
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (reinit) {
                reinit = false;
                Create();
            }

            pass = new IndirectDrawRenderPass(evt, maxCount, mesh, mat, cbDrawArgs, cbPoints);
            renderer.EnqueuePass(pass);
        }

        public void CleanUp() {
            //Clean up
            if (cbDrawArgs != null) {
                cbDrawArgs.Release();
                cbDrawArgs = null;
            }

            if (cbPoints != null) {
                cbPoints.Release();
                cbPoints = null;
            }
        }

        public void OnDisable() {
            CleanUp();
            reinit = true;
        }

        //-------------------------------------------------------------------------

    }
}