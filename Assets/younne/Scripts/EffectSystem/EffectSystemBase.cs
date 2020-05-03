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
        protected Camera _targetCamera;
        protected Material _mat;
        protected EffectDataBase _effectData;
        protected virtual Shader _shader { get; }

        protected EffectState _state = EffectState.Idle;

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