using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classification
{
    public class RandomProjection   
    {
        //At first, define random projection matrix
        double[,] rp;

        //Dimension of random projection, dimension of 
        private int d, k;

        Random rand = new Random();

        public RandomProjection(int dimension) { 
            rp = new double[dimension,3];
            this.d = dimension;

            //Construct projection matrix with distributionin respect to Achlioptas, MS Research
            //http://people.ee.duke.edu/~lcarin/p93.pdf
            for (int i = 0; i < d; ++i) { 
                for (int j = 0; j < /* feature vector dimension */; ++j){
                    double r = rand.Next(6);
                    if (r < 4) rp[i,j] = 0; 
                    else if (r < 5) rp[i,j] = Math.Sqrt(3);
                    else rp[i,j] = -Math.Sqrt(3);
                }
            }
        }

        public void do_projection(out double[,] projected_space){
            projected_space = new double[,];
            for (int i = 0; i < d; ++i) { 
                for (int j = 0; j < /* number of feature vectors */; ++j){
                    for (int k = 0; k < /* feature vector dimension */; ++k){
                        projected_space[i, j] += rp[i,k] * vectors[k, j];
                    }
                }
            }
            return;
        }



    }
}
