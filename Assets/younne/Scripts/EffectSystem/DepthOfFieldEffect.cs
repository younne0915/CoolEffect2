using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    public class DepthOfFieldEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/BlurEffect");

        CommandBuffer _transparentBuffer = new CommandBuffer();
        RenderTexture _renderTex;

        private float _blurIntensity = 2;

        private int iterator = 6;

        private int MaxIterations = 6;
        int[] _tempRenderTextureDownIds;


        public DepthOfFieldEffect(Camera camera, EffectDataBase effectData) : base(camera, effectData)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            _tempRenderTextureDownIds = new int[MaxIterations];
            for (int i = 0; i < MaxIterations; i++)
            {
                _tempRenderTextureDownIds[i] = Shader.PropertyToID($"_BloomTempDown{i}");
            }
        }

        public override void StartEffect()
        {
            if (_state == EffectState.Running) return;

            base.StartEffect();

            _targetCamera.enabled = true;

            if (_renderTex != null)
            {
                _renderTex.Release();
            }

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 200,
                    RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            _renderTex.name = "RenderTex";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();

            var tempTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 200,
                   RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            tempTex.name = "TempTex";
            tempTex.antiAliasing = 1;
            tempTex.Create();

            _targetCamera.targetTexture = tempTex;

            _opaqueBuffer.Clear();
            _opaqueBuffer.SetGlobalFloat(ShaderProperties.BlurSize, _blurIntensity);

            RenderTargetIdentifier source = tempTex;
            
            for (int i = 0; i < iterator; i++)
            {
                var mipDown = _tempRenderTextureDownIds[i];

                _opaqueBuffer.GetTemporaryRT(mipDown,
                    _targetCamera.pixelWidth, _targetCamera.pixelHeight,
                    0, FilterMode.Bilinear,
                    RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);

                _opaqueBuffer.Blit(source, mipDown, _mat, 0);
                source = mipDown;
            }

            _opaqueBuffer.Blit(source, tempTex);

            //RenderTexture total = source;
            //MotionEffectCtrl.Instance.image.texture = source;

            _transparentBuffer.Clear();
            _transparentBuffer.Blit(tempTex, BuiltinRenderTextureType.CurrentActive);

            for (int i = 0; i < iterator; i++)
            {
                var mipDown = _tempRenderTextureDownIds[i];
                _opaqueBuffer.ReleaseTemporaryRT(mipDown);
            }

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterSkybox, _opaqueBuffer);
            }
            MotionEffectCtrl.Instance.mainCam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _transparentBuffer);
        }



        public override void ReleaseEffect()
        {
            base.ReleaseEffect();


            if (_targetCamera != null)
            {
                _targetCamera.enabled = false;
                _targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _opaqueBuffer);
            }
            MotionEffectCtrl.Instance.mainCam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _transparentBuffer);
        }
    }
}
