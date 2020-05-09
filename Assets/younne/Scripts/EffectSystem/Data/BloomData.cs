using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokkayo
{

    [System.Serializable]
    public class BloomData : EffectDataBase
    {
        [Range(1, 16)]
        public int iterations = 1;

        [Range(0, 10)]
        public float threshold = 1;

        [Range(0, 1)]
        public float softThreshold = 0.5f;

        [Range(0, 10)]
        public float intensity = 1;

        public bool debug;

    }

}