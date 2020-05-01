using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sokkayo;

public class MotionEffectCtrl : MonoBehaviour
{
    public Camera grabCam;
    public Camera mainCam;
    public RawImage image;

    [SerializeField]
    private Button motionBlurBtn;

    private DepthOfFieldEffect depthOfFieldEffect;

    public static MotionEffectCtrl Instance = null;

    private void Awake()
    {
        Instance = this;
        motionBlurBtn.onClick.AddListener(DepthOfFieldClick);
    }


    private void DepthOfFieldClick()
    {
        //_IsMove = true;

        Debug.LogErrorFormat("MotionBlurClick");

        if (depthOfFieldEffect == null)
        {
            depthOfFieldEffect = new DepthOfFieldEffect(grabCam);
        }

        depthOfFieldEffect.CreateEffect();
    }
}
