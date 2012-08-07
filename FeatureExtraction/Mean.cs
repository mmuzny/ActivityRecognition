using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    class Mean : FeatureExtractor<float[],double>
    {
        public double ExtractFeature(ref float[] values_array)
        {
            float sum=0;
            foreach (float value in values_array){
                sum+=value;
            }
            return sum/values_array.Length;
        }
    }
}
