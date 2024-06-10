using Game.ExtensionMethods;
using Game.GeneticAlgorithm;
using Game.Helpers;
using Game.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Game.Tests
{
    [TestClass]
    public class GenerationTests
    {
        [TestInitialize]
        public void SetUp()
        {
            TownHelper.Initialize();
        }

        [TestMethod]
        public void GenerateIndividualTest()
        {
            var individual = WorldHelper.GenerateIndividual(Configuration.TownCount);

            // Ensure that individual contains no repeated values
            var uniqueValueGroups = individual.Sequence.GroupBy(s => s);
            Assert.IsTrue(uniqueValueGroups.All(g => g.Count() == 1), "The individual contains repeated values.");
        }

        [TestMethod]
        public void EnsureCandidateParentsUniqueTest()
        {
            var population = DefaultPopulationHelper.GetTestPopulation();

            for (int i = 0; i < 10; i++)
            {
                var (candidateA, candidateB) = WorldHelper.GetCandidateParents(population);

                Assert.IsFalse(candidateA.Sequence.SequenceEqual(candidateB.Sequence), "Candidate parents are not unique.");
            }
        }

        [TestMethod]
        public void EnsureRankedTournamentSelectionTest()
        {
            var population = DefaultPopulationHelper.GetTestPopulation();

            MultiObjectiveHelper.UpdatePopulationFitness(population);

            // Rank 1
            var individualA = population[1];

            // Rank 2
            var individualD = population[3];

            var fitterIndividualA = WorldHelper.TournamentSelection(individualA, individualD);
            var fitterIndividualD = WorldHelper.TournamentSelection(individualD, individualA);

            Assert.AreEqual(individualA, fitterIndividualA, "Tournament selection did not select the fitter individual.");
            Assert.AreEqual(individualA, fitterIndividualD, "Tournament selection did not select the fitter individual.");
        }

        [TestMethod]
        public void EnsureCrowdingDistanceTournamentSelectionTest()
        {
            var population = DefaultPopulationHelper.GetTestPopulation();

            MultiObjectiveHelper.UpdatePopulationFitness(population);

            // Rank 1, float.MaxValue crowding distance
            var individualA = population[0];

            // Rank 1, ~5.65 crowding distance
            var individualB = population[1];

            var fitterIndividualA = WorldHelper.TournamentSelection(individualA, individualB);
            var fitterIndividualB = WorldHelper.TournamentSelection(individualB, individualA);

            Assert.AreEqual(individualA, fitterIndividualA, "Tournament selection did not select the individual with higher crowding distance.");
            Assert.AreEqual(individualA, fitterIndividualB, "Tournament selection did not select the individual with higher crowding distance.");
        }

        [TestMethod]
        public void EnsureCrossoverTest()
        {
            var individualA = new Individual(new List<int> { 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 });
            var individualB = new Individual(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var crossoverPointA = 3;
            var crossoverPointB = 1;
            var crossoverPointC = 8;

            var childA = WorldHelper.DoCrossover(individualA, individualB, crossoverPointA);
            var childB = WorldHelper.DoCrossover(individualA, individualB, crossoverPointB);
            var childC = WorldHelper.DoCrossover(individualA, individualB, crossoverPointC);

            var expectedChildASequence = new List<int> { 0, 9, 1, 2, 3, 4, 5, 6, 7, 8 };
            var expectedChildBSequence = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var expectedChildCSequence = new List<int> { 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 };

            Assert.IsTrue(childA.Sequence.SequenceEqual(expectedChildASequence), "Child A sequence does not match the expected sequence.");
            Assert.IsTrue(childB.Sequence.SequenceEqual(expectedChildBSequence), "Child B sequence does not match the expected sequence.");
            Assert.IsTrue(childC.Sequence.SequenceEqual(expectedChildCSequence), "Child C sequence does not match the expected sequence.");

            Assert.AreEqual(childA.Sequence.Count, individualA.Sequence.Count, "Child A sequence length does not match the original.");
            Assert.AreEqual(childB.Sequence.Count, individualA.Sequence.Count, "Child B sequence length does not match the original.");
            Assert.AreEqual(childC.Sequence.Count, individualA.Sequence.Count, "Child C sequence length does not match the original.");
        }

        [TestMethod]
        public void EnsureUniqueTownsTest()
        {
            var sequence = new List<int> { 0, 9, 1, 2, 3, 4, 5, 6, 7, 8 };

            for (int i = 0; i < 10; i++)
            {
                var (townA, townB) = WorldHelper.GetUniqueTowns(sequence);

                Assert.AreNotEqual(townA, townB, "Unique towns are not unique.");
            }
        }

        [TestMethod]
        public void EnsureRotationMutationResultTest()
        {
            var individual = new Individual(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var result = WorldHelper.DoRotateMutate(individual);

            // Ensure that all values are still in the list
            var distinctResultCount = result.Sequence.Distinct().Count();
            Assert.AreEqual(distinctResultCount, 10, "Rotation mutation resulted in duplicate or missing values.");

            // Ensure that there are no duplicate entries
            var sequences = result.Sequence.GroupBy(s => s);
            Assert.IsTrue(sequences.All(s => s.Count() == 1), "Rotation mutation resulted in duplicate values.");
        }

        [TestMethod]
        public void EnsureSwapMutationResultTest()
        {
            var individual = new Individual(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var result = WorldHelper.DoSwapMutate(individual);

            // Ensure that all values are still in the list
            var distinctResultCount = result.Sequence.Distinct().Count();
            Assert.AreEqual(distinctResultCount, 10, "Swap mutation resulted in duplicate or missing values.");

            // Ensure that there are no duplicate entries
            var sequences = result.Sequence.GroupBy(s => s);
            Assert.IsTrue(sequences.All(s => s.Count() == 1), "Swap mutation resulted in duplicate values.");

            // Perform manual swap to ensure result is correct
            var firstIndex = -1;
            var lastIndex = -1;
            for (int i = 0; i < individual.Sequence.Count; i++)
            {
                if (firstIndex == -1 && result.Sequence[i] != individual.Sequence[i])
                {
                    firstIndex = i;
                    continue;
                }

                if (firstIndex != -1 && result.Sequence[i] != individual.Sequence[i])
                {
                    lastIndex = i;
                    break;
                }
            }

            var originalSequence = individual.Sequence.ToList();
            originalSequence.SwapInPlace(firstIndex, lastIndex);

            Assert.IsTrue(originalSequence.SequenceEqual(result.Sequence), "Swap mutation did not produce the expected sequence.");
        }
    }
}
