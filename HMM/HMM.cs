using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classifier
{
    public class HiddenMarkovModel
    {
        private double[] probabilities;
        private double[,] emissionMatrix;
        private double[,] transitionMatrix;
        private int states;
        private int alphabet_size;

        private HiddenMarkovModel(int states, int alphabet_size) {
            this.states = states;
            this.alphabet_size = alphabet_size;
            this.probabilities = new double[this.states];
            this.emissionMatrix = new double[this.states, this.alphabet_size];
            this.transitionMatrix = new double[this.states, this.states];
        }

        public double evaluate(int[] observations, bool logarithm)
        {
            if (observations == null)
                throw new ArgumentNullException("Observations missing");

            if (observations.Length == 0)
                return 0.0;

            double likelihood = 0;
            double[] coefficients;

            forward(observations, out coefficients);

            for (int i = 0; i < coefficients.Length; i++)
                likelihood += Math.Log(coefficients[i]);

            return logarithm ? likelihood : Math.Exp(likelihood);
        }

        private double[,] forward(int[] observations, out double[] c)
        {
            if (observations == null)
                throw new ArgumentNullException("Observations missing");

            double[,] fwd = new double[observations.Length, states];
            c = new double[observations.Length];

            for (int i = 0; i < states; i++)
                c[0] += fwd[0, i] = probabilities[i] * emissionMatrix[i, observations[0]];

            if (c[0] != 0)
            {
                for (int i = 0; i < states; i++)
                    fwd[0, i] = fwd[0, i] / c[0];
            }

            for (int t = 1; t < observations.Length; t++)
            {
                for (int i = 0; i < states; i++)
                {
                    double p = emissionMatrix[i, observations[t]];

                    double sum = 0.0;
                    for (int j = 0; j < states; j++)
                        sum += fwd[t - 1, j] * transitionMatrix[j, i];
                    fwd[t, i] = sum * p;

                    c[t] += fwd[t, i];
                }

                if (c[t] != 0)
                {
                    for (int i = 0; i < states; i++)
                        fwd[t, i] = fwd[t, i] / c[t];
                }
            }

            return fwd;
        }

        public int[] decode(int[] observations, out double probability)
        {

            int T = observations.Length;
            int minState;
            double minWeight;
            double weight;

            int[,] s = new int[states, T];
            double[,] a = new double[states, T];

            for (int i = 0; i < states; i++)
            {
                a[i, 0] = (-1.0 * System.Math.Log(probabilities[i])) - System.Math.Log(emissionMatrix[i, observations[0]]);
            }

            for (int t = 1; t < T; t++)
            {
                for (int j = 0; j < states; j++)
                {
                    minState = 0;
                    minWeight = a[0, t - 1] - System.Math.Log(transitionMatrix[0, j]);

                    for (int i = 1; i < states; i++)
                    {
                        weight = a[i, t - 1] - System.Math.Log(transitionMatrix[i, j]);

                        if (weight < minWeight)
                        {
                            minState = i;
                            minWeight = weight;
                        }
                    }

                    a[j, t] = minWeight - System.Math.Log(emissionMatrix[j, observations[t]]);
                    s[j, t] = minState;
                }
            }


            minState = 0;
            minWeight = a[0, T - 1];

            for (int i = 1; i < states; i++)
            {
                if (a[i, T - 1] < minWeight)
                {
                    minState = i;
                    minWeight = a[i, T - 1];
                }
            }

            int[] path = new int[T];
            path[T - 1] = minState;

            for (int t = T - 2; t >= 0; t--)
                path[t] = s[path[t + 1], t + 1];


            probability = System.Math.Exp(-minWeight);
            return path;
        }


        public double learn(int[][] observations, int iterations, double tolerance)
        {
            if (iterations == 0 && tolerance == 0)
                throw new ArgumentException("Iterations and limit cannot be both zero.");

            int N = observations.Length;
            int currentIteration = 1;
            bool stop = false;

            double[] pi = probabilities;
            double[,] A = transitionMatrix;

            double[][, ,] epsilon = new double[N][, ,];
            double[][,] gamma = new double[N][,];

            for (int i = 0; i < N; i++)
            {
                int T = observations[i].Length;
                epsilon[i] = new double[T, states, states];
                gamma[i] = new double[T, states];
            }

            double oldLikelihood = Double.MinValue;
            double newLikelihood = 0;


            do
            {
                for (int i = 0; i < N; i++)
                {
                    var sequence = observations[i];
                    int T = sequence.Length;
                    double[] scaling;

                    double[,] fwd = forward(observations[i], out scaling);
                    double[,] bwd = backward(observations[i], scaling);

                    for (int t = 0; t < T; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < states; k++)
                            s += gamma[i][t, k] = fwd[t, k] * bwd[t, k];

                        if (s != 0)
                        {
                            for (int k = 0; k < states; k++)
                                gamma[i][t, k] /= s;
                        }
                    }

                    for (int t = 0; t < T - 1; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < states; k++)
                            for (int l = 0; l < states; l++)
                                s += epsilon[i][t, k, l] = fwd[t, k] * transitionMatrix[k, l] * bwd[t + 1, l] * emissionMatrix[l, sequence[t + 1]];

                        if (s != 0)
                        {
                            for (int k = 0; k < states; k++)
                                for (int l = 0; l < states; l++)
                                    epsilon[i][t, k, l] /= s;
                        }
                    }

                    for (int t = 0; t < scaling.Length; t++)
                        newLikelihood += Math.Log(scaling[t]);
                }


                newLikelihood /= observations.Length;

                if (checkConvergence(oldLikelihood, newLikelihood,
                    currentIteration, iterations, tolerance))
                {
                    stop = true;
                }
                else
                {
                    currentIteration++;
                    oldLikelihood = newLikelihood;
                    newLikelihood = 0.0;

                    for (int k = 0; k < states; k++)
                    {
                        double sum = 0;
                        for (int i = 0; i < N; i++)
                            sum += gamma[i][0, k];
                        pi[k] = sum / N;
                    }

                    for (int i = 0; i < states; i++)
                    {
                        for (int j = 0; j < states; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < N; k++)
                            {
                                int T = observations[k].Length;

                                for (int l = 0; l < T - 1; l++)
                                    num += epsilon[k][l, i, j];

                                for (int l = 0; l < T - 1; l++)
                                    den += gamma[k][l, i];
                            }

                            A[i, j] = (den != 0) ? num / den : 0.0;
                        }
                    }

                    for (int i = 0; i < states; i++)
                    {
                        for (int j = 0; j < alphabet_size; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < N; k++)
                            {
                                int T = observations[k].Length;

                                for (int l = 0; l < T; l++)
                                {
                                    if (observations[k][l] == j)
                                        num += gamma[k][l, i];
                                }

                                for (int l = 0; l < T; l++)
                                    den += gamma[k][l, i];
                            }

                            emissionMatrix[i, j] = (num == 0) ? 1e-10 : num / den;
                        }
                    }

                }

            } while (!stop);

            return newLikelihood;
        }

    }
}
