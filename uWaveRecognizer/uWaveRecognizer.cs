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
                                                              where L : GestureSet<M>
        {   
            //Table for computation distances between nodes 
            private double[,] warping_table;

            //Step of moving the window
            private static dynamic moving_step = 1;

            //Sliding window size
            private static dynamic window_size = 4;

            //Templates list
            private static L templates;

            //Expression tree build lambda function for subtraction
            private static readonly Func<M, M, M> Sub, Add, Mul, Div;
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

            public uWaveRecognizer() { }

            //Unroll points set in with respect to defined window_size and moving_step
            private void unroll_points(ref T uni, out T unrolled)
            {
                int i = 0; unrolled = (T) new Unistroke<M>();
                while (i < uni.trace.Count) {
                    Unistroke<M>.Point3<M> p = new Unistroke<M>.Point3<M>();
                    if ((i + window_size) > uni.trace.Count) window_size = uni.trace.Count - i;
                    for (int j = i; j < i + window_size; j++) {
                        p.x = Add(p.x, uni[i].x);
                        p.y = Add(p.y, uni[i].y);
                        p.z = Add(p.z, uni[i].z);
                    }
                    p.x = Div(p.x, window_size);
                    p.y = Div(p.y, window_size);
                    p.z = Div(p.z, window_size);
                    unrolled.trace.Add(p); i+=(int) moving_step;
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
            private void quantize_points(ref T uni)
            {
                for (int i = 0; i < uni.trace.Count; i++) {
                    distraction_table(ref uni[i].x);
                    distraction_table(ref uni[i].y);
                    distraction_table(ref uni[i].z);
                }                            
            }

            //Compute distance between 2 points
            private double compute_distance(Unistroke<M>.Point3<M> a, Unistroke<M>.Point3<M> b) {
                return Math.Pow((double) (object) Sub(a.x, b.x), 2) + Math.Pow((double) (object)Sub(a.y, b.y), 2) + Math.Pow((double) (object)Sub(a.z, b.z), 2);
            }

            //Iterative Dynamic Time Warping function to compute table values
            private void dtw(ref T obj,ref T obj_2, int index_1, int index_2, out double distance)
            {
                for (int i = 0; i < index_1; i++) {
                    for (int j = 0; j < index_2; j++) {
                        if (i == 0 && j == 0) {
                            warping_table[i, j] = compute_distance(obj[0], obj_2[0]);
                        }
                        else if (i == 0)
                        {
                            warping_table[i, j] = compute_distance(obj[i], obj_2[j]) + warping_table[i, j - 1];
                        }
                        else if (j == 0)
                        {
                            warping_table[i, j] = compute_distance(obj[i], obj_2[j]) + warping_table[i - 1, j];
                        }
                        else 
                        {
                            warping_table[i, j] = compute_distance(obj[i], obj_2[j]) +
                                                  Math.Min(warping_table[i - 1, j - 1], Math.Min(warping_table[i,j-1], warping_table[i-1, j]));
                        }
                    }                
                }
                distance = warping_table[index_1-1, index_2-1];
            }

            //Classify feature vector according to training structures
            public int classify(T obj, out int result)
            {
                double inf = double.PositiveInfinity; T best_template = null; result = 0;
                T obj_unrolled, template_unrolled;
                unroll_points(ref obj, out obj_unrolled);
                quantize_points(ref obj_unrolled);
                for(int i = 0; i < templates.gestures.Count; i++){
                    double distance;
                    T template =(T) (object) templates.gestures[i].unistrokes[0];
                    unroll_points(ref template, out template_unrolled);
                    quantize_points(ref template_unrolled);
                    warping_table = new double[obj_unrolled.trace.Count, template_unrolled.trace.Count];
                    dtw(ref obj_unrolled, ref template_unrolled, obj_unrolled.trace.Count, template_unrolled.trace.Count, out distance);
                    distance = distance / (obj_unrolled.trace.Count + template_unrolled.trace.Count);
                    System.Console.WriteLine("Distance template " + i + " " +distance);
                    if (distance < inf) { result = i; best_template = template; inf = distance; }
                }
                return 0;
            }
        
            //Train classifier by given training set
            public int train(L obj){
                templates = obj;
                return 0;
            }

        }
}
