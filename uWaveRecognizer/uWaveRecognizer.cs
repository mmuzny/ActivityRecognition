using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using SignalProcessing;

namespace Classification
{
        //uWaveRecognizer especially designed for classification of accelerometer data
        public class uWaveRecognizer<T,L,M> : Classifier<T,L> where T : SignalProcessing.Unistroke<M> 
                                                              where L : List<T>
        {   
            //Table for computation distances between nodes 
            private M[] warping_table;

            //Step of moving the window
            private static int moving_step = 1;

            //Sliding window size
            private static int window_size = 1;

            //Templates list
            private static L templates;

            //Expression tree build lambda function for subtraction
            private static readonly Func<M, M, double> Sub, Add, Mul, Div;
            private static readonly Func<M, M, bool> Greater;
            
            //Static constructor definition
            static uWaveRecognizer() {
                var firstOperand = Expression.Parameter(typeof(M), "i");
                var secondOperand = Expression.Parameter(typeof(M), "j");
                var body = Expression.Subtract(firstOperand, secondOperand);
                var body_mul = Expression.Subtract(firstOperand, secondOperand);
                var body_div = Expression.Subtract(firstOperand, secondOperand);
                var body_add = Expression.Subtract(firstOperand, secondOperand);
                var body_gt = Expression.GreaterThan(firstOperand, secondOperand);
                Sub = Expression.Lambda<Func<M, M, double>>
                      (body, firstOperand, secondOperand).Compile();
                Div = Expression.Lambda<Func<M, M, double>>
                      (body_div, firstOperand, secondOperand).Compile();
                Add = Expression.Lambda<Func<M, M, double>>
                      (body_add, firstOperand, secondOperand).Compile();
                Mul = Expression.Lambda<Func<M, M, double>>
                      (body_mul, firstOperand, secondOperand).Compile();
                Greater = Expression.Lambda<Func<M, M, bool>>
                      (body_gt, firstOperand, secondOperand).Compile();
            }

            public uWaveRecognizer() { }

            //Unroll points set in with respect to defined window_size and moving_step
            private void unroll_points(ref T uni, out T unrolled)
            {
                int i = 0; unrolled = (T) new Unistroke<M>();
                while (i < uni.trace.Count) {
                    Unistroke<M>.Point3<M> p = new Unistroke<M>.Point3<M>();
                    if ((i + window_size) > uni.trace.Count) window_size = uni.trace.Count - i;
                    for (int j = i; j < i + window_size; j++) {
                        p.x = (M) (object) Add(p.x, uni[i].x);
                        p.y = (M) (object) Add(p.y, uni[i].y);
                        p.z = (M) (object) Add(p.z, uni[i].z);
                    }
                    p.x = (M)(object)Div(p.x, (M)(object)window_size);
                    p.y = (M)(object)Div(p.y, (M)(object)window_size);
                    p.z = (M)(object)Div(p.z, (M)(object)window_size);
                    unrolled.trace.Add(p); i+=moving_step;
                }
            }

            //Scales value to -16..16 value range
            private void distraction_table(ref M value) {
                double cast_value = (double)(object)value;
                if ((cast_value > 80) && !(cast_value > 128)) {cast_value = 16; }
                else if ((cast_value > 40) && !(cast_value > 80)) {cast_value = 10 + (cast_value-10)/10 *5;}
                else if ((cast_value > -20) && !(cast_value > -10)) {cast_value = -10 + (cast_value+10)/10 *5;}
                else if ((cast_value > -128) && !(cast_value > -80)) {cast_value = -16;}
                else if ((cast_value > -40) && !(cast_value > 40)) {cast_value = cast_value / 4;}
                else {cast_value = 0;}
                value = (M)(object)cast_value;
            }

            //Quantizes points with non-linear scale
            private void quantize_points(ref T uni, ref List<byte> quantizated)
            {
                for (int i = 0; i < uni.trace.Count; i++) {
                    distraction_table(ref uni[i].x);
                    distraction_table(ref uni[i].y);
                    distraction_table(ref uni[i].z);
                }                            
            }

            //Dynamic Time Warping function to compute table values
            private void dtw(T obj, T obj_2, int index_1, int index_2, out double distance)
            { 
                
            }
            
            //Classify feature vector according to training structures
            public int classify(T obj, out int result)
            {
                double inf = double.PositiveInfinity; T best_template = null; result = 0;
                for(int i = 0; i < templates.Count; i++){
                    double distance;
                    T template = templates[i];
                    dtw(obj, template, obj.trace.Count - 1, obj.trace.Count - 1, out distance);
                    distance = distance / (obj.trace.Count + template.trace.Count);
                    if (distance < inf) { result = i; best_template = template; inf = distance; }
                }
                return 0;
            }
        
            //Train classifier by given training
            public int train(L obj){
                return 0;
            }

        }
}
