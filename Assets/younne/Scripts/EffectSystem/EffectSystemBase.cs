using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    public class EffectSystemBase : IDisposable
    {
        protected CommandBuffer _cmdBuffer;
        protected Camera _targetCamera;
        protected Material _mat;

        public EffectSystemBase(Camera camera)
        {
            _cmdBuffer = new CommandBuffer() { name = "EffectSystem" };

            if(camera == null)
            {
                Debug.LogError("[EffectSystem] Effect Target Is Null, Error");
                return;
            }
            _targetCamera = camera;
        }

        protected void CreateMat(Shader shader)
        {
            if(shader == null)
            {
                Debug.LogError("shader == null");
            }
            else
            {
                _mat = new Material(shader);
            }
        }

        public void ExecuteCmdBuffer(CameraEvent cameraEvent)
        {
            if(_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(cameraEvent, _cmdBuffer);
                //_cmdBuffer.Clear();
            }
        }

        public void Dispose()
        {
            _cmdBuffer.Dispose();
        }
    }

}