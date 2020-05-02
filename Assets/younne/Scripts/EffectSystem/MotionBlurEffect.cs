using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/MotionBlur");

        RenderTexture _renderTex;
        CommandBuffer _transparentBuffer;

        private float _blurIntensity = 2;

        private int iterator = 1;

        private int MaxIterations = 60;
        int[] _tempRenderTextureDownIds;

        public MotionBlurEffect(Camera camera) : base(camera)
        {
            
        }

        protected override void Initialize()
        {
            base.Initialize();
            _transparentBuffer = new CommandBuffer();

            _tempRenderTextureDownIds = new int[MaxIterations];
            for (int i = 0; i < MaxIterations; i++)
            {
                _tempRenderTextureDownIds[i] = Shader.PropertyToID($"_BloomTempDown{i}");
            }

            //_targetCamera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;

        }

        public override void StartEffect()
        {
            if (_renderTex != null)
            {
                _renderTex.Release();
            }

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 200,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            _renderTex.name = "RenderTex";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();


            _opaqueBuffer.Clear();
            _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex);
            //_opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex, _mat, 0);

            RenderTargetIdentifier source = _renderTex;
            for (int i = 0; i < iterator; i++)
            {
                var mipDown = _tempRenderTextureDownIds[i];

                _opaqueBuffer.GetTemporaryRT(mipDown,
                    _targetCamera.pixelWidth, _targetCamera.pixelHeight,
                    0, FilterMode.Point,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

                _opaqueBuffer.Blit(source, mipDown, _mat, 0);
                source = mipDown;
            }

            _opaqueBuffer.Blit(source, _renderTex);

            _transparentBuffer.Clear();
            _transparentBuffer.Blit(_renderTex, BuiltinRenderTextureType.CurrentActive);

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterSkybox, _opaqueBuffer);
                _targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _transparentBuffer);
            }

        }
    }
}
