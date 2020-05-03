using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokkayo
{
    [System.Serializable]
    public class MotionBlurEffectData : EffectDataBase
    {
        [Range(0, 1f)]
        public float blurSize = 0.2f;

        [Range(1, 6)]
        public int iterator = 1;
        public int maxIterations = 6;
    }
}

