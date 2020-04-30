using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        RenderTexture _targetTex;
        RenderTexture _renderTex;
        RenderTexture _tempTex;

        public MotionBlurEffect(Camera camera) : base(camera)
        {
            
        }

        protected override void Initialize()
        {
            base.Initialize();
            _shader = Shader.Find("younne/MotionBlur");
        }

        public override void CreateEffect()
        {
            if(_targetTex != null)
            {
                _targetTex.Release();
            }

            if (_renderTex != null)
            {
                _renderTex.Release();
            }

            _targetTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, (int)(_targetCamera.depth + 2),
               RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            _targetTex.name = "TargetTex";
            _targetTex.antiAliasing = 1;
            _targetTex.Create();

            _targetCamera.targetTexture = _targetTex;

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 0,
                    RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            _renderTex.name = "RenderTex";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();


            _cmdBuffer.Clear();
            _cmdBuffer.Blit(_targetTex, _renderTex, _mat, 0);

            EffectCtrl.Instance.image.texture = _renderTex;
            
            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterSkybox, _cmdBuffer);
            }

            _cmdBuffer.Clear();

            //_tempTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 0,
            //        RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            //_tempTex.name = "TempTex";
            //_tempTex.antiAliasing = 1;
            //_tempTex.Create();

            CommandBuffer tempBuffer = new CommandBuffer();


            tempBuffer.Blit(_renderTex, BuiltinRenderTextureType.CurrentActive);

            Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, tempBuffer);

        }
    }
}
