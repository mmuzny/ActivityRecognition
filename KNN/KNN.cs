using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalProcessing;

namespace Classification
{
    public class KNN<T,L,M> : Classifier<T,L>  where T : SignalProcessing.Unistroke<M>
                                               where L : ActivitySet<M>, GestureSet<M>
    {
        public KNN() { 
            
        }

        public int classify() { 
            
        }
    }
}
