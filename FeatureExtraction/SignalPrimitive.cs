using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    /// <summary>
    /// Provides signal primitive data structure, provides methods for specific feature extraction
    /// </summary>

    class SignalPrimitive
    {
        //Defines a private interfaces to feature extractors
        private Mean meanInterface = new Mean();
        private RMS rmsInterface = new RMS();
        private MAD madInterface = new MAD();

        //Single dimension array containing specific data
        public float[] dataLine;
        
        //Constructor defining previously set window length
        public SignalPrimitive() {
            dataLine=new float[(int) Math.Ceiling(Globals.sampleRate*Globals.windowWidth)];
        }

        //Constructor defining custom window length
        public SignalPrimitive(int signalLength) { 
            dataLine=new float[signalLength];
        }

        //Serves a mean of the signal
        public float mean
        {
            get
            {
                return meanInterface.ExtractFeature(ref dataLine);
            }
        }

        //Serves general root-mean-square of signal
        public double rms 
        {
            get 
            {
                return rmsInterface.ExtractFeature(ref dataLine);
            }
        }

        //Serves general MAD of signal
        public double mad
        {
            get
            {
                return madInterface.ExtractFeature(ref dataLine);
            }
        }

    }
}
