﻿using Game.ExtensionMethods;
using Game.GeneticAlgorithm;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Helpers
{
    /// <summary>
    /// A set of generic static helper methods for 2 objective optimisation problems.
    /// </summary>
    public static class MultiObjectiveHelper
    {
        /// <summary>
        /// Update Population Fitness - Takes in a population of individuals, and calculates their 
        /// rank and crowding distances.
        /// </summary>
        /// <param name="population">The set of individuals to have their rank and crowding distance calculated.</param>
        public static void UpdatePopulationFitness(List<Individual> population)
        {
            // Clear the existing ranks and crowding distances
            foreach (var individual in population)
            {
                individual.Rank = -1;
                individual.CrowdingDistance = -1;
            }

            NormalizeFitnessValues(population);

            CalculateRank(population);

            // For each rank, calculate the crowdding distance for each individual
            var ranks = population.GroupBy(p => p.Rank);
            foreach (var singleRank in ranks)
            {
                CalculateCrowdingDistance(singleRank);
            }
        }

        private static void CalculateRank(List<Individual> population)
        {
            var currentFront = new List<Individual>();
            var individualsDominated = new Dictionary<Individual, List<Individual>>();
            var individualDominationCount = new Dictionary<Individual, int>();

            foreach (var individualA in population)
            {
                individualsDominated.Add(individualA, new List<Individual>());
                individualDominationCount.Add(individualA, 0);

                foreach (var individualB in population)
                {
                    if(individualA == individualB)
                    {
                        continue;
                    }

                    if (Dominates(individualB, individualA))
                    {
                        individualDominationCount[individualA]++;
                    }
                    else if (Dominates(individualA, individualB))
                    {
                        individualsDominated[individualA].Add(individualB);
                    }
                }

                if (individualDominationCount[individualA] == 0)
                {
                    individualA.Rank = 0;
                    currentFront.Add(individualA);
                }
            }

            var i = 0;
            while (currentFront.Any())
            {
                var nextFront = new List<Individual>();
                foreach(var individualA in currentFront)
                {
                    foreach(var individualB in individualsDominated[individualA])
                    {
                        individualDominationCount[individualB]--;
                        if(individualDominationCount[individualB] == 0)
                        {
                            individualB.Rank = i + 1;
                            nextFront.Add(individualB);
                        }

                    }
                }
                i++;

                currentFront = nextFront;
            }
        }

        public static bool Dominates(Individual a, Individual b)
        {
            if (a.DistanceFitness < b.DistanceFitness &&
                a.TimeFitness < b.TimeFitness )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculations for crowsing distance must be done on normalized fitness values to stop
        /// biasing towards an objective with a larger absolute fitness value.
        /// </summary>
        /// <param name="population"></param>
        private static void NormalizeFitnessValues(List<Individual> population)
        {
            var maxDistance = population.Max(i => i.DistanceFitness);
            var maxTime = population.Max(i => i.TimeFitness);

            population.ForEach(i => i.NormalizedDistanceFitness = i.DistanceFitness / maxDistance);
            population.ForEach(i => i.NormalizedTimeFitness = i.TimeFitness / maxTime);
        }

        private static void CalculateCrowdingDistance(IGrouping<int, Individual> singleRank)
        {
            // As we only have two objectives, ordering individuals along one front allows us to make assumptions
            // about the locations of the neighbouring individuals in the array.
            var orderedIndividuals = singleRank.Select(i => i).OrderBy(i => i.NormalizedDistanceFitness).ToArray();
            var individualsInFront = orderedIndividuals.Count();

            for (int i = 0; i < individualsInFront; i++)
            {
                // If we are at the start or end of a front, it should have infinite crowding distance
                if (i == 0 || i == individualsInFront - 1)
                {
                    orderedIndividuals[i].CrowdingDistance = double.PositiveInfinity;
                }
                else
                {
                    // Grab a reference to each individual to make the next section a bit cleaner.
                    var current = orderedIndividuals[i];
                    var left = orderedIndividuals[i - 1];
                    var right = orderedIndividuals[i + 1];

                    // Get the positions on the 2D fitness graph, where time is our X axis and distance is our Y.
                    var currentPosition = new Vector2f(current.NormalizedTimeFitness, current.NormalizedDistanceFitness);
                    var leftPosition = new Vector2f(left.NormalizedTimeFitness, left.NormalizedDistanceFitness);
                    var rightPosition = new Vector2f(right.NormalizedTimeFitness, right.NormalizedDistanceFitness);

                    // Calculate the distance to the neighbourn on each side
                    var distanceLeft = currentPosition.Distance(leftPosition);
                    var distanceRight = currentPosition.Distance(rightPosition);

                    // Set the crowding distance for the current individual
                    orderedIndividuals[i].CrowdingDistance = Math.Pow(distanceLeft + distanceRight, 2);
                }
            }
        }

        private static bool IsNotDominated(Individual individualA, List<Individual> remainingToBeRanked)
        {
            // Loop over each individual and check if it dominates this individual.
            foreach (var individualB in remainingToBeRanked)
            {
                if (individualA == individualB)
                {
                    continue;
                }

                // If this individual is at least better than us in one objective and equal in another,
                // then we are dominated by this individual
                if (individualB.DistanceFitness <= individualA.DistanceFitness &&
                    individualB.TimeFitness <= individualA.TimeFitness)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
