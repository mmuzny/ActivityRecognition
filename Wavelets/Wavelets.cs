using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathPrimitives;

namespace Preprocessing
{
    public class Wavelet
    {
        public double[] low_pass_filter_coef, high_pass_filter_coef;
        private WaveletType wavelet_type;
        public enum WaveletType{
            Daubechie3,
            Haar
        };

        private static double[] daubechie3_low_pass_coefs = { 0.035226291882100656,-0.13501102001039084,-0.085441273882241486,0.45987750211933132,
                                               0.80689150931333875,0.33267055295095688};
        private static double[] daubechie3_high_pass_coefs = { -0.33267055295095688,0.80689150931333875,-0.45987750211933132,-0.13501102001039084,
                                                 0.085441273882241486,0.035226291882100656};


        public Wavelet(WaveletType name){
            this.wavelet_type =  name;
            if (this.wavelet_type == WaveletType.Daubechie3) {
                this.low_pass_filter_coef = daubechie3_low_pass_coefs;
                this.high_pass_filter_coef = daubechie3_high_pass_coefs;
            }
        }

        public WaveletType name{
            get{ return this.wavelet_type;}
            set{ this.wavelet_type = value;}
        }
    }

    public class WaveletToolkit
    {

        public void forward_wavelet_transform(ref Wavelet w, double[] signal, int level, ref List<double[]> decomposed) { 
            
        }


        public void backward_wavelet_transform(ref Wavelet w, double[] signal, int level, ref List<double[]> decomposed) { 
            
        }

        public void wavelet_packet_decomposition(ref Wavelet w, double[] signal, int level, ref List<double[]> decomposed) {
            if (level == 0) 
            {
                decomposed.Add(signal);
                return; 
            }
            double[] level_coefs_g = signal, level_coefs_h = signal;
            TimeDomain.convolution(ref w.low_pass_filter_coef, level_coefs_g, out level_coefs_g);
            TimeDomain.downsample(level_coefs_g, 2, out level_coefs_g);
            wavelet_packet_decomposition(ref w, level_coefs_g, level-1, ref decomposed); 
            TimeDomain.convolution(ref w.high_pass_filter_coef, level_coefs_h, out level_coefs_h);            
            TimeDomain.downsample(level_coefs_h, 2, out level_coefs_h);
            wavelet_packet_decomposition(ref w, level_coefs_h, level-1, ref decomposed); 
        }
    }
}
