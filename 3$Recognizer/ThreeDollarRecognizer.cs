using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using SignalProcessing;

namespace Classification
{
    public class ThreeDollarRecognizer<T,L,M> : Classifier<T,L> where T : SignalProcessing.Unistroke<M>
                                                                where L : GestureSet<M>
    {
        //Defines number of samples in one point serie (maybe 64??)
        private static int N = 40;

        //Defines size of the bounding cube which is used to scale
        private static dynamic size = 127;

        //Templates list
        private static L templates;
           
        //GSS definition
        private static double phi = 0.5f * (-1 + Math.Sqrt(5));
        
        //Expression tree build lambda function for subtraction
        private static readonly Func<M, M, M> Sub, Add, Mul, Div;
        private static readonly Func<M, M, bool> Greater;

        //Static class constructor
        static ThreeDollarRecognizer() {
            var firstOperand = Expression.Parameter(typeof(M), "i");
            var secondOperand = Expression.Parameter(typeof(M), "j");
            var body = Expression.Subtract(firstOperand, secondOperand);
            var body_mul = Expression.Subtract(firstOperand, secondOperand);
            var body_div = Expression.Subtract(firstOperand, secondOperand);
            var body_add = Expression.Subtract(firstOperand, secondOperand);
            var body_gt = Expression.GreaterThan(firstOperand, secondOperand);
            Sub = Expression.Lambda<Func<M, M, M>>
                  (body, firstOperand, secondOperand).Compile();
            Div = Expression.Lambda<Func<M, M, M>>
                  (body_div, firstOperand, secondOperand).Compile();
            Add = Expression.Lambda<Func<M, M, M>>
                  (body_add, firstOperand, secondOperand).Compile();
            Mul = Expression.Lambda<Func<M, M, M>>
                  (body_mul, firstOperand, secondOperand).Compile();
            Greater = Expression.Lambda<Func<M, M, bool>>
                  (body_gt, firstOperand, secondOperand).Compile();
        }

        //Compute distance between 2 points
        private double distance(Unistroke<M>.Point3<M> a, Unistroke<M>.Point3<M> b)
        {
            return Math.Sqrt(Math.Pow((double)(object)Sub(a.x, b.x), 2) + Math.Pow((double)(object)Sub(a.y, b.y), 2) + Math.Pow((double)(object)Sub(a.z, b.z), 2));
        }


        //Determines length of whole path
        private double path_length(ref T unistroke) {
            double d = 0;

            for (int i = 0; i < unistroke.Length()-1; i++) {
                d += distance(unistroke[i], unistroke[i + 1]); 
            }

            return d;
        }

        //Determines path distance
        private double path_distance(ref T u_1, ref T u_2) {
            double d = 0;
            for (int i = 0; i < u_1.Length()-1; i++) {
                d += distance(u_1[i], u_2[i + 1]); 
            }
            return d;
        }

        //Resamples points using linear interpolation
        private void resample(ref T uni, out List<Unistroke<M>.Point3<M>> new_points)
        {
            new_points = new List<Unistroke<M>.Point3<M>>();
            Unistroke<M> u = uni;
            dynamic d_seq = path_length(ref uni) / (N - 1);
            int i = 0; dynamic d_curr = 0, d_comp = 0;
            for (i=0; i<u.trace.Count; i++){
                d_curr = distance(u[i], u[i + 1]);
                if (d_curr + d_comp > d_seq)
                {
                    M x = Add(u[i - 1].x, Mul((M) (object) (d_seq - d_comp), Sub(u[i].x, u[i - 1].x)));
                    M y = Add(u[i - 1].y, Mul((M) (object) (d_seq - d_comp), Sub(u[i].y, u[i - 1].y)));
                    M z = Add(u[i - 1].z, Mul((M) (object) (d_seq - d_comp), Sub(u[i].z, u[i - 1].z)));
                    new_points.Add(new Unistroke<M>.Point3<M>(x, y, z));
                    uni.trace.Insert(i + 1, new Unistroke<M>.Point3<M>(x, y, z));
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
            dynamic l = uni.trace.Count;
            centroid = new Unistroke<M>.Point3<M>();
            for (int i = 0; i < uni.trace.Count; i++){
                centroid.x = Add(uni[i].x, centroid.x);
                centroid.y = Add(uni[i].y, centroid.y);
                centroid.z = Add(uni[i].z, centroid.z);
            }

            centroid.x = Div(centroid.x, (M)(object)uni.trace.Count);
            centroid.y = Div(centroid.y, (M)(object)uni.trace.Count);
            centroid.z = Div(centroid.z, (M)(object)uni.trace.Count);
        }

        //Rotates points set by given angle and axis
        private void rotate_by(ref T uni, double angle, Unistroke<M>.Point3<M> axis) {
            Unistroke<M>.Matrix3<M> rotation_matrix = new Unistroke<M>.Matrix3<M>(angle, axis);
            for (int i = 0; i < uni.trace.Count; i++) {
                uni[i] = rotation_matrix * uni[i];
            }
        }

        //Rotate each point to zero in respect to indicative angle between first point and the set centroid
        private void rotate_to_zero(ref T uni)
        {
            Unistroke<M>.Point3<M> center;
            centroid(ref uni,out center);
            double angle = Math.Acos((double) (object) Div((center * uni[0]), Mul(center.size(), uni[0].size())));
            Unistroke<M>.Point3<M> axis = (center ^ uni[0]) / (center ^ uni[0]).size();
            //Here, the 3d rotation matrix is constructed
            Unistroke<M>.Matrix3<M> rotation_matrix = new Unistroke<M>.Matrix3<M>(angle, axis);
            for (int i = 0; i < uni.trace.Count; i++) {
                uni[i] = (rotation_matrix * (uni[i] - center)) + center;
            }
        }

        //Searches for bounding box borders
        private void bounding_box(ref T uni, out Unistroke<M>.Point3<M> a, out Unistroke<M>.Point3<M> b)
        {
            M min_x = (M) (object) 0, min_y =(M) (object) 0, min_z =(M) (object) 0, max_x= (M) (object)0, max_y = (M) (object) 0, max_z = (M) (object)0;
            for (int i = 0; i < uni.trace.Count; i++) {
                if (Greater(min_x, uni[i].x)) { min_x = uni[i].x; }
                if (Greater(min_y, uni[i].y)) { min_y = uni[i].y; }
                if (Greater(min_z, uni[i].z)) { min_z = uni[i].z; }
                if (!Greater(max_x, uni[i].x)) { max_x = uni[i].x; }
                if (!Greater(max_y, uni[i].y)) { max_y = uni[i].y; }
                if (!Greater(max_z, uni[i].z)) { max_z = uni[i].z; }
            }

            a = new Unistroke<M>.Point3<M>(min_x, min_y, min_z);
            b = new Unistroke<M>.Point3<M>(max_x, max_y, max_z);
        }

        //Scales points data set to cube
        private void scale_to_cube(ref T uni) {
            M width, height, depth;
            Unistroke<M>.Point3<M> a, b;
            bounding_box(ref uni, out a, out b);
            width = Sub(b.x, a.x);
            height = Sub(b.y, a.y);
            depth = Sub(b.z, a.z);
            for (int i = 0; i < uni.trace.Count; i++) {
                uni[i].x =(M) (object) Mul(uni[i].x,(M) (object) (size / width)); 
                uni[i].y =(M) (object) Mul(uni[i].y,(M) (object) (size / width)); 
                uni[i].z =(M) (object) Mul(uni[i].z,(M) (object) (size / width)); 
            }
        }
        
        //Translates points in respect to original centroid
        private void translate_to_original(ref T uni) {
            Unistroke<M>.Point3<M> p;
            centroid(ref uni, out p);
            for (int i = 0; i < uni.trace.Count; i++) {
                uni[i] = new Unistroke<M>.Point3<M>((M) (object) Sub(uni[i].x, p.x), (M) (object)Sub(uni[i].y, p.y), (M) (object) Sub(uni[i].z, p.z));
            }          
        }

        //Distance best angle and distance available with given angle array and icrement
        private void distance_at_angles(ref T uni, ref T t, double[] angles, double increment, out double min_d, out double[] min_angle)
        {
            double distance = double.PositiveInfinity;
            min_angle = new double[3];
            for (int i = 0; i < 8; i++)
            {
                double[] comute = { angles[0], angles[1], angles[2] };
                if (i % 2 == 1) { comute[0] += increment; }
                if (i % 4 > 1) { comute[1] += increment; }
                if (i % 8 > 3) { comute[2] += increment; }
                double dist = distance_at_single_angle(ref uni, ref t, comute);
                if (dist < distance) {
                    distance = dist;
                    min_angle = comute;
                }
            }
            min_d = distance;
        }
        
        //Distance at single specified angle
        private double distance_at_single_angle(ref T uni, ref T t, double[] angles) {          
            Unistroke<M>.Matrix3<M> rotation_matrix = new Unistroke<M>.Matrix3<M>(angles[0], angles[1], angles[2]);
            T new_points =(T) new Unistroke<M>();
            for (int i = 0; i < uni.trace.Count; i++) {
                new_points.trace.Add(rotation_matrix * uni[i]);
            }
            return path_distance(ref new_points, ref t);
        }


        //Computes distance at best angle
        private double distance_at_best_angle(ref T uni, ref T template, double lower_bound, double upper_bound, double extra_bound, double delta_theta)
        {
            double[] best_lower = { 0.0f, 0.0f, 0.0f };
            double[] best_upper = { 0.0f, 0.0f, 0.0f };
            double f_1, f_2;
            double x_1 = phi *-lower_bound + (1 - phi) * lower_bound;
            double x_2 = (1 - phi) * -lower_bound + phi * lower_bound;
            distance_at_angles(ref uni, ref template, best_lower, x_1, out f_1, out best_lower);
            distance_at_angles(ref uni, ref template, best_upper, x_2, out f_2, out best_upper);
            while (Math.Abs(upper_bound - lower_bound) > delta_theta) {
                if (f_1 < f_2)
                {
                    upper_bound = x_2;
                    x_2 = x_1;
                    f_2 = f_1;
                    x_1 = phi * lower_bound + (1 - phi) * upper_bound;
                    distance_at_angles(ref uni, ref template, best_lower, x_1, out f_1, out best_lower);
                }
                else 
                {
                    lower_bound = x_1;
                    x_1 = x_2;
                    f_1 = f_2;
                    x_2 = (1 - phi) * lower_bound + phi * upper_bound;
                    distance_at_angles(ref uni, ref template, best_upper, x_2, out f_2, out best_upper);
                }
            }
            return Math.Min(f_1, f_2);
        }

        //Classify feature vector according to training structures
        public int classify(T obj, out int result)
        {
            List<Unistroke<M>.Point3<M>> temp;
            resample(ref obj, out temp);
            obj = (T)(object)temp;
            rotate_to_zero(ref obj);
            scale_to_cube(ref obj);
            translate_to_original(ref obj);
            double inf = double.PositiveInfinity; T best_template = null;
            for(int i = 0; i < templates.Count; i++){
                T template =(T) (object) templates.gestures[i].unistrokes[0];
                double d = distance_at_best_angle(ref obj, ref template, Math.PI / 2, Math.PI / 2, Math.PI / 2, 2.0f * Math.PI * (15.0f / 360.0f));
               System.Console.WriteLine("Distance template " + i + " " + d);
                if (d < inf) { inf = d; best_template = template; }
            }

            result = 0;
            return 0;
        }
        
        //Train classifier by given training
        public int train(L obj){
            templates = obj;
            for (int i = 0; i < templates.Count; i++) { 
                 List<Unistroke<M>.Point3<M>> temp;
                 T template =(T) (object) templates.gestures[i].unistrokes[0];
                 resample(ref template, out temp);
                 template = (T)(object)temp;
                 rotate_to_zero(ref template);
                 scale_to_cube(ref template);
                 translate_to_original(ref template);
                 Console.WriteLine(i);
            }
            return 0;
        }
    }
}
