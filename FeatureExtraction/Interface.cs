﻿/// Defines interfaces to generic feature extraction and gesture recognition
/// Template value T is either signal Window or FFT spectrum
/// Template value L is defined output structure

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

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
        private string _gestureName;
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
    public class Activity<M> { 

        public Activity(string activity_name) {
            this._activityName = activity_name;
            this.activity_records = new List<ActivityRecord<M>>();
        }

        //Defines name of a Activity
        string _activityName;

        //Defines activity training sets      
        public List<ActivityRecord<M>> activity_records;        
    }

    /// <summary>
    /// Gathers all gestures
    /// </summary>
    public class GestureSet<M> : RecordSet<M> {
        //List of possible recognizable gestures
        public List<Gesture<M>> gestures;

        public GestureSet() {
            gestures = new List<Gesture<M>>();
        }

        //Count of gestures in set
        public int Count {
            get {
                return gestures.Count;
            }
        }

        public override void extractFeatureInstances()
        {
            throw new NotImplementedException();
        } 
    }

    
    /// <summary>
    /// Gathers all activities
    /// </summary>
    public class ActivitySet<M> : RecordSet<M> {
        //List of possible recognizable activities
        public List<Activity<M>> activities;

        public ActivitySet() {
            activities = new List<Activity<M>>();
        }

        //Count of gestures in set
        public int Count
        {
            get
            {
                return activities.Count;
            }
        }

        public override void extractFeatureInstances()
        {
            throw new NotImplementedException();
        }

        public override int numberOfFeatureVectors()
        {            
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// Covering class of Activity & Gesture Set
    /// </summary>
    /// <typeparam name="M">Primitive signal value type</typeparam>
    public abstract class RecordSet<M> {

        public abstract int numberOfFeatureVectors();

        public abstract void extractFeatureInstances();

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
