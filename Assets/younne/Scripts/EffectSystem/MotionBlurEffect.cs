using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        RenderTexture _targetTex;
        RenderTexture _renderTex;

        public MotionBlurEffect(Camera camera) : base(camera)
        {
            CreateMat(Shader.Find("younne/MotionBlur"));
        }

        public void SetColor(RawImage image)
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

            image.texture = _renderTex;
            
            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterEverything, _cmdBuffer);
            }

            //_targetCamera.targetTexture = null;

        }
    }
}
