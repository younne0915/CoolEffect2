using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sokkayo;

public class MotionEffectCtrl : MonoBehaviour
{
    public static MotionEffectCtrl Instance = null;

    public Camera grabCam;
    public Camera mainCam;
    public RawImage image;

    [SerializeField]
    private Button motionBlurBtn;

    private DepthOfFieldEffect depthOfFieldEffect;

    private bool _IsMove = false;
    private float _passTime = 0;

    private void Awake()
    {
        Instance = this;
        motionBlurBtn.onClick.AddListener(DepthOfFieldClick);
    }


    private void DepthOfFieldClick()
    {
        _IsMove = true;

        if (depthOfFieldEffect == null)
        {
            depthOfFieldEffect = new DepthOfFieldEffect(grabCam, null);
        }

        depthOfFieldEffect.StartEffect();
    }

    private void Update()
    {
        if (_IsMove)
        {
            _passTime += Time.deltaTime;
            if(_passTime > 9999999)
            {
                _IsMove = false;
                _passTime = 0;

                if(depthOfFieldEffect != null)
                {
                    depthOfFieldEffect.ReleaseEffect();
                }
            }
        }
    }
}
