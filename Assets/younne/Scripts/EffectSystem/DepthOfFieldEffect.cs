using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sokkayo
{
    public class DepthOfFieldEffect : EffectSystemBase
    {
        protected override Shader _shader => Shader.Find("younne/DepthOfField");
        private DepthOfFieldData _depthOfFieldData;

        public DepthOfFieldEffect(Camera camera, EffectDataBase effectData) : base(camera, CameraEvent.AfterForwardOpaque)
        {
            _depthOfFieldData = effectData as DepthOfFieldData;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override void StartEffect()
        {
            if (_state == EffectState.Running) return;

            base.StartEffect();

            

            if (_targetCamera != null)
            {
                _targetCamera.AddCommandBuffer(_triggerEvent, _opaqueBuffer);
            }
        }



        public override void ReleaseEffect()
        {
            base.ReleaseEffect();

            if (_targetCamera != null)
            {
                _targetCamera.RemoveCommandBuffer(_triggerEvent, _opaqueBuffer);
            }
        }
    }
}
