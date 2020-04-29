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
        CalculateDelPos();
    }

    void CalculateDelPos()
    {
        _deltVec = transform.position - targetTrans.position;
    }

    private void LateUpdate()
    {
        transform.position = targetTrans.position + _deltVec;
    }
}
