using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class PaddleMeshCreator : MonoBehaviour
{
    private bool m_IsPressFire = false;

    private Mesh m_Mesh;
    private List<Vector3> m_Verticles;
    private List<int> m_Triangles;
    private List<Vector2> m_uvs;
    private List<Vector3> m_Colors;

    private Vector3 m_LastPress = Vector3.zero;
    private bool m_AddOrgrinal = false;

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
                m_Mesh = new Mesh();
                m_Verticles = new List<Vector3>();
                m_Triangles = new List<int>();
                m_uvs = new List<Vector2>();
                m_Colors = new List<Vector3>();
            }
            else
            {
                m_Verticles.Clear();
                m_Triangles.Clear();
                m_uvs.Clear();
                m_Colors.Clear();
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


            if (m_AddOrgrinal)
            {
                Vector3 dir = mousePosition - m_LastPress;
                Vector3 crossVec3 = Quaternion.Euler(0, 0, 90) * dir;

                //Debug.LogErrorFormat("mousePosition = {0}, m_LastPress = {1}, dir = {2},crossVec3 = {3} ",
                //    mousePosition, m_LastPress, dir, crossVec3);

                var vertextOffset = m_Verticles.Count;

                Vector3 v1 = m_LastPress + dir;
                Vector3 v2 = m_LastPress - dir;

                m_Verticles.Add(v1);
                m_Verticles.Add(v2);

                if(vertextOffset == 0)
                {
                    m_Triangles.Add(vertextOffset + 0);
                    m_Triangles.Add(vertextOffset + 1);
                }
                else if(vertextOffset == 2)
                {
                    //vertextOffset = 2;
                    m_Triangles.Add(vertextOffset + 0);
                    m_Triangles.Add(vertextOffset + 0);
                    m_Triangles.Add(vertextOffset - 1);
                    m_Triangles.Add(vertextOffset + 1);
                }
                else
                {
                    //4
                    m_Triangles.Add(vertextOffset + 0);
                    m_Triangles.Add(vertextOffset - 2);
                    m_Triangles.Add(vertextOffset - 1);
                    m_Triangles.Add(vertextOffset - 1);
                    m_Triangles.Add(vertextOffset + 1);
                    m_Triangles.Add(vertextOffset + 0);
                }

            }

            if (m_AddOrgrinal == false)
            {
                m_AddOrgrinal = true;
            }

            m_LastPress = mousePosition;

            Debug.LogErrorFormat("m_Triangles = {0}, m_Verticles = {1} ", m_Triangles.Count, m_Verticles.Count);
        }
    }

    private Vector3 ConvertScreenToWorldPoint(Vector3 mousePosition)
    {
        return Camera.main.ScreenToWorldPoint(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
    }

}
