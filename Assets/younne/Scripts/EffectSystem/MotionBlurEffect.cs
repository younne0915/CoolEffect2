using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        RenderTexture _renderTex;
        public MotionBlurEffect(Camera camera) : base(camera)
        {
            CreateMat(Shader.Find("younne/MotionBlur"));
        }

        public void SetColor(RawImage image)
        {
            //if(_renderTex == null)
            //{
            //    _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 16,
            //   RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            //    _renderTex.name = "GrabCamera Texture";
            //    _renderTex.antiAliasing = 1;
            //    _renderTex.Create();
            //}
            //else
            //{
            //    _renderTex.Release();
            //}

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 16,
               RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _renderTex.name = "GrabCamera Texture";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();

            _cmdBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex, _mat);
            image.texture = _renderTex;

        }
    }
}
