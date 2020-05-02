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
    public bool IsMove
    {
        get { return _IsMove; }
    }

    [SerializeField]
    private float speed = 2;

    [SerializeField]
    private float motionInterval = 2;

    [SerializeField]
    public Camera grabCam;

    [SerializeField]
    public Camera mainCam;

    [SerializeField]
    private Transform playerTran;

    public RawImage image;

    private float _motionTime = 0;

    private MotionBlurEffect motionBlur;

    public static EffectCtrl Instance = null;

    private void Awake()
    {
        mainCam.depthTextureMode = DepthTextureMode.Depth;

        Instance = this;
        motionBlurBtn.onClick.AddListener(MotionBlurClick);

        bool issupportsMotionVectors = SystemInfo.supportsMotionVectors;

        Debug.LogErrorFormat("issupportsMotionVectors = {0}", issupportsMotionVectors);
    }

    private void MotionBlurClick()
    {
        _IsMove = true;

        Debug.LogErrorFormat("MotionBlurClick");

        if(motionBlur == null)
        {
            motionBlur = new MotionBlurEffect(mainCam);
        }

        motionBlur.StartEffect();
    }

    void Update()
    {
        if (_IsMove)
        {
            playerTran.position += playerTran.forward * Time.deltaTime * speed;
            playerTran.position = new Vector3(playerTran.position.x, 0, playerTran.position.z);
            _motionTime += Time.deltaTime;

            if(_motionTime > motionInterval)
            {
                _IsMove = false;
                _motionTime = 0;
            }
        }
    }


}
