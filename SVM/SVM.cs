using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classification;

namespace Classification
{
    public class SVM<T, L, M> : Classifier<T, L> where T : SignalProcessing.Unistroke<M>
                                                 where L : GestureSet<M>                    
    {

    }
    public class SVM<T, L, M> : Classifier<T, L> where T : SignalProcessing.ActivityRecord<M>
                                                 where L : ActivitySet<M>
    {
        
    }

    public class Generic_SVM_Classier<M> {        

        //Expression tree build lambda function for subtraction, addition etc.
        private static readonly Func<M, M, M> Sub, Add, Mul, Div;
        private static readonly Func<M, M, bool> Greater;  

        //Pointers to feature vectors list
        private List<FeatureInstance<M>> feature_vectors;

        //Kernel function delegate type
        private delegate double kernel_function(M[] a, M[] b);

        //Pointer to kernel function
        private kernel_function kern;

        //Defines kernel function sigma parameter
        private double sigma;

        //Alpha coeffs
        private double[] alpha;

        //Error cache
        private double[] error_cache;

        //Defines used kernel function for non-linear classification
        private Kernel kernel;

        //Generic SVM Classifier constructor
        public Generic_SVM_Classier(Kernel k, double sigma) {
            this.kernel = k;
            this.sigma = sigma;
            if (this.kernel == Kernel.Gaussian) this.kern = gaussian_kernel;
            if (this.kernel == Kernel.Polynomial) this.kern = polynomial_kernel;
        }

        //Implements Sequential Minimal Optimization according to John Platt, MS Research proposal
        //Used to identify support vectors, find optimal lagrange multiplicator values
        //Ref: http://research.microsoft.com/en-us/um/people/jplatt/smoTR.pdf

        private void sequential_minimal_optimization() {
            this.alpha = new double[feature_vectors.Count];
            int num_changed = 0, examine_all = 1;
            while (num_changed > 0 || examine_all == 1) {
                num_changed = 0;
                if (examine_all == 1)
                {
                    foreach (FeatureInstance<M> vector in feature_vectors)
                    {
                        num_changed += examine_example(vector);
                    }
                }
                else 
                {
                    for (int i = 0; i < feature_vectors.Count; i++ )
                    {
                        if (alpha[i] != 0 && alpha[i] != 0)
                            examine_all += examine_example(feature_vectors[i]);
                    }
                }
                if (examine_all == 1) examine_all = 0;
                else if (num_changed == 0) examine_all = 1;
            }
        }

        private int examine_example(FeatureInstance<M> vector) {
            int y2 = vector.classes.First();
            error_cache[feature_vectors.
        }

        private double compute(FeatureInstance<M> ins) {
            double sum = -bias;

            for (int i = 0; i < feature_vectors.Count; i++) {
                if (alpha[i] > 0)
                    sum += alpha[i] * feature_vectors[i].classes.First() * kern(feature_vectors[i].features, ins.features);
            }

            return sum;
        }

        public enum Kernel
        {
            Gaussian,
            Polynomial,
            Hyperbolic
        }

        //Gaussian kernel definition
        private double gaussian_kernel(M[] a, M[] b)
        {
            double partial_sum=0;
            for (int i = 0; i < a.Length; i++) {
                partial_sum+=Math.Pow((double)(object)Sub(a[i], b[i]), 2);
            }
            return 1 / Math.Exp(partial_sum / 2 * sigma);
        }
        
        private double polynomial_kernel(M[] a, M[] b)
        {
            //TODO
            return 0;
        }
    }
}
