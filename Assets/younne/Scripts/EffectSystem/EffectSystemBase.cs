using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    static class ShaderProperties
    {
        public static readonly int BlurSize = Shader.PropertyToID("_BlurSize");
        public static readonly int PreviousViewProjectionMatrix = Shader.PropertyToID("_PreviousViewProjectionMatrix");
        public static readonly int CurrentViewProjectionInverseMatrix = Shader.PropertyToID("_CurrentViewProjectionInverseMatrix");
    }

    public enum EffectState
    {
        Idle,
        Created,
        Running,
    }

    public class EffectSystemBase : IDisposable
    {
        protected CommandBuffer _opaqueBuffer;
        protected CommandBuffer _transparentBuffer;
        protected Camera _targetCamera;
        protected Material _mat;
        protected EffectDataBase _effectData;
        protected virtual Shader _shader { get; }

        protected EffectState _state = EffectState.Idle;
        protected RenderTexture _renderTex;

        public EffectSystemBase(Camera camera, EffectDataBase effectData)
        {
            _targetCamera = camera;
            _effectData = effectData;
            if (camera == null)
            {
                Debug.LogError("[EffectSystem] Effect Target Is Null, Error");
            }

            Initialize();
        }

        protected virtual void Initialize()
        {
            _opaqueBuffer = new CommandBuffer() { name = this.ToString() + "Opaque" };
            _transparentBuffer = new CommandBuffer() { name = this.ToString() + "Transparent" };

            CreateMat();
        }

        protected void CreateMat()
        {
            if(_shader == null)
            {
                Debug.LogError("shader == null");
            }
            else
            {
                _mat = new Material(_shader);
            }

            _state = EffectState.Created;
        }

        public virtual void StartEffect()
        {
            _state = EffectState.Running;

            if (_renderTex != null)
            {
                _renderTex.Release();
            }

            _renderTex = new RenderTexture(_targetCamera.pixelWidth, _targetCamera.pixelHeight, 200,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            _renderTex.name = "RenderTex";
            _renderTex.antiAliasing = 1;
            _renderTex.Create();
        }

        public virtual void ReleaseEffect()
        {
            _state = EffectState.Idle;
        }

        public void Dispose()
        {
            _opaqueBuffer.Dispose();
        }
    }

}