using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classification;
using SignalProcessing;

namespace Classification
{
    public class SVM<T, L, M> : Classifier<T, L> where L : SignalProcessing.RecordSet<M>
    {
        //Record set
        private L record_set;
      
        //Expression tree build lambda function for subtraction, addition etc.
        private static readonly Func<M, M, M> Sub, Add, Mul, Div;
        private static readonly Func<M, M, bool> Greater;

        //Initializes random number generator
        Random rand = new Random();

        //Pointers to feature vectors list
        private List<FeatureInstance<M>> feature_vectors;

        //Kernel function delegate type
        private delegate double kernel_function(M[] a, M[] b, double p);

        //Pointer to kernel function
        private kernel_function kern;

        //Alpha coeffs
        private double[] alpha;

        //Error cache
        private double[] error_cache;

        //Epsilon parameter
        private double epsilon = 1e-3;

        //C boundary parameter
        private double C = 1;

        //Tolerance parameter
        private double tolerance = 1e-3;

        //Bias parameter
        private double bias;

        //Parameter for kernel function (sigma - gaussian, d - polynomial)
        private double parameter;

        //Defines used kernel function for non-linear classification
        private Kernel kernel;

        //Generic SVM Classifier constructor
        public SVM(Kernel k, double parameter) {
            this.kernel = k;
            this.parameter = parameter;
            if (this.kernel == Kernel.Gaussian) 
            { 
                this.kern = gaussian_kernel; 
            }
            if (this.kernel == Kernel.Polynomial) 
            { 
                this.kern = polynomial_kernel; 
            }
        }

        public SVM() {
            this.kernel = Kernel.Polynomial;
            this.kern = polynomial_kernel;
            this.parameter = 2;
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
                        if (alpha[i] != 0 && alpha[i] != C)
                            examine_all += examine_example(feature_vectors[i]);
                    }
                }
                if (examine_all == 1) examine_all = 0;
                else if (num_changed == 0) examine_all = 1;
            }
        }

        //Examine first choice lagrange multiplicator
        private int examine_example(FeatureInstance<M> vector) {
            int i2 = feature_vectors.IndexOf(vector);
            int y2 = vector.classes.First();
            double alpha2 = alpha[i2];
            double e2 = (alpha2 > 0 && alpha2 < C) ? error_cache[feature_vectors.IndexOf(vector)] : compute(vector) - y2;

            double r2 = e2 * y2;
            if ((r2 < -tolerance && alpha2 < C) && !(r2 > tolerance && alpha2 > 0)) return 0;
            int i1 = -1; double max = 0;
            for (int i = 0; i < feature_vectors.Count; i++) {
                if (alpha[i] > 0 && alpha[i] < C) {
                    double aux = Math.Abs(e2 - error_cache[i]);
                    if (aux > max) { max = aux; i1 = i; }
                }
            }

            if (i1 >= 0 && takeStep(i1, i2)) return 1;

            int start = rand.Next(alpha.Length);
            for (i1 = 0; i1 < alpha.Length; ++i1){
                if (alpha[i1] > 0 && alpha[i1] < C){
                    if (takeStep(i1, i2)) return 1;
                }
            }
            for (i1 = start; i1 < alpha.Length; ++i1) { 
                if (alpha[i1] > 0 && alpha[i1] < C){                
                    if (takeStep(i1, i2)) return 1;
                }
            }

            start = rand.Next(alpha.Length);
            for (i1 = 0; i1 < alpha.Length; ++i1){
                    if (takeStep(i1, i2)) return 1;
            }
            for (i1 = start; i1 < alpha.Length; ++i1) { 
                    if (takeStep(i1, i2)) return 1;
            }

            return 0;
        }
        
        //Step forward with examining 2nd choice lagrange multiplicator
        private bool takeStep(int i1, int i2) {
            if (i1 == i2) return false;
            double alpha1 = alpha[i1];
            double alpha2 = alpha[i2];
            int y1 = feature_vectors[i1].classes.First();
            int y2 = feature_vectors[i2].classes.First();
            double e1 = (alpha1 > 0 && alpha1 < C) ? error_cache[i1] : compute(feature_vectors[i1]) - y1;
            double e2 = (alpha2 > 0 && alpha2 < C) ? error_cache[i2] : compute(feature_vectors[i2]) - y2;
            int s = y1 * y2;
            double L, H;

            if (y1 != y2)
            {
                L = Math.Max(0, alpha2 - alpha1);
                H = Math.Min(C, C + alpha2 - alpha1);
            }
            else {                 
                L = Math.Max(0, alpha2 + alpha1 - C);
                H = Math.Min(C, alpha2 + alpha1);
            }

            if (L == H) return false;

            double k11, k12, k22;
            double a2;

            k11 = kern(feature_vectors[i1].features, feature_vectors[i1].features, parameter);
            k12 = kern(feature_vectors[i1].features, feature_vectors[i2].features, parameter);
            k22 = kern(feature_vectors[i2].features, feature_vectors[i2].features, parameter);

            double eta = k11 + k22 - 2 * k12;

            if (eta > 0)
            {
                a2 = alpha2 + y2 * (e1 - e2) / eta;
                if (a2 < L) a2 = L;
                else if (a2 > H) a2 = H;
            }
            else 
            { 
                double L1 = alpha1 + s*(alpha2 - L);
                double H1 = alpha1 + s*(alpha2 - H);
                double f1 = y1*(e1 + bias) - alpha1*kern(feature_vectors[i1].features, feature_vectors[i1].features, parameter) - s*alpha2*kern(feature_vectors[i1].features, feature_vectors[i2].features, parameter);
                double f2 = y2*(e2 + bias) - alpha2*kern(feature_vectors[i2].features, feature_vectors[i2].features, parameter) - s*alpha1*kern(feature_vectors[i1].features, feature_vectors[i2].features, parameter);
                double Lobj = L1 * f1 + L * f2 + 0.5*Math.Pow(L1,2)*kern(feature_vectors[i1].features, feature_vectors[i1].features, parameter) + 0.5*Math.Pow(L,2)*kern(feature_vectors[i2].features,feature_vectors[i2].features,parameter) + s*L*L1*kern(feature_vectors[i1].features, feature_vectors[i2].features,parameter);                
                double Hobj = H1 * f1 + H * f2 + 0.5*Math.Pow(H1,2)*kern(feature_vectors[i1].features, feature_vectors[i1].features, parameter) + 0.5*Math.Pow(H,2)*kern(feature_vectors[i2].features,feature_vectors[i2].features,parameter) + s*H*H1*kern(feature_vectors[i1].features, feature_vectors[i2].features,parameter);
                if (Lobj < Hobj-epsilon) a2= L;
                else if (Lobj > Hobj + epsilon) a2 = H;
                else a2 = alpha2;
            }

            if (Math.Abs(a2 - alpha2) < epsilon * (a2 + alpha2 + epsilon)) return false;

            double a1 = alpha1 + s * (alpha2 - a2);

            if (a1 < 0)
            {
                a2 += s * a1;
                a1 = 0;
            }   else if (a1 > C) {
                double d = a1 - C;
                a2 += s * d;
                a1 = C;
            }

            double b1 = 0, b2 = 0;
            double new_bound = 0, delta_b;

            if (a1 > 0 && a1 < C)
            {
                new_bound = e1 + y1 * (a1 - alpha1) * k11 + y2 * (a2 - alpha2) * k12 + bias;

            }
            else {
                if (a2 > 0 && a2 < C) new_bound = e2 + y1 * (a1 - alpha1) * k12 + y2 * (a2 - alpha2) * k22 + bias;
                else {
                    b1 = e1 + y1 * (a1 - alpha1) * k11 + y2 * (a2 - alpha2) * k12 + bias;
                    b2 = e2 + y1 * (a1 - alpha1) * k12 + y2 * (a2 - alpha2) * k22 + bias;
                    new_bound = (b1 + b2) / 2;
                }                   
            }
            delta_b = new_bound - bias;
            bias = new_bound;

            double t1 = y1 * (a1 - alpha1);
            double t2 = y2 * (a2 - alpha2);

            for (int i = 0; i < feature_vectors.Count; ++i) {
                if (0 < alpha[i] && alpha[i] < C) {
                    error_cache[i] += t1 * kern(feature_vectors[i1].features, feature_vectors[i2].features, parameter) + t2 * kern(feature_vectors[i2].features, feature_vectors[i].features, parameter) - delta_b;
                }
            }

            error_cache[i1] = 0f;
            error_cache[i2] = 0f;

            alpha[i1] = a1;
            alpha[i2] = a2;

            return true;

        }

        //Compute SVM output on given feature vector
        private double compute(FeatureInstance<M> ins)
        {
            double sum = -bias;

            for (int i = 0; i < feature_vectors.Count; i++) {
                if (alpha[i] > 0)
                    sum += alpha[i] * feature_vectors[i].classes.First() * kern(feature_vectors[i].features, ins.features, parameter);
            }

            return sum;
        }

        //Kernel types enumeration
        public enum Kernel
        {
            Gaussian,
            Polynomial,
            Hyperbolic
        }

        //Gaussian kernel definition
        private double gaussian_kernel(M[] a, M[] b, double sigma)
        {
            double partial_sum=0;
            for (int i = 0; i < a.Length; i++) {
                partial_sum*=(double)(object)Sub(a[i], b[i]);
            }
            return 1 / Math.Exp(partial_sum / (2 * Math.Pow(sigma,2)));
        }

        //Polynomial kernel definition
        private double polynomial_kernel(M[] a, M[] b, double d) 
        {             
            double partial_sum=0;
            for (int i = 0; i < a.Length; i++) {
                partial_sum+=(double)(object)Mul(a[i], b[i]);
            }
            return Math.Pow(partial_sum, d);
        }


        public int train(L obj)
        {
            record_set = obj;
            error_cache = new double[record_set.numberOfFeatureVectors()];
            return 0;
        }

        public int classify(T obj, out int result) {
            result = 0;
            
            return 1;
        }
        
    }
}
