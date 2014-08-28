﻿using SharpLearning.Containers.Views;
using SharpLearning.DecisionTrees.ImpurityCalculators;
using System;

namespace SharpLearning.DecisionTrees.SplitSearchers
{
    public sealed class RandomSplitSearcher : ISplitSearcher
    {
        readonly int m_minimumSplitSize;
        readonly double m_minimumLeafWeight;
        readonly Random m_random;

        /// <summary>
        /// Select a random split between the min and max range of the feature within the parent interval.
        /// The implementation assumes that the features and targets have been sorted
        /// together using the features as sort criteria
        /// </summary>
        /// <param name="minimumSplitSize">The minimum size for a node to be split</param>
        public RandomSplitSearcher(int minimumSplitSize, int seed)
            : this(minimumSplitSize, 0.0, seed)
        {
        }

        /// <summary>
        /// Select a random split between the min and max range of the feature within the parent interval.
        /// The implementation assumes that the features and targets have been sorted
        /// together using the features as sort criteria
        /// </summary>
        /// <param name="minimumSplitSize">The minimum size for a node to be split</param>
        public RandomSplitSearcher(int minimumSplitSize, double minimumLeafWeight, int seed)
        {
            if (minimumSplitSize <= 0) { throw new ArgumentException("minimum split size must be larger than 0"); }
            m_minimumSplitSize = minimumSplitSize;
            m_minimumLeafWeight = minimumLeafWeight;
            m_random = new Random(seed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="impurityCalculator"></param>
        /// <param name="feature"></param>
        /// <param name="targets"></param>
        /// <param name="parentInterval"></param>
        /// <param name="parentImpurity"></param>
        /// <returns></returns>
        public SplitResult FindBestSplit(IImpurityCalculator impurityCalculator, double[] feature, double[] targets, Interval1D parentInterval, double parentImpurity)
        {
            var min = double.MaxValue;
            var max = double.MinValue;
            
            for (int i = parentInterval.FromInclusive; i < parentInterval.ToExclusive; i++)
            {
                var value = feature[i];
                
                if(value < min)
                {
                    min = value;
                }
                else if(value > max)
                {
                    max = value;
                }
            }

            if(min == max)
            {
                return SplitResult.Initial();
            }

            var threshold = RandomThreshold(min, max);

            if (threshold == max)
            {
                threshold = min;
            }

            var splitIndex = -1;
            var impurityImprovement = 0.0;
            var impurityLeft = 0.0;
            var impurityRight = 0.0;
            
            var currentFeature = double.MinValue;

            for (int i = parentInterval.FromInclusive; i < parentInterval.ToExclusive; i++)
            {      
                currentFeature = feature[i];
                if(currentFeature > threshold)
                {
                    splitIndex = i;
                    
                    impurityCalculator.UpdateInterval(parentInterval);
                    impurityCalculator.UpdateIndex(i);
                    impurityImprovement = impurityCalculator.ImpurityImprovement(parentImpurity);
                    
                    var childImpurities = impurityCalculator.ChildImpurities();
                    impurityLeft = childImpurities.Left;
                    impurityRight = childImpurities.Right;

                    break;
                }
            }

            return new SplitResult(splitIndex, threshold, impurityImprovement,
                impurityLeft, impurityRight);
        }

        public double RandomThreshold(double min, double max)
        {
            return m_random.NextDouble() * (max - min) + min;
        }
    }
}