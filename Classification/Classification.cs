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
    /// <typeparam name="L">Set of classified structures</typeparam>
    public interface Classifier<T,L>
    {
        /// <summary>
        /// Core classification method
        /// </summary>
        /// <param name="obj">Classified object</param>
        /// <param name="result">Result of classfication</param>
        /// <returns>Success of classification</returns>
        int classify(T obj, out int result);

        /// <summary>
        /// This method trains classifier 
        /// </summary>
        /// <returns>Success of training process</returns>
        int train(L obj);
    }

    public interface IterativeClassifier<T,L> : Classifier<T,L>
    {
        /// <summary>
        /// Updates classifiers model with a single entity
        /// </summary>
        /// <param name="obj">Classified entity</param>
        void updateClassifier(T obj);

        /// <summary>
        /// Updates classifiers model with a entity set
        /// </summary>
        /// <param name="obj">Classified entity set</param>
        void updateClassifier(L obj);
    }
}
