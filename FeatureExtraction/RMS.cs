using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    class RMS : FeatureExtractor<float[], double>
    {
        public double ExtractFeature(ref float[] values_array) {
            float sum=0;
            foreach (float value in values_array) {
                sum += value;
            }
            sum /= values_array.Length;
            return Math.Sqrt(sum);
        }
    }
}
