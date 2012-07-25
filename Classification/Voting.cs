using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classification
{
    class Voting<T,L> : PostProcessing<T,L>
    {
        public int postprocess(ref Classifier<T, L>[] obj, T f, out int result) {
            int[] results = new int[obj.Length]; int i = 0;

            foreach (Classifier<T, L> classifer in obj) {
                 classifer.classify(f, out results[i++]);
            }
            var res = from numbers in results
                    group numbers by numbers into grouped
                    select new { Number = grouped.Key, Freq = grouped.Count() };
           
            return 0;
        }

        public int postprocess(Classifier<T, L>[] obj, L f_s, T f, out int result) {
            int[] results = new int[obj.Length]; int i = 0;
            
            foreach (Classifier<T, L> classifer in obj) {
                 classifer.train(f_s);
                 classifer.classify(f, out results[i++]);
            }
            var res = from numbers in results
                    group numbers by numbers into grouped
                    select new { Number = grouped.Key, Freq = grouped.Count() };
            return 0;
        }
    }
}
