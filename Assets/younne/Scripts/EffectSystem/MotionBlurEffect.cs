﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sokkayo
{
    public class MotionBlurEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/MotionBlur");

        int[] _tempRenderTextureDownIds;

        Matrix4x4 previousViewProjectionMatrix;

        private MotionBlurEffectData _motionBlurEffectData;

        public MotionBlurEffect(Camera camera, EffectDataBase effectData) : base(camera, CameraEvent.AfterSkybox)
        {
            _motionBlurEffectData = effectData as MotionBlurEffectData;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _tempRenderTextureDownIds = new int[_motionBlurEffectData.maxIterations];
            for (int i = 0; i < _motionBlurEffectData.maxIterations; i++)
            {
                _tempRenderTextureDownIds[i] = Shader.PropertyToID($"_BloomTempDown{i}");
            }

            previousViewProjectionMatrix = _targetCamera.projectionMatrix * _targetCamera.worldToCameraMatrix;
        }

        public void Update()
        {
            SetPagram();
        }

        private void SetPagram()
        {
            _opaqueBuffer.SetGlobalMatrix("_PreviousViewProjectionMatrix", previousViewProjectionMatrix);
            Matrix4x4 currentViewProjectionMatrix = _targetCamera.projectionMatrix * _targetCamera.worldToCameraMatrix;
            Matrix4x4 currentViewProjectionInverseMatrix = currentViewProjectionMatrix.inverse;
            _opaqueBuffer.SetGlobalMatrix("_CurrentViewProjectionInverseMatrix", currentViewProjectionInverseMatrix);
            previousViewProjectionMatrix = currentViewProjectionMatrix;
        }

        public override void StartEffect()
        {
            if (_state == EffectState.Running) return;
            base.StartEffect();

            _opaqueBuffer.Clear();
            _opaqueBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTex);
            _opaqueBuffer.SetGlobalFloat(ShaderProperties.BlurSize, _motionBlurEffectData.blurSize);

            RenderTargetIdentifier source = _renderTex;
            for (int i = 0; i < _motionBlurEffectData.totalIterator; i++)
            {
                var mipDown = _tempRenderTextureDownIds[i];

                _opaqueBuffer.GetTemporaryRT(mipDown,
                    _targetCamera.pixelWidth, _targetCamera.pixelHeight,
                    0, FilterMode.Point,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

                _opaqueBuffer.Blit(source, mipDown, _mat, 0);
                source = mipDown;
            }

            _opaqueBuffer.Blit(source, BuiltinRenderTextureType.CameraTarget);

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(_triggerEvent, _opaqueBuffer);
            }

        }

        public override void ReleaseEffect()
        {
            base.ReleaseEffect();

            //TODO release mipDown
            for (int i = 0; i < _motionBlurEffectData.maxIterations; i++)
            {
                _opaqueBuffer.ReleaseTemporaryRT(_tempRenderTextureDownIds[i]);
            }

            if (_targetCamera != null)
            {
                _targetCamera.RemoveCommandBuffer(_triggerEvent, _opaqueBuffer);
            }
        }
    }
}
