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
    public class Gesture<M> {

        public Gesture(string gesture_name) {
            this._gestureName = gesture_name;
            this.unistrokes = new List<Unistroke<M>>();
        }

        //Defines name of a Gesture
        string _gestureName;
        public string gestureName
        {
            get { return _gestureName; }
            set { _gestureName = value; }
        }


        //Defines gesture training sets
        public List<Unistroke<M>> unistrokes;        
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
    public class GestureSet<M> {

        public GestureSet() {
            gestures = new List<Gesture<M>>();
        }

        //List of possible recognizable gestures
        public List<Gesture<M>> gestures;

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
    public class ActivitySet<M> {
        //List of possible recognizable activities
        //public List<Gesture> activities;

        //Count of gestures in set
        /*
        public int Count
        {
            get
            {
                return activities.Count;
            }
        }
        */
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
