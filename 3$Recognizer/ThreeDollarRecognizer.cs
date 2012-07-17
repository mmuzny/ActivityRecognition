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
        private static readonly Func<M, M, double> Sub;

        static ThreeDollarRecognizer() {
            var firstOperand = Expression.Parameter(typeof(M), "i");
            var secondOperand = Expression.Parameter(typeof(M), "j");
            var body = Expression.Subtract(firstOperand, secondOperand);
            Sub = Expression.Lambda<Func<M, M, double>>
                  (body, firstOperand, secondOperand).Compile();
        }


        private double distance(Unistroke<M>.Point3<M> i,Unistroke<M>.Point3<M> j){
            return Math.Sqrt(Math.Pow(Sub(i.x, j.x), 2) + Math.Pow(Sub(i.y, j.y), 2) + Math.Pow(Sub(i.z, j.z), 2)); 
        }

        private double path_length(ref Unistroke<M> unistroke) {
            for (int i = 0; i < unistroke.Length(); i++) { 
                                       
            }

            return 0;
        }

        public void resample(T obj)
        {
            
        }
       
        public int classify(T obj, out int result){
            result = 0;
            return 0;
        }

        public int train(L obj){
            
            return 0;
        }
    }
}
