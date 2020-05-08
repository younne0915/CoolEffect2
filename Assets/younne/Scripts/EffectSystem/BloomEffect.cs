using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{

    public class BloomEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/Bloom");

        private BloomData _bloomData;

        private List<RenderTexture> _releaseLit = new List<RenderTexture>();

        public BloomEffect(Camera camera, EffectDataBase effectData) : base(camera, effectData)
        {
            _bloomData = _effectData as BloomData;
        }

        public override void StartEffect()
        {
            base.StartEffect();

            int rtW = _targetCamera.pixelWidth / _bloomData.downSample;
            int rtH = _targetCamera.pixelWidth / _bloomData.downSample;

            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 24);
            buffer0.filterMode = FilterMode.Bilinear;

            _opaqueBuffer.Clear();
            _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex);

            _opaqueBuffer.Blit(_renderTex, buffer0, _mat, 0);

            _mat.SetFloat(ShaderProperties.BlurSize, _bloomData.blurSize);

            for (int i = 0; i < _bloomData.totalIterator; i++)
            {
                _mat.SetFloat("_BlurSize", 1.0f + i * _bloomData.blurSize);

                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the vertical pass
                _opaqueBuffer.Blit(buffer0, buffer1, _mat, 1);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the horizontal pass
                _opaqueBuffer.Blit(buffer0, buffer1, _mat, 2);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }

            EffectCtrl.Instance.image.texture = buffer0;

            _mat.SetTexture("_Bloom", buffer0);

            RenderTexture tempTex = RenderTexture.GetTemporary(rtW, rtH, 0);
            _opaqueBuffer.Blit(_renderTex, tempTex);

            _opaqueBuffer.Blit(tempTex, BuiltinRenderTextureType.CameraTarget, _mat, 3);

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, _opaqueBuffer);
            }

            _releaseLit.Add(buffer0);
            _releaseLit.Add(tempTex);
        }

        public void Update()
        {
            SetPagram();
        }

        private void SetPagram()
        {
            _mat.SetFloat("_LuminanceThreshold", _bloomData.luminanceThreshold);
        }

        public override void ReleaseEffect()
        {
            base.ReleaseEffect();

            RenderTexture releaseTex = null;
            for (int i = 0; i < _releaseLit.Count; i++)
            {
                releaseTex = _releaseLit[i];
                if(releaseTex != null)
                {
                    RenderTexture.ReleaseTemporary(releaseTex);
                }
            }
            _releaseLit.Clear();
        }
    }
}
