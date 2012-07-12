using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    /// <summary>
    /// Class that provides Mean absolute deviation of given signal
    /// </summary>
    
    class MAD : FeatureExtractor<float[], double>
    {
        public double ExtractFeature(ref float[] values_array)
        {
            float median;
            //Prepare single dimension array before sorting
            float[] data = new float[values_array.Length];
            values_array.CopyTo(data, 0);
            Array.Sort(data);

            if (data.Length % 2 != 0)
            {
                median = data[(int)Math.Floor((float)data.Length / 2)];
            }
            else 
            {
                median = (data[data.Length / 2] + data[(data.Length / 2) - 1]) / 2;
            }

            for(int i=0; i<data.Length; i++)
            {
                data[i] = Math.Abs(median - data[i]);
            }

            if (data.Length % 2 != 0)
            {
                median = data[(int)Math.Floor((float)data.Length / 2)];
            }
            else
            {
                median = (data[data.Length / 2] + data[(data.Length / 2) - 1]) / 2;
            }

            return median;
        }
    }
}
