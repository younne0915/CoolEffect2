using Sokkayo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EffectCtrl : MonoBehaviour
{
    [SerializeField]
    private Button motionBlurBtn;

    private bool _IsMove = false;

    [SerializeField]
    private float speed = 2;

    [SerializeField]
    private float motionInterval = 2;

    [SerializeField]
    public Camera grabCam;

    [SerializeField]
    public Camera mainCam;

    public RawImage image;

    private float _motionTime = 0;

    private MotionBlurEffect motionBlur;

    public static EffectCtrl Instance = null;

    private void Awake()
    {
        Instance = this;
        motionBlurBtn.onClick.AddListener(MotionBlurClick);
    }

    private void MotionBlurClick()
    {
        //_IsMove = true;

        Debug.LogErrorFormat("MotionBlurClick");

        if(motionBlur == null)
        {
            motionBlur = new MotionBlurEffect(mainCam);
        }

        motionBlur.CreateEffect();
    }

    void LateUpdate()
    {
        if (_IsMove)
        {
            transform.position += transform.forward * Time.deltaTime * speed;
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            _motionTime += Time.deltaTime;

            if(_motionTime > motionInterval)
            {
                _IsMove = false;
                _motionTime = 0;
            }
        }
    }
}
