using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    #region 入门精要

    //public class BloomEffect : EffectSystemBase
    //{
    //    protected override Shader _shader => Shader.Find("younne/Bloom");

    //    private BloomData _bloomData;

    //    private List<RenderTexture> _releaseLit = new List<RenderTexture>();

    //    public BloomEffect(Camera camera, EffectDataBase effectData) : base(camera, effectData)
    //    {
    //        _bloomData = _effectData as BloomData;
    //    }

    //    public override void StartEffect()
    //    {
    //        base.StartEffect();

    //        int rtW = _targetCamera.pixelWidth / _bloomData.downSample;
    //        int rtH = _targetCamera.pixelWidth / _bloomData.downSample;

    //        RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 24);
    //        buffer0.filterMode = FilterMode.Bilinear;

    //        _opaqueBuffer.Clear();
    //        _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex);

    //        _opaqueBuffer.Blit(_renderTex, buffer0, _mat, 0);

    //        _mat.SetFloat(ShaderProperties.BlurSize, _bloomData.blurSize);

    //        for (int i = 0; i < _bloomData.totalIterator; i++)
    //        {
    //            _mat.SetFloat("_BlurSize", 1.0f + i * _bloomData.blurSize);

    //            RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

    //            // Render the vertical pass
    //            _opaqueBuffer.Blit(buffer0, buffer1, _mat, 1);

    //            RenderTexture.ReleaseTemporary(buffer0);
    //            buffer0 = buffer1;
    //            buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

    //            // Render the horizontal pass
    //            _opaqueBuffer.Blit(buffer0, buffer1, _mat, 2);

    //            RenderTexture.ReleaseTemporary(buffer0);
    //            buffer0 = buffer1;
    //        }

    //        EffectCtrl.Instance.image.texture = buffer0;

    //        _mat.SetTexture("_Bloom", buffer0);

    //        RenderTexture tempTex = RenderTexture.GetTemporary(rtW, rtH, 0);
    //        _opaqueBuffer.Blit(_renderTex, tempTex);

    //        _opaqueBuffer.Blit(tempTex, BuiltinRenderTextureType.CameraTarget, _mat, 3);

    //        if (_targetCamera != null)
    //        {
    //            _targetCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, _opaqueBuffer);
    //        }

    //        _releaseLit.Add(buffer0);
    //        _releaseLit.Add(tempTex);
    //    }

    //    public void Update()
    //    {
    //        SetPagram();
    //    }

    //    private void SetPagram()
    //    {
    //        _mat.SetFloat("_LuminanceThreshold", _bloomData.luminanceThreshold);
    //    }

    //    public override void ReleaseEffect()
    //    {
    //        base.ReleaseEffect();

    //        RenderTexture releaseTex = null;
    //        for (int i = 0; i < _releaseLit.Count; i++)
    //        {
    //            releaseTex = _releaseLit[i];
    //            if(releaseTex != null)
    //            {
    //                RenderTexture.ReleaseTemporary(releaseTex);
    //            }
    //        }
    //        _releaseLit.Clear();
    //    }
    //}

    #endregion

    static class BloomShaderPass
    {
        public static readonly int BoxDownPrefilterPass = 0;
        public static readonly int BoxDownPass = 1;
        public static readonly int BoxUpPass = 2;
        public static readonly int ApplyBloomPass = 3;
        public static readonly int DebugBloomPass = 4;
    }

    public class BloomEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/Bloom");

        private BloomData _bloomData;
        private int[] _bloomTexArr = new int[16];

        public BloomEffect(Camera camera, EffectDataBase effectData) : base(camera, CameraEvent.AfterImageEffects)
        {
            _bloomData = effectData as BloomData;
        }

        protected override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < 16; i++)
            {
                _bloomTexArr[i] = Shader.PropertyToID($"_BloomTempTex{i}");
            }
        }

        public override void StartEffect()
        {
            base.StartEffect();

            int width = _targetCamera.pixelWidth / 2;
            int height = _targetCamera.pixelHeight / 2;

            _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex);

            _opaqueBuffer.GetTemporaryRT(_bloomTexArr[0], width, height);
            RenderTargetIdentifier currDes = _bloomTexArr[0];
            _opaqueBuffer.Blit(_renderTex, currDes, _mat, BloomShaderPass.BoxDownPrefilterPass);
            RenderTargetIdentifier currSource = currDes;

            int i = 1;
            for (; i < _bloomData.iterations; i++)
            {
                width /= 2;
                height /= 2;
                if (height < 2 || width < 2)
                {
                    break;
                }

                _opaqueBuffer.GetTemporaryRT(_bloomTexArr[i], width, height, 0);
                currDes = _bloomTexArr[i];
                _opaqueBuffer.Blit(currSource, currDes, _mat, BloomShaderPass.BoxDownPass);
                currSource = currDes;
            }

            for (i -= 2; i >= 0; i--)
            {
                currDes = _bloomTexArr[i];
                _opaqueBuffer.Blit(currSource, currDes, _mat, BloomShaderPass.BoxUpPass);
                currSource = currDes;
            }

            if (_bloomData.debug)
            {
                _opaqueBuffer.Blit(currSource, BuiltinRenderTextureType.CameraTarget, _mat, BloomShaderPass.DebugBloomPass);
            }
            else
            {
                _opaqueBuffer.SetGlobalTexture("_SourceTex", _renderTex);
                _opaqueBuffer.Blit(currSource, BuiltinRenderTextureType.CameraTarget, _mat, BloomShaderPass.ApplyBloomPass);
            }

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(_triggerEvent, _opaqueBuffer);
            }
        }

        public void Update()
        {
            SetPagram();
        }

        private void SetPagram()
        {
            float knee = _bloomData.threshold * _bloomData.softThreshold;
            Vector4 filter;
            filter.x = _bloomData.threshold;
            filter.y = filter.x - knee;
            filter.z = 2f * knee;
            filter.w = 0.25f / (knee + 0.00001f);
            _mat.SetVector("_Filter", filter);
            _mat.SetFloat("_Intensity", Mathf.GammaToLinearSpace(_bloomData.intensity));
        }

        public override void ReleaseEffect()
        {
            base.ReleaseEffect();

            for (int i = 0; i < _bloomTexArr.Length; i++)
            {
                _opaqueBuffer.ReleaseTemporaryRT(_bloomTexArr[i]);
            }

            if (_targetCamera != null)
            {
                _targetCamera.RemoveCommandBuffer(_triggerEvent, _opaqueBuffer);
            }
        }
    }
}
