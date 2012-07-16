using System;
using System.Collections.Generic;
using System.Linq;
using MathNet;

namespace Classification
{

    public class LDA<T> : Classifier<FeatureInstance<T>, FeatureInstance<T>[]>
    {
        
        private int features;
        private int classes;
        private List<FeatureInstance<T>> data;        
        private MathNet.Numerics.LinearAlgebra.Matrix x;
        private MathNet.Numerics.LinearAlgebra.Matrix p;
        private MathNet.Numerics.LinearAlgebra.Matrix c;
        private MathNet.Numerics.LinearAlgebra.Matrix c_inverse;
        private MathNet.Numerics.LinearAlgebra.Matrix[] means;
        private MathNet.Numerics.LinearAlgebra.Matrix[] xs;
        private MathNet.Numerics.LinearAlgebra.Matrix[] cs;
        private MathNet.Numerics.LinearAlgebra.Matrix mean_u;

        public int classify(FeatureInstance<T> obj, out int result)
        {
            result = 1;
            return 0;
        }
        
        public LDA(int features, int classes)
        {
            this.data = new List<FeatureInstance<T>>();
            this.classes = classes;
            this.features = features;
        }

        
        public int train(FeatureInstance<T>[] obj)
        {
            foreach (FeatureInstance<T> feature in obj) data.Add(feature);

            //Number of feature vectors each class
            int[] f = new int[this.classes];
            int j = 0, k = 0;
            //Initialize mean values arrays
            means = new MathNet.Numerics.LinearAlgebra.Matrix[this.classes];
            for (int i = 0; i < this.classes; i++) {
                means[i] = new MathNet.Numerics.LinearAlgebra.Matrix(1, features);    
            }
            mean_u = new MathNet.Numerics.LinearAlgebra.Matrix(1, features);


            //Compute the mean values of train data
            foreach (FeatureInstance<T> item in data)
            {
                for (int i = 0; i < features; i++)
                {
                    means[item.classes.ElementAt(0)][0, i] += (double) (object) item[i];
                    mean_u[0, i] += (double)(object) item[i];
                }
                f[item.classes.ElementAt(0)]++;
            }

            for (int i = 0; i < features; i++)
            {
                int j = 0;
                mean_u[0, i] /= f.Sum();
                foreach (MathNet.Numerics.LinearAlgebra.Matrix m in means) {
                    m[0, i] /= f[j++];    
                }
            }

            //Create classification matrices
            for (int i = 0; i < this.classes; i++) {                
                xs[i] = new MathNet.Numerics.LinearAlgebra.Matrix(f[i], features);
            }
            int[] skipper = new int[this.classes];
            for (int i = 0; i < classes; i++) {
                skipper[i] = 0;
            }
            x = new MathNet.Numerics.LinearAlgebra.Matrix(f.Sum(), features);
            foreach (FeatureInstance<T> item in data)
            {
                for (int i = 0; i < features; i++)
                {
                    xs[item.classes.ElementAt(0)][skipper[item.classes.ElementAt(0)], i] = (double) (object) item[i] - mean_u[0, i];
                }
                skipper[item.classes.ElementAt(0)]++;
            }


            //Create covariance matrices
            cs = new MathNet.Numerics.LinearAlgebra.Matrix[this.classes];
            for (int i = 0; i < this.classes; i++) {
                cs[i] = MathNet.Numerics.LinearAlgebra.Matrix.Transpose(xs[i]) * xs[i];
            }
            for (int i = 0; i < features; i++)
            {
                for (j = 0; j < features; j++)
                {
                    for (int a = 0; a < classes; a++) {
                        cs[a][i, j] /= f[a];
                    }
                }
            }

            //Compute pooled covariance matrix
            c = new MathNet.Numerics.LinearAlgebra.Matrix(features, features);

            for (int i = 0; i < features; i++)
            {
                for (j = 0; j < features; j++)
                {
                    c[i, j] = ((double)f1 / ((double)f1 + (double)f2)) * c_1[i, j] + ((double)f1 / ((double)f1 + (double)f2)) * c_2[i, j];
                }
            }

            //Compute prior probability vector

            p = new MathNet.Numerics.LinearAlgebra.Matrix(1, 2);
            p[0, 0] = ((double)f1 / ((double)f1 + (double)f2));
            p[0, 1] = ((double)f2 / ((double)f1 + (double)f2));

            c_inverse = this.c.Inverse();

            return 0;

        }

        public int classify(double[] vector)
        {
            //Compute the classification sums
            MathNet.Numerics.LinearAlgebra.Matrix v = new MathNet.Numerics.LinearAlgebra.Matrix(vector, 1);

            MathNet.Numerics.LinearAlgebra.Matrix sum_1 = this.mean_u1 * this.c_inverse * MathNet.Numerics.LinearAlgebra.Matrix.Transpose(v) - 0.5 * this.mean_u1 * this.c_inverse * MathNet.Numerics.LinearAlgebra.Matrix.Transpose(this.mean_u1);
            double su_1 = sum_1[0, 0];
            double f_1 = su_1 + Math.Log(p[0, 0]);

            MathNet.Numerics.LinearAlgebra.Matrix sum_2 = this.mean_u2 * this.c_inverse * MathNet.Numerics.LinearAlgebra.Matrix.Transpose(v) - 0.5 * this.mean_u2 * this.c_inverse * MathNet.Numerics.LinearAlgebra.Matrix.Transpose(this.mean_u2);
            double su_2 = sum_2[0, 0];
            double f_2 = su_2 + Math.Log(p[0, 1]);
            Console.WriteLine(f_1);
            Console.WriteLine(f_2);

            if (f_1 > f_2) { return 1; } else { return 2; }

        }
    }
}
