using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classification
{
    /// <summary>
    /// Represents a feature instance
    /// </summary>
    /// <typeparam name="T">Basic feature type (float, double,...)</typeparam>
    public class FeatureInstance<T>
    {

        /// <summary>
        /// 1d field of features associated with feature instance
        /// </summary>
        public T[] features
        {
            get;
            set;
        }

        /// <summary>
        /// Features indexer definition
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Specific feature from the features field</returns>
        public T this[int i]
        {
            get 
            {
                return this.features[i];    
            }
            set 
            {
                this.features[i] = value;
            }
        }


        /// <summary>
        /// HashSet containing classes associated with feature instance
        /// </summary>
        public HashSet<int> classes
        {
            get;
            set;
        }
    }
}
