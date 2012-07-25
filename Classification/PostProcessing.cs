using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classification
{
    /// <summary>
    /// Defines interface to general PostProcessing mechanism (Voting, Boosting, ... )
    /// </summary>
    /// <typeparam name="T">Classified structure</typeparam>
    /// <typeparam name="L">Set of classified structures</typeparam>
      
    interface PostProcessing<T,L>
    {
        /// <summary>
        /// Overloaded method for postprocessing over trained classifiers
        /// </summary>
        /// <param name="obj">Referenced classifier set</param>
        /// <param name="f">Feature vector structure</param>
        /// <param name="result">Result of postprocessing</param>
        /// <returns>Success of postprocessing</returns>
        int postprocess(ref Classifier<T,L>[] obj, T f, out int result);

        /// <summary>
        /// Overloaded method for postprocessing over non-trained classifiers
        /// Trained inside this method using labeled data
        /// </summary>
        /// <param name="obj">Non-referenced classifier set</param>
        /// <param name="f_s">Labeled feature set</param>
        /// <param name="f">Feature vector structure</param>
        /// <param name="result">Result of postprocessing</param>
        /// <returns>Success of postprocessing</returns>
        int postprocess(Classifier<T, L>[] obj, L f_s, T f, out int result);
    }
}
