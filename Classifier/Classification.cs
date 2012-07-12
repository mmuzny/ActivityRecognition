using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classification
{

    /// <summary>
    /// Defines interface for generic classificator
    /// </summary>
    /// <typeparam name="T">Classified structure</typeparam>
    interface Classifier<T>
    {
        /// <summary>
        /// Core classification method
        /// </summary>
        /// <param name="obj">Classified object</param>
        /// <returns>Success of classification</returns>
        int classify(T obj);

        /// <summary>
        /// Adds training sample to a classificator
        /// </summary>
        /// <param name="obj">Classified entity</param>
        void addTrainingSample(T obj);

        /// <summary>
        /// This method trains classifier 
        /// </summary>
        /// <returns>Success of training process</returns>
        int train();
    }
}
