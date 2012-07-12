using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    /// <summary>
    /// Small hack to define global variables
    /// </summary>
    public class Globals
    {
        //Defines accelerometer channels count
        public static int channelsCount = 3;
        //Defines sliding window width
        public static float windowWidth = 3.5f;
        //Defines sliding window overlap 
        public static float windowOverlap = 0.7f;
        //Defines a values sample rate in Hz
        public static float sampleRate=75f;
    }
}