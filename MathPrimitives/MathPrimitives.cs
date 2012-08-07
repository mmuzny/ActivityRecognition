using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPrimitives
{
    public class TimeDomain
    {
        public static void convolution(ref double[] x, double[] y, out double[] output) {
            output = new double[y.Length + x.Length - 1];
            for (int i = 0; i < x.Length; i++) {
                for (int j = i; j < y.Length+i; j++) {
                    output[j] += x[i] * y[j];
                }
            }
        }

        public static void downsample(double[] x, int interval, out double[] downsampled) {
            int count = 0;
            downsampled = new double[x.Length - (x.Length / 4)];
            for (int i = 0; i < x.Length; i = i + 1) {
                if (i + 1 % interval != 0) downsampled[count++] = x[i];
            }            
        }
    }

    public class FrequencyDomain 
    {       
        /*     
        public static void FFT(ref double[] x, out double[] magnitude, out double[] phase) { 
                  
        }
         */
    }
}
