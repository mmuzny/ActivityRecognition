using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    class Mean : FeatureExtractor<float[],float>
    {
        public float ExtractFeature(ref float[] values_array)
        {
            float sum=0;
            foreach (float value in values_array){
                sum+=value;
            }
            return sum/values_array.Length;
        }
    }
}
