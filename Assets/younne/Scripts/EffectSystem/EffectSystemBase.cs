using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    public class EffectSystemBase : IDisposable
    {
        protected CommandBuffer _opaqueBuffer;
        protected Camera _targetCamera;
        protected Material _mat;
        protected Shader _shader;

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
        }

        public virtual void CreateEffect()
        {

        }

        public void ExecuteCmdBuffer(CameraEvent cameraEvent)
        {
            if(_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(cameraEvent, _opaqueBuffer);
                //_cmdBuffer.Clear();
            }
        }

        public void Dispose()
        {
            _opaqueBuffer.Dispose();
        }
    }

}