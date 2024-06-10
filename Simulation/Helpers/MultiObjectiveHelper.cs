using Game.ExtensionMethods;
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

            // For each rank, calculate the crowding distance for each individual
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
                    if (individualA == individualB)
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
                foreach (var individualA in currentFront)
                {
                    foreach (var individualB in individualsDominated[individualA])
                    {
                        individualDominationCount[individualB]--;
                        if (individualDominationCount[individualB] == 0)
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
            return (a.DistanceFitness < b.DistanceFitness && a.TimeFitness <= b.TimeFitness) ||
                   (a.DistanceFitness <= b.DistanceFitness && a.TimeFitness < b.TimeFitness);
        }

        /// <summary>
        /// Calculations for crowding distance must be done on normalized fitness values to stop
        /// biasing towards an objective with a larger absolute fitness value.
        /// </summary>
        /// <param name="population"></param>
        private static void NormalizeFitnessValues(List<Individual> population)
        {
            var maxDistance = population.Max(i => i.DistanceFitness);
            var maxTime = population.Max(i => i.TimeFitness);

            population.ForEach(i =>
            {
                i.NormalizedDistanceFitness = maxDistance == 0 ? 0 : i.DistanceFitness / maxDistance;
                i.NormalizedTimeFitness = maxTime == 0 ? 0 : i.TimeFitness / maxTime;
            });
        }

        private static void CalculateCrowdingDistance(IGrouping<int, Individual> singleRank)
        {
            var orderedByDistance = singleRank.OrderBy(i => i.NormalizedDistanceFitness).ToArray();
            var orderedByTime = singleRank.OrderBy(i => i.NormalizedTimeFitness).ToArray();
            var individualsInFront = orderedByDistance.Length;

            foreach (var individual in singleRank)
            {
                individual.CrowdingDistance = 0;
            }

            if (individualsInFront > 0)
            {
                orderedByDistance[0].CrowdingDistance = double.PositiveInfinity;
                orderedByDistance[individualsInFront - 1].CrowdingDistance = double.PositiveInfinity;

                orderedByTime[0].CrowdingDistance = double.PositiveInfinity;
                orderedByTime[individualsInFront - 1].CrowdingDistance = double.PositiveInfinity;
            }

            for (int i = 1; i < individualsInFront - 1; i++)
            {
                double distanceFitnessDifference = (orderedByDistance[i + 1].NormalizedDistanceFitness - orderedByDistance[i].NormalizedDistanceFitness) +
                                                   (orderedByDistance[i].NormalizedDistanceFitness - orderedByDistance[i - 1].NormalizedDistanceFitness);
                orderedByDistance[i].CrowdingDistance += distanceFitnessDifference;
            }

            for (int i = 1; i < individualsInFront - 1; i++)
            {
                double timeFitnessDifference = (orderedByTime[i + 1].NormalizedTimeFitness - orderedByTime[i].NormalizedTimeFitness) +
                                               (orderedByTime[i].NormalizedTimeFitness - orderedByTime[i - 1].NormalizedTimeFitness);
                orderedByTime[i].CrowdingDistance += timeFitnessDifference;
            }
        }
    }
}
