using Sokkayo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EffectCtrl : MonoBehaviour
{
    public static EffectCtrl Instance = null;

    [SerializeField]
    private Button motionBlurBtn;


    [SerializeField]
    private Button bloomBtn;

    private bool _IsMove = false;
    public bool IsMove
    {
        get { return _IsMove; }
    }

    [SerializeField]
    private AnimationCurve speedCurve;

    [SerializeField]
    [Range(2, 50)]
    private float speedScale;

    [SerializeField]
    private float motionInterval = 2;

    [SerializeField]
    public Camera mainCam;

    [SerializeField]
    private Transform playerTran;

    [SerializeField]
    private MotionBlurEffectData motionBlurEffectData;

    [SerializeField]
    private BloomData bloomData;

    public RawImage image;

    private float _passedTime = 0;

    private MotionBlurEffect motionBlur;
    private BloomEffect bloomEffect;


    private void Awake()
    {

        Instance = this;


        mainCam.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        //mainCam.depthTextureMode = DepthTextureMode.Depth;

        motionBlurBtn.onClick.AddListener(MotionBlurClick);
        bloomBtn.onClick.AddListener(BloomBtnClick);
    }

    private void BloomBtnClick()
    {
        if (bloomEffect == null)
        {
            bloomEffect = new BloomEffect(mainCam, bloomData);
        }

        bloomEffect.StartEffect();
    }

    private void MotionBlurClick()
    {
        _IsMove = true;


        if(motionBlur == null)
        {
            motionBlur = new MotionBlurEffect(mainCam, motionBlurEffectData);
        }

        motionBlur.StartEffect();
    }

    void Update()
    {
        if (_IsMove)
        {
            _passedTime += Time.deltaTime;

            if(_passedTime > motionInterval)
            {
                _IsMove = false;
                _passedTime = 0;

                if (motionBlur != null)
                {
                    motionBlur.ReleaseEffect();
                }
            }
            else
            {
                float percent = _passedTime / motionInterval;
                if (percent > 0.5f)
                {
                    motionBlurEffectData.totalIterator = 3;
                }
                else if (percent > 0.3)
                {
                    motionBlurEffectData.totalIterator = 2;
                }
                else
                {
                    motionBlurEffectData.totalIterator = 1;
                }
                float speed = speedCurve.Evaluate(percent) * speedScale;
                playerTran.position += playerTran.forward * Time.deltaTime * speed;
                playerTran.position = new Vector3(playerTran.position.x, 0, playerTran.position.z);

                if (motionBlur != null)
                {
                    motionBlur.Update();
                }
            }
        }

        if(bloomEffect != null)
        {
            bloomEffect.Update();
        }
    }



}
