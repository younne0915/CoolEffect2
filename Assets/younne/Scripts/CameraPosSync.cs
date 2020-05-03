using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosSync : MonoBehaviour
{
    [SerializeField]
    private Transform targetTrans;

    private Vector3 _deltVec;

    [ExecuteInEditMode]
    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        CalculateDelPos();
    }

    void CalculateDelPos()
    {
        _deltVec = transform.position - targetTrans.position;

    }

    private void Update()
    {
        transform.position = targetTrans.position + _deltVec;
    }

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    var mat = new Material(Shader.Find("younne/TestCameraMotion"));
    //    Graphics.Blit(source, destination, mat);
    //}
}
