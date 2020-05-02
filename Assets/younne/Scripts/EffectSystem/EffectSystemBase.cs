using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    static class ShaderProperties
    {
        public static readonly int BlurIntensity = Shader.PropertyToID("_BlurIntensity");
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
        protected Camera _targetCamera;
        protected Material _mat;
        protected virtual Shader _shader { get; }

        protected EffectState _state = EffectState.Idle;

        public EffectSystemBase(Camera camera)
        {
            _targetCamera = camera;
            if (camera == null)
            {
                Debug.LogError("[EffectSystem] Effect Target Is Null, Error");
            }

            Initialize();
        }

        protected virtual void Initialize()
        {
            _opaqueBuffer = new CommandBuffer() { name = this.ToString() };
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