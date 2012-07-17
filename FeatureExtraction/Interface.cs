/// Defines interfaces to generic feature extraction and gesture recognition
/// Template value T is either signal Window or FFT spectrum
/// Template value L is defined output structure

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing
{
    /// <summary>
    /// Gesture class meant to define one gesture
    /// </summary>
    class Gesture {
        //Defines name of a Gesture
        string GestureName;

        //Defines gesture training sets
        
    }

    /// <summary>
    /// Activity class meant to define one activity
    /// </summary>
    class Activity { 
        //Defines name of a Activity
        string ActivityName;

        //Defines activity training sets
        
    }

    /// <summary>
    /// Gathers all gestures
    /// </summary>
    class GestureSet {
        //List of possible recognizable gestures
        public List<Gesture> gestures;

        //Count of gestures in set
        public int Count {
            get {
                return gestures.Count;
            }
        }
    }
    
    /// <summary>
    /// Gathers all activities
    /// </summary>
    class ActivitySet {
        //List of possible recognizable activities
        public List<Gesture> activities;

        //Count of gestures in set
        public int Count
        {
            get
            {
                return activities.Count;
            }
        }
    }

    /// <summary>
    /// Defines interface for feature extractor
    /// </summary>
    /// <typeparam name="T">Format of signal data line</typeparam>
    /// <typeparam name="L">Extracted feature format</typeparam>
    interface FeatureExtractor<T, L>
    {
        //Core method to be implemented 
        L ExtractFeature(ref T values_array);
    }

}
