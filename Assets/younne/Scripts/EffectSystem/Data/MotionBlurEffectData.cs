﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokkayo
{
    [System.Serializable]
    public class MotionBlurEffectData : EffectDataBase
    {
        [Range(0, 0.2f)]
        public float blurSize = 0.1f;

        [Range(1, 6)]
        public int totalIterator = 1;
        public int maxIterations = 6;
    }
}

