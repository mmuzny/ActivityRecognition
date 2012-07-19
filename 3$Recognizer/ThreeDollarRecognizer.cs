using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using SignalProcessing;

namespace Classification
{
    public class ThreeDollarRecognizer<T,L,M> : Classifier<T,L> where T : SignalProcessing.Unistroke<M> 
                                                              where L : List<SignalProcessing.Unistroke<M>>
    {
        //Defines number of samples in one point serie
        public static int N = 64;
        
        //Expression tree build lambda function for subtraction
        private static readonly Func<M, M, double> Sub, Add, Mul, Div;

        //Static class constructor
        static ThreeDollarRecognizer() {
            var firstOperand = Expression.Parameter(typeof(M), "i");
            var secondOperand = Expression.Parameter(typeof(M), "j");
            var body = Expression.Subtract(firstOperand, secondOperand);
            var body_mul = Expression.Subtract(firstOperand, secondOperand);
            var body_div = Expression.Subtract(firstOperand, secondOperand);
            var body_add = Expression.Subtract(firstOperand, secondOperand);
            Sub = Expression.Lambda<Func<M, M, double>>
                  (body, firstOperand, secondOperand).Compile();
            Div = Expression.Lambda<Func<M, M, double>>
                  (body_div, firstOperand, secondOperand).Compile();
            Add = Expression.Lambda<Func<M, M, double>>
                  (body_add, firstOperand, secondOperand).Compile();
            Mul = Expression.Lambda<Func<M, M, double>>
                  (body_mul, firstOperand, secondOperand).Compile();
        }

        //Distance computation between two points
        private double distance(Unistroke<M>.Point3<M> i,Unistroke<M>.Point3<M> j){
            return Math.Sqrt(Math.Pow(Sub(i.x, j.x), 2) + Math.Pow(Sub(i.y, j.y), 2) + Math.Pow(Sub(i.z, j.z), 2)); 
        }

        //Determines length of whole path
        private double path_length(ref Unistroke<M> unistroke) {
            double d = 0;

            for (int i = 0; i < unistroke.Length()-1; i++) {
                d += distance(unistroke[i], unistroke[i + 1]); 
            }

            return d;
        }

        //Resamples points using linear interpolation
        public void resample(ref T uni, out List<Unistroke<M>.Point3<M>> new_points)
        {
            new_points = new List<Unistroke<M>.Point3<M>>();
            Unistroke<M> u = uni;
            double d_seq = path_length(ref u) / (N - 1);
            int i = 0; double d_curr = 0, d_comp = 0;
            for (i=0; i<u.trace.Count; i++){
                d_curr = distance(u[i], u[i + 1]);
                if (d_curr + d_comp > d_seq)
                {
                    double x = Add(u[i - 1].x, (M)(object)((d_seq - d_comp) * Sub(u[i].x, u[i - 1].x)));
                    double y = Add(u[i - 1].y, (M)(object)((d_seq - d_comp) * Sub(u[i].y, u[i - 1].y)));
                    double z = Add(u[i - 1].z, (M)(object)((d_seq - d_comp) * Sub(u[i].z, u[i - 1].z)));
                    new_points.Add(new Unistroke<M>.Point3<M>((M)(object)x, (M)(object)y, (M)(object)z));
                    uni.trace.Insert(i + 1, new Unistroke<M>.Point3<M>((M)(object)x, (M)(object)y, (M)(object)z));
                    d_comp = 0;
                }
                else 
                {
                    d_comp += d_curr;
                } 
            }
        }
        
        //Find the set centroid
        private void centroid(ref T uni, out Unistroke<M>.Point3<M> centroid) {
            centroid = new Unistroke<M>.Point3<M>();
            for (int i = 0; i < uni.trace.Count; i++){
                centroid.x = (M) (object) Add(uni[i].x, centroid.x);
                centroid.y = (M) (object) Add(uni[i].y, centroid.y);
                centroid.z = (M) (object) Add(uni[i].z, centroid.z);
            }

            centroid.x = (M)(object)((double)(object)centroid.x / uni.trace.Count);
            centroid.y = (M)(object)((double)(object)centroid.y / uni.trace.Count);
            centroid.x = (M)(object)((double)(object)centroid.x / uni.trace.Count);
        }
       
        //Rotate each point to zero in respect to indicative angle between first point and the set centroid
        public void rotate_to_zero(ref T uni, out List<Unistroke<M>.Point3<M>> new_points)
        {
            new_points = new List<Unistroke<M>.Point3<M>>();
            Unistroke<M>.Point3<M> center;
            Unistroke<M> u = uni;
            centroid(ref uni,out center);
            double angle = Math.Acos((double) (object) (center * uni[0]) / Mul(center.size(), uni[0].size()));
            Unistroke<M>.Point3<M> axis = (center ^ uni[0]) / (center ^ uni[0]).size();
            //Here, the 3d rotation matrix is constructed
            Unistroke<M>.Matrix3<M> rotation_matrix = new Unistroke<M>.Matrix3<M>();
            for (int i = 0; i < uni.trace.Count; i++) {
                new_points[i] = (rotation_matrix * (uni[i] - center)) + center;
            }
        }

        //Classify feature vector according to training structures
        public int classify(T obj, out int result)
        {
            result = 0;
            return 0;
        }
        
        //Train classifier by given training
        public int train(L obj){
            return 0;
        }
    }
}
