using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokkayo
{

    [System.Serializable]
    public class BloomData : EffectDataBase
    {
        [Range(0.2f, 3.0f)]
        public float blurSize = 0.1f;

        [Range(1, 6)]
        public int totalIterator = 1;

        [Range(1, 8)]
        public int downSample = 2;

        [Range(0.0f, 4.0f)]
        public float luminanceThreshold = 0.6f;
    }

}