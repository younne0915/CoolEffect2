﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        RenderTexture _renderTex;
        CommandBuffer _transparentBuffer;

        public MotionBlurEffect(Camera camera) : base(camera)
        {
            
        }

        protected override void Initialize()
        {
            _shader = Shader.Find("younne/MotionBlur");
            base.Initialize();
            _transparentBuffer = new CommandBuffer();
        }

        public override void CreateEffect()
        {
            if (_renderTex != null)
            {
                _renderTex.Release();
            }

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 200,
                    RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            _renderTex.name = "RenderTex";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();


            _opaqueBuffer.Clear();
            _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex, _mat, 0);

            EffectCtrl.Instance.image.texture = _renderTex;

            _transparentBuffer.Clear();
            _transparentBuffer.Blit(_renderTex, BuiltinRenderTextureType.CurrentActive);

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _opaqueBuffer);
                _targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _transparentBuffer);
            }

        }
    }
}
