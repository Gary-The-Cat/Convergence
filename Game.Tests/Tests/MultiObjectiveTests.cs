using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Helpers;
using Game.Tests.Helpers;
using System;

namespace Game.Tests
{
    [TestClass()]
    public class MultiObjectiveTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void EnsureRankTest()
        {
            var population = DefaultPopulationHelper.GetTestPopulation();

            MultiObjectiveHelper.UpdatePopulationFitness(population);

            Assert.AreEqual(population[0].Rank, 0);
            Assert.AreEqual(population[1].Rank, 0);
            Assert.AreEqual(population[2].Rank, 0);

            Assert.AreEqual(population[3].Rank, 1);
            Assert.AreEqual(population[4].Rank, 1);
            Assert.AreEqual(population[5].Rank, 1);
            
            Assert.AreEqual(population[6].Rank, 2);
        }

        [TestMethod()]
        public void EnsureCrowdingDistanceTest()
        {
            double epsilon = 0.00001;
            var population = DefaultPopulationHelper.GetTestPopulation();

            MultiObjectiveHelper.UpdatePopulationFitness(population);

            Assert.IsTrue(double.IsPositiveInfinity(population[0].CrowdingDistance));
            Assert.IsTrue(Math.Abs(population[1].CrowdingDistance - 1.333333) < epsilon);
            Assert.IsTrue(double.IsPositiveInfinity(population[2].CrowdingDistance));

            Assert.IsTrue(double.IsPositiveInfinity(population[3].CrowdingDistance));
            Assert.IsTrue(Math.Abs(population[4].CrowdingDistance - 1.333333) < epsilon);
            Assert.IsTrue(double.IsPositiveInfinity(population[5].CrowdingDistance));

            Assert.IsTrue(double.IsPositiveInfinity(population[6].CrowdingDistance));
        }
    }
}

