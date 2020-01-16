using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class PaddleMeshCreator : MonoBehaviour
{
    public MeshRenderer tempRender;

    private bool m_IsPressFire = false;

    private Mesh m_Mesh;
    private List<Vector3> m_Verticles = new List<Vector3>();
    private List<int> m_Triangles = new List<int>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private List<Color> m_Colors = new List<Color>();

    private Vector3 m_LastPress = Vector3.zero;
    private bool m_AddOrgrinal = false;
    private MeshRenderer m_MeshRender;
    private MeshFilter m_MeshFilter;

    private List<Vector3> m_TempVerticles = new List<Vector3>();
    private List<int> m_TempTriangles = new List<int>();
    private List<Vector2> m_Tempuvs = new List<Vector2>();
    private List<Color> m_TempColors = new List<Color>();

    private Material m_BlurMaterial;
    private CommandBuffer m_CommandBuffer;
    private RenderTexture _opaqueTexture;

    private RenderTexture _tempTexture;

    //private RenderTexture _opaqueTexture;

    public RawImage rawImage;

    private Material m_MainMaterial;

    static class ShaderProperties
    {
        public static readonly int TempRenderTexture = Shader.PropertyToID("_Temp");
        public static readonly int BlurRenderTexture = Shader.PropertyToID("_BlurTex");
        public static readonly int MainRenderTexture = Shader.PropertyToID("_MainTex");


    }

    private void Awake()
    {
        m_CommandBuffer = new CommandBuffer();
    }

    private void Update()
    {
        AttempCreatePaddleMesh();
    }

    private void AttempCreatePaddleMesh()
    {
        if (Time.timeScale < float.Epsilon)
            return;


        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            m_IsPressFire = true;

            if (m_Mesh == null)
            {
                GameObject go = new GameObject("Paddle");
                m_MeshFilter = go.AddComponent<MeshFilter>();
                m_MeshRender = go.AddComponent<MeshRenderer>();
                m_Mesh = new Mesh();
                m_MeshFilter.mesh = m_Mesh;

                Shader paddleEffectShader = Shader.Find("younne/PaddleEffect");
                m_MainMaterial = new Material(paddleEffectShader);
                m_MainMaterial.hideFlags = HideFlags.HideAndDontSave;
                m_MeshRender.sharedMaterial = m_MainMaterial;

                Shader shader = Shader.Find("younne/BlurEffect");
                m_BlurMaterial = new Material(shader);
                m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;

                UpdateCameraTarget();

            }
            else
            {
                m_TempVerticles.Clear();
                m_TempTriangles.Clear();
                m_Tempuvs.Clear();
                m_TempColors.Clear();
            }
        }

        if (CrossPlatformInputManager.GetButtonUp("Fire1"))
        {
            m_IsPressFire = false;
            m_AddOrgrinal = false;
        }
        

        if (m_IsPressFire)
        {
            var mousePosition = CrossPlatformInputManager.mousePosition;
            var vertextOffset = 3;
            if (m_AddOrgrinal)
            {
                Vector3 dir = mousePosition - m_LastPress;
                Vector3 crossVec3 = Quaternion.Euler(0, 0, 90) * dir;

                vertextOffset = m_TempVerticles.Count;

                Vector3 v1 = m_LastPress + crossVec3;
                Vector3 v2 = m_LastPress - crossVec3;

                m_TempVerticles.Add(v1);
                float uvx = v1.x / Screen.width;
                float uvy = v1.y / Screen.height;
                m_Tempuvs.Add(new Vector2(uvx, uvy));

                Debug.LogErrorFormat("uv1.x = {0}, uv1.y = {1}", uvx, uvy);

                m_TempVerticles.Add(v2);
                uvx = v2.x / Screen.width;
                uvy = v2.y / Screen.height;
                m_Tempuvs.Add(new Vector2(uvx, uvy));

                Debug.LogErrorFormat("uv2.x = {0}, uv2.y = {1}", uvx, uvy);

                if (vertextOffset == 0)
                {
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset + 1);
                }
                else if(vertextOffset == 2)
                {
                    //vertextOffset = 2;
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset - 1);
                    m_TempTriangles.Add(vertextOffset + 1);
                }
                else
                {
                    //4
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset - 2);
                    m_TempTriangles.Add(vertextOffset - 1);
                    m_TempTriangles.Add(vertextOffset - 1);
                    m_TempTriangles.Add(vertextOffset + 1);
                    m_TempTriangles.Add(vertextOffset + 0);
                }

            }

            if (m_AddOrgrinal == false)
            {
                m_AddOrgrinal = true;
            }

            m_LastPress = mousePosition;

            if (vertextOffset <= 2) return;

            StartMeshUpdate();
            FinishMeshUpdate();
            UpdateCommandBuffers();
        }
    }

    private Vector3 ConvertScreenToWorldPoint(Vector3 mousePosition)
    {
        return Camera.main.ScreenToWorldPoint(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
    }

    public void StartMeshUpdate()
    {
        m_Verticles.Clear();
        m_Triangles.Clear();
        m_uvs.Clear();
        m_Colors.Clear();
    }

    public void FinishMeshUpdate()
    {
        m_Verticles.AddRange(m_TempVerticles);
        m_Triangles.AddRange(m_TempTriangles);
        m_uvs.AddRange(m_Tempuvs);
        m_Colors.AddRange(m_TempColors);

        m_MeshFilter.mesh.Clear();
        m_MeshFilter.mesh.SetVertices(m_Verticles);
        m_MeshFilter.mesh.SetTriangles(m_Triangles, 0);
        m_MeshFilter.mesh.SetUVs(0, m_uvs);
        m_MeshFilter.mesh.SetColors(m_Colors);
    }


    void RemoveCommandBuffers()
    {
        Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);
    }

    void AddCommandBuffers()
    {
        m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, _tempTexture);
        m_CommandBuffer.Blit(_tempTexture, _opaqueTexture, m_BlurMaterial, 0);
        rawImage.texture = _opaqueTexture;

        //m_CommandBuffer.SetGlobalTexture(ShaderProperties.BlurRenderTexture, _opaqueTexture);
        m_MeshRender.sharedMaterial.SetTexture(ShaderProperties.BlurRenderTexture, _opaqueTexture);

        Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);

        tempRender.sharedMaterial.SetTexture(ShaderProperties.BlurRenderTexture, _opaqueTexture);
    }


    public void UpdateCameraTarget()
    {
        Camera.main.targetTexture = null;
        if (_opaqueTexture != null)
        {
            _opaqueTexture.Release();
        }

        _opaqueTexture = new RenderTexture(Screen.width, Screen.height,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _opaqueTexture.name = nameof(_opaqueTexture);
        _opaqueTexture.antiAliasing = 1;
        _opaqueTexture.Create();

        if (_tempTexture != null)
        {
            _tempTexture.Release();
        }

        _tempTexture = new RenderTexture(Screen.width, Screen.height,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _tempTexture.name = nameof(_opaqueTexture);
        _tempTexture.antiAliasing = 1;
        _tempTexture.Create();

        UpdateCommandBuffers();
    }

    private void UpdateCommandBuffers()
    {
        RemoveCommandBuffers();
        AddCommandBuffers();
    }
}
