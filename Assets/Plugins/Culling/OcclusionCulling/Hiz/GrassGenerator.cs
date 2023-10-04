using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassGenerator : MonoBehaviour
{
    public Shader depthTextureShader; //��������mipmap��shader

    RenderTexture m_depthTexture; //�� mipmap �����ͼ
    public RenderTexture depthTexture => m_depthTexture;

    int m_depthTextureSize = 0;

    public int depthTextureSize
    {
        get
        {
            if (m_depthTextureSize == 0)
                m_depthTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(Screen.width, Screen.height));
            return m_depthTextureSize;
        }
    }

    Material m_depthTextureMaterial;
    const RenderTextureFormat m_depthTextureFormat = RenderTextureFormat.RHalf; //���ȡֵ��Χ0-1����ͨ�����ɡ�

    CommandBuffer m_calulateDepthCommandBuffer;


    void InitDepthTexture()
    {
        if (m_depthTexture != null) return;
        m_depthTexture = new RenderTexture(depthTextureSize, depthTextureSize, 0, m_depthTextureFormat);
        m_depthTexture.autoGenerateMips = false;
        m_depthTexture.useMipMap = true;
        m_depthTexture.filterMode = FilterMode.Point;
        m_depthTexture.Create();
    }

    //����mipmap
    void DrawHiz()
    {
        m_calulateDepthCommandBuffer.Clear();
        int w = m_depthTexture.width;
        int mipmapLevel = 0;

        RenderTexture currentRenderTexture = null; //��ǰmipmapLevel��Ӧ��mipmap
        RenderTexture preRenderTexture = null; //��һ���mipmap����mipmapLevel-1��Ӧ��mipmap

        //�����ǰ��mipmap�Ŀ�ߴ���8���������һ���mipmap
        while (w > 8)
        {
            currentRenderTexture = RenderTexture.GetTemporary(w, w, 0, m_depthTextureFormat);
            currentRenderTexture.filterMode = FilterMode.Point;
            if (preRenderTexture == null)
            {
                //Mipmap[0]��copyԭʼ�����ͼ
                m_calulateDepthCommandBuffer.Blit(null, currentRenderTexture, m_depthTextureMaterial, 1);
            }
            else
            {
                //��Mipmap[i] Blit��Mipmap[i+1]��
                m_calulateDepthCommandBuffer.Blit(preRenderTexture, currentRenderTexture, m_depthTextureMaterial, 0);
                RenderTexture.ReleaseTemporary(preRenderTexture);
            }

            m_calulateDepthCommandBuffer.CopyTexture(currentRenderTexture, 0, 0, m_depthTexture, 0, mipmapLevel);
            preRenderTexture = currentRenderTexture;

            w /= 2;
            mipmapLevel++;
        }

        RenderTexture.ReleaseTemporary(preRenderTexture);
    }


    public Mesh grassMesh;
    public int subMeshIndex = 0;
    public Material grassMaterial;
    public int GrassCountPerRaw = 300; //每行草的数量
    public ComputeShader compute; //剔除的ComputeShader
    private Texture2D whiteTexture;

    int m_grassCount;
    int kernel;
    Camera mainCamera;
    Light mainLight;

    ComputeBuffer argsBuffer;
    ComputeBuffer grassMatrixBuffer; //所有草的世界坐标矩阵
    ComputeBuffer cullResultBuffer; //剔除后的结果
    ComputeBuffer cullResultCount; //剔除后的数量

    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    int cullResultBufferId, vpMatrixId, positionBufferId, hizTextureId, shadowMapTextureId;
    int shadowCasterPassIndex;

    CommandBuffer m_computeShaderCommandBuffer, m_collectShadowCommandBuffer, m_drawGrassCommandBuffer;
    MaterialPropertyBlock m_materialBlock;

    //CommandBuffer b;//单颗草绘制测试

    void Start()
    {
        m_grassCount = GrassCountPerRaw * GrassCountPerRaw;
        mainCamera = Camera.main;
        mainLight = GameObject.Find("Directional Light").GetComponent<Light>();

        if (grassMesh != null)
        {
            args[0] = grassMesh.GetIndexCount(subMeshIndex);
            args[2] = grassMesh.GetIndexStart(subMeshIndex);
            args[3] = grassMesh.GetBaseVertex(subMeshIndex);
        }
        else
            args[0] = args[1] = args[2] = args[3] = 0;

        m_depthTextureMaterial = new Material(depthTextureShader);
        mainCamera.depthTextureMode |= DepthTextureMode.Depth;


        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        InitDepthTexture();
        InitComputeBuffer();
        InitGrassPosition();
        InitComputeShader();
        AddCommandBuffer();
    }

    void InitComputeShader()
    {
        kernel = compute.FindKernel("GrassCulling");
        compute.SetInt("grassCount", m_grassCount);
        compute.SetInt("depthTextureSize", depthTextureSize);
        compute.SetBool("isOpenGL",
            Camera.main.projectionMatrix.Equals(GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false)));
        compute.SetBuffer(kernel, "grassMatrixBuffer", grassMatrixBuffer);

        cullResultBufferId = Shader.PropertyToID("cullResultBuffer");
        vpMatrixId = Shader.PropertyToID("vpMatrix");
        hizTextureId = Shader.PropertyToID("hizTexture");
        positionBufferId = Shader.PropertyToID("positionBuffer");
        shadowMapTextureId = Shader.PropertyToID("_ShadowMapTexture");
        shadowCasterPassIndex = grassMaterial.FindPass("ShadowCaster");
    }

    void InitComputeBuffer()
    {
        if (grassMatrixBuffer != null) return;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        grassMatrixBuffer = new ComputeBuffer(m_grassCount, sizeof(float) * 16);
        cullResultBuffer = new ComputeBuffer(m_grassCount, sizeof(float) * 16, ComputeBufferType.Append);
        cullResultCount = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    void AddCommandBuffer()
    {
        m_computeShaderCommandBuffer = new CommandBuffer() { name = "Dispatch Compute" };
        mainCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture, m_computeShaderCommandBuffer);
        m_calulateDepthCommandBuffer = new CommandBuffer() { name = "Calculate DepthTexture" };
        mainCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture, m_calulateDepthCommandBuffer);


        m_collectShadowCommandBuffer = new CommandBuffer() { name = "Collect Shadow" };
        mainLight.AddCommandBuffer(LightEvent.AfterShadowMapPass, m_collectShadowCommandBuffer);

        m_drawGrassCommandBuffer = new CommandBuffer() { name = "Draw Grass" };
        mainCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_drawGrassCommandBuffer);

        m_materialBlock = new MaterialPropertyBlock();
    }

    void OnPreRender()
    {
        //掉帧处理
        //for (int i = 0; i < 200000; i++)
        //{
        //    string s = i.ToString() + i;
        //}

        //AfterDepthTexture：执行compute shader
        m_computeShaderCommandBuffer.Clear();
        m_computeShaderCommandBuffer.SetComputeMatrixParam(compute, vpMatrixId,
            GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false) * mainCamera.worldToCameraMatrix);
        m_computeShaderCommandBuffer.SetBufferCounterValue(cullResultBuffer, 0);
        m_computeShaderCommandBuffer.SetComputeBufferParam(compute, kernel, cullResultBufferId, cullResultBuffer);
        m_computeShaderCommandBuffer.SetComputeTextureParam(compute, kernel, hizTextureId, depthTexture);
        m_computeShaderCommandBuffer.DispatchCompute(compute, kernel, 1 + m_grassCount / 640, 1, 1);
        m_computeShaderCommandBuffer.CopyCounterValue(cullResultBuffer, argsBuffer, sizeof(uint));

        //AfterShadowMapPass：执行处理阴影
        m_materialBlock.Clear();
        m_materialBlock.SetBuffer(positionBufferId, cullResultBuffer);
        m_collectShadowCommandBuffer.Clear();
        m_collectShadowCommandBuffer.DrawMeshInstancedIndirect(grassMesh, subMeshIndex, grassMaterial,
            shadowCasterPassIndex, argsBuffer, 0, m_materialBlock);

        //AfterForwardOpaque：画草
        m_drawGrassCommandBuffer.Clear();
        //解决草偏暗的问题
        m_drawGrassCommandBuffer.EnableShaderKeyword("LIGHTPROBE_SH");
        m_drawGrassCommandBuffer.EnableShaderKeyword("SHADOWS_SCREEN");
        m_materialBlock.SetTexture(shadowMapTextureId, whiteTexture);
        m_drawGrassCommandBuffer.DrawMeshInstancedIndirect(grassMesh, subMeshIndex, grassMaterial, 0, argsBuffer, 0,
            m_materialBlock);

        //b.Clear();
        //b.EnableShaderKeyword("LIGHTPROBE_SH");
        //b.EnableShaderKeyword("SHADOWS_SCREEN");
        //b.DrawMesh(grassMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), grassMaterial, 0, 0);
        DrawHiz();
    }

    //void Update()
    //{
    //    Graphics.DrawMesh(grassMesh, Matrix4x4.TRS(new Vector3(1, 0, 0), Quaternion.identity, Vector3.one), grassMaterial, 0);
    //}

    //获取每个草的世界坐标矩阵
    void InitGrassPosition()
    {
        const int padding = 2;
        int width = (100 - padding * 2);
        int widthStart = -width / 2;
        float step = (float)width / GrassCountPerRaw;
        Matrix4x4[] grassMatrixs = new Matrix4x4[m_grassCount];
        for (int i = 0; i < GrassCountPerRaw; i++)
        {
            for (int j = 0; j < GrassCountPerRaw; j++)
            {
                Vector2 xz = new Vector2(widthStart + step * i, widthStart + step * j);
                Vector3 position = new Vector3(xz.x, GetGroundHeight(xz), xz.y);
                grassMatrixs[i * GrassCountPerRaw + j] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            }
        }

        grassMatrixBuffer.SetData(grassMatrixs);
    }

    //通过Raycast计算草的高度
    float GetGroundHeight(Vector2 xz)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(xz.x, 10, xz.y), Vector3.down, out hit, 20))
        {
            return 10 - hit.distance;
        }

        return 0;
    }

    void OnDisable()
    {
        grassMatrixBuffer?.Release();
        grassMatrixBuffer = null;

        cullResultBuffer?.Release();
        cullResultBuffer = null;

        cullResultCount?.Release();
        cullResultCount = null;

        argsBuffer?.Release();
        argsBuffer = null;

        mainCamera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, m_computeShaderCommandBuffer);
        mainCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_drawGrassCommandBuffer);

        m_depthTexture?.Release();
        m_depthTexture = null;
        mainCamera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, m_calulateDepthCommandBuffer);
        if (mainLight != null)
            mainLight.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, m_collectShadowCommandBuffer);
    }
}