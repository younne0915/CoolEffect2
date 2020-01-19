using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class PaddleMeshCreator : MonoBehaviour
{
    [Range(2, 30)]
    [SerializeField]
    private float boost = 10;

    [Range(0, 0.02f)]
    [SerializeField]
    private float dragXThreshold = 0.008f;

    [Range(0, 0.02f)]
    [SerializeField]
    private float dragYThreshold = 0.006f;

    [SerializeField]
    private Camera grabCamera;

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

    public RawImage rawImage;

    private Material m_MainMaterial;

    private Transform m_Paddle;
    private bool m_StartRoken = false;

    private float m_LeftTime = 10;
    private Vector2 m_DirNormal;
    private float m_PassRemoveTime = 0;

    static class ShaderProperties
    {
        public static readonly int TempRenderTexture = Shader.PropertyToID("_Temp");
        public static readonly int BlurRenderTexture = Shader.PropertyToID("_BlurTex");
        public static readonly int MainRenderTexture = Shader.PropertyToID("_MainTex");
        public static readonly int DirVec4 = Shader.PropertyToID("dirVec2");
    }

    private void Awake()
    {
        m_CommandBuffer = new CommandBuffer();
        grabCamera.enabled = false;
    }

    private void Update()
    {
        AttempCreatePaddleMesh();
        AttempStartReckon();
    }

    private void SetRender()
    {
        StartMeshUpdate();
        FinishMeshUpdate();
    }

    private void AttempStartReckon()
    {
        if (m_StartRoken)
        {
            Debug.LogError("111");

            m_LeftTime -= Time.time;
            if(m_LeftTime < 0)
            {
                m_TempVerticles.Clear();
                m_Tempuvs.Clear();
                m_TempTriangles.Clear();

                m_StartRoken = false;

                SetRender();

                m_LeftTime = 10;

                Debug.LogError("222");

            }
            else
            {

                int count = m_TempVerticles.Count;

                Debug.LogError("count = " + count);

                if (count >= 2)
                {
                    m_TempVerticles.RemoveRange(count - 2, 2);
                    m_Tempuvs.RemoveRange(count - 2, 2);

                    int vertextOffset = m_TempVerticles.Count;
                    if (vertextOffset <= 2)
                    {
                        m_TempVerticles.Clear();
                        m_Tempuvs.Clear();
                        m_TempTriangles.Clear();
                    }
                    else
                    {
                        int triangleCount = m_TempTriangles.Count;
                        m_TempTriangles.RemoveRange(triangleCount - 6, 6);

                        Debug.LogError("aaa");
                    }
                }
                SetRender();
            }
        }
    }

    private void AttempCreatePaddleMesh()
    {
        if (Time.timeScale < float.Epsilon)
            return;


        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            m_IsPressFire = true;
            grabCamera.enabled = true;

            if (m_Mesh == null)
            {
                GameObject go = new GameObject("Paddle");
                m_Paddle = go.transform;
                m_MeshFilter = go.AddComponent<MeshFilter>();
                m_MeshRender = go.AddComponent<MeshRenderer>();
                go.transform.position = transform.position;
                go.layer = LayerMask.NameToLayer("Paddle");

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

            grabCamera.enabled = false;
            m_StartRoken = true;

            float cnt = m_TempVerticles.Count / 2;
        }


        if (m_IsPressFire)
        {
            var mousePosition = CrossPlatformInputManager.mousePosition;
            var vertextOffset = 3;
            if (m_AddOrgrinal)
            {
                float randomBeta = Random.Range(0.2f, 1);
                m_DirNormal = (mousePosition - m_LastPress).normalized;
                Vector3 dir = m_DirNormal * boost * randomBeta;
                Vector3 crossVec3 = Quaternion.Euler(0, 0, 90) * dir;

                vertextOffset = m_TempVerticles.Count;

                Vector3 v1 = m_LastPress + crossVec3;
                Vector3 v2 = m_LastPress - crossVec3;

                Vector2 dragDir = new Vector2(m_DirNormal.x * dragXThreshold, m_DirNormal.y * dragYThreshold);

                float uvx = v1.x / Screen.width;
                float uvy = v1.y / Screen.height;
                Vector2 uvFinal = new Vector2(uvx, uvy) - dragDir;
                m_Tempuvs.Add(uvFinal);

                uvx = v2.x / Screen.width;
                uvy = v2.y / Screen.height;
                Vector2 uvFinal2 = new Vector2(uvx, uvy) - dragDir;
                m_Tempuvs.Add(uvFinal2);

                v1 = ConvertScreenToPaddleSpace(v1);
                m_TempVerticles.Add(v1);
                v2 = ConvertScreenToPaddleSpace(v2);
                m_TempVerticles.Add(v2);

                //if (vertextOffset == 0)
                //{
                //    m_TempTriangles.Add(vertextOffset + 0);
                //    m_TempTriangles.Add(vertextOffset + 1);
                //}
                //else if(vertextOffset == 2)
                //{
                //    //vertextOffset = 2;
                //    m_TempTriangles.Add(vertextOffset + 0);
                //    m_TempTriangles.Add(vertextOffset + 0);
                //    m_TempTriangles.Add(vertextOffset - 1);
                //    m_TempTriangles.Add(vertextOffset + 1);
                //}
                //else
                //{
                //    //4
                //    m_TempTriangles.Add(vertextOffset + 0);
                //    m_TempTriangles.Add(vertextOffset - 2);
                //    m_TempTriangles.Add(vertextOffset - 1);
                //    m_TempTriangles.Add(vertextOffset - 1);
                //    m_TempTriangles.Add(vertextOffset + 1);
                //    m_TempTriangles.Add(vertextOffset + 0);
                //}

                if (vertextOffset == 0)
                {
                    m_TempTriangles.Add(vertextOffset + 1);
                    m_TempTriangles.Add(vertextOffset + 0);
                }
                else if (vertextOffset == 2)
                {
                    //vertextOffset = 2;
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset + 1);
                    m_TempTriangles.Add(vertextOffset - 1);
                }
                else
                {
                    //4
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset - 1);
                    m_TempTriangles.Add(vertextOffset - 2);
                    m_TempTriangles.Add(vertextOffset + 0);
                    m_TempTriangles.Add(vertextOffset + 1);
                    m_TempTriangles.Add(vertextOffset - 1);
                }

            }

            if (m_AddOrgrinal == false)
            {
                m_AddOrgrinal = true;
            }

            m_LastPress = mousePosition;

            if (vertextOffset <= 2) return;

            SetRender();
        }
    }

    private Vector3 ConvertScreenToWorldPoint(Vector3 mousePosition)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 1), Camera.MonoOrStereoscopicEye.Mono);
    }

    private Vector3 ConvertScreenToPaddleSpace(Vector3 mousePosition)
    {
        Vector3 vec3 = ConvertScreenToWorldPoint(mousePosition);
        return m_Paddle.InverseTransformPoint(vec3);
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
        if (m_MeshFilter == null) return;

        m_Verticles.AddRange(m_TempVerticles);
        m_Triangles.AddRange(m_TempTriangles);
        m_uvs.AddRange(m_Tempuvs);
        m_Colors.AddRange(m_TempColors);

        m_MeshFilter.mesh.Clear();
        m_MeshFilter.mesh.SetVertices(m_Verticles);
        m_MeshFilter.mesh.SetTriangles(m_Triangles, 0);
        m_MeshFilter.mesh.SetUVs(0, m_uvs);
        m_MeshFilter.mesh.SetColors(m_Colors);
        m_MeshRender.sharedMaterial.SetVector(ShaderProperties.DirVec4, new Vector4(-m_DirNormal.x, -m_DirNormal.y, 0, 0));

    }


    void RemoveCommandBuffers()
    {
        grabCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);
    }

    void AddCommandBuffers()
    {
        //m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, _tempTexture);
        m_CommandBuffer.Blit(_tempTexture, _opaqueTexture, m_BlurMaterial, 0);
        rawImage.texture = _opaqueTexture;

        m_MeshRender.sharedMaterial.SetTexture(ShaderProperties.BlurRenderTexture, _opaqueTexture);

        grabCamera.AddCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);
    }


    public void UpdateCameraTarget()
    {
        grabCamera.targetTexture = null;
        if (_opaqueTexture != null)
        {
            _opaqueTexture.Release();
        }

        _opaqueTexture = new RenderTexture(Screen.width, Screen.height,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _opaqueTexture.name = nameof(_opaqueTexture);
        _opaqueTexture.antiAliasing = 1;
        _opaqueTexture.wrapMode = TextureWrapMode.Clamp;
        _opaqueTexture.Create();

        if (_tempTexture != null)
        {
            _tempTexture.Release();
        }

        _tempTexture = new RenderTexture(Screen.width, Screen.height,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        _tempTexture.name = nameof(_tempTexture);
        _tempTexture.antiAliasing = 1;
        _tempTexture.Create();

        grabCamera.targetTexture = _tempTexture;

        UpdateCommandBuffers();
    }

    private void UpdateCommandBuffers()
    {
        RemoveCommandBuffers();
        AddCommandBuffers();
    }

    private void LateUpdate()
    {
        grabCamera.transform.position = Camera.main.transform.position;
    }
}
