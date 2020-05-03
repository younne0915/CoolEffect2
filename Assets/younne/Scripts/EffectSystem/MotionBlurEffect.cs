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
        int[] _tempRenderTextureDownIds;

        Matrix4x4 previousViewProjectionMatrix;

        private MotionBlurEffectData _motionBlurEffectData;

        public MotionBlurEffect(Camera camera, EffectDataBase effectData) : base(camera, effectData)
        {
            
        }

        protected override void Initialize()
        {
            base.Initialize();
            _transparentBuffer = new CommandBuffer();
            _motionBlurEffectData = _effectData as MotionBlurEffectData;

            _tempRenderTextureDownIds = new int[_motionBlurEffectData.maxIterations];
            for (int i = 0; i < _motionBlurEffectData.maxIterations; i++)
            {
                _tempRenderTextureDownIds[i] = Shader.PropertyToID($"_BloomTempDown{i}");
            }

            previousViewProjectionMatrix = _targetCamera.projectionMatrix * _targetCamera.worldToCameraMatrix;
        }

        public void Update()
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
            _opaqueBuffer.SetGlobalFloat(ShaderProperties.BlurSize, _motionBlurEffectData.blurSize);
            

            RenderTargetIdentifier source = _renderTex;
            for (int i = 0; i < _motionBlurEffectData.iterator; i++)
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

        public override void ReleaseEffect()
        {
            base.ReleaseEffect();

            if (_targetCamera != null)
            {
                _targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _opaqueBuffer);
                _targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _transparentBuffer);
            }
        }
    }
}
