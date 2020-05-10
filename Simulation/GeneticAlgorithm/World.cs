using Game.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.GeneticAlgorithm
{
    public class World
    {
        public const int PopulationCount = 1000;

        public List<double> FitnessOverTime { get; private set; }

        private int generationCount;

        private int noImprovementCount;

        private double previousFitness;

        public bool HasConverged => 
               generationCount > GAConfig.MaxGenerations
            || noImprovementCount > GAConfig.MaxNoImprovement;

        private static List<double> cumulativeProportions;
        private static Random random = new Random();

        public List<Individual> Population { get; set; }

        public World()
        {
            Population = new List<Individual>();
            cumulativeProportions = new List<double>();
            FitnessOverTime = new List<double>();

            generationCount = 0;
            noImprovementCount = 0;
            previousFitness = double.MaxValue;
        }

        public void Spawn()
        {
            // Generate {PopulationCount} individuals
            for(int i = 0; i < PopulationCount; i++)
            {
                this.Population.Add(GenerateIndividual());
            }
        }

        private Individual GenerateIndividual()
        {
            // Generate a list of numbers [0, 1, 2, 3... 9]
            var sequence = Enumerable.Range(0, Configuration.TownCount).ToList();

            // Randomly shuffle the list [3, 1, 5, 9... 4]
            sequence.Shuffle();

            // Create a new individual with our random sequence
            return new Individual(sequence);
        }

        public void DoGeneration()
        {
            this.generationCount++;

            // We are about t 
            this.UpdateCumulativeProportions();

            // Create a list to hold our new offspring
            var offspring = new List<Individual>();

            // While our offspring are less than our current population count, create new offspring
            while (offspring.Count < PopulationCount)
            {
                // Get parents
                var mother = GetParent();
                var father = GetParent();

                // Handle the case where we have picked the same individual as both parents
                while (mother == father)
                {
                    father = GetParent();
                }

                // Perform Crossover
                var (offspringA, offspringB) = GetOffspring(mother, father);

                // Mutate
                (offspringA, offspringB) = Mutate(offspringA, offspringB);

                // Add offspring to population
                offspring.Add(offspringA);
                offspring.Add(offspringB);
            }

            // Add all the offspring to our existing population
            Population.AddRange(offspring);

            // Take the best 'PopulationCount' worth of individuals
            Population = Population.OrderBy(i => i.GetFitness()).Take(PopulationCount).ToList();

            // Grab the fittest individual in the population
            var bestIndividualFitness = Population.First().GetFitness();

            // Record this fitness value
            FitnessOverTime.Add(bestIndividualFitness);

            if(previousFitness == bestIndividualFitness)
            {
                noImprovementCount++;
            }
            else
            {
                previousFitness = bestIndividualFitness;
                noImprovementCount = 0;
            }
        }

        private (Individual offspringA, Individual offspringB) Mutate(Individual offspringA, Individual offspringB)
        {
            var newOffspringA = new Individual(offspringA.Sequence);
            var newOffspringB = new Individual(offspringB.Sequence);

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                newOffspringA = Mutate(offspringA);
            }

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                newOffspringB = Mutate(offspringB);
            }

            return (newOffspringA, newOffspringB);
        }

        private Individual Mutate(Individual offspringA)
        {
            if (random.NextDouble() > 0.5)
            {
                return SwapMutate(offspringA);
            }
            else
            {
                // do rotate mutate
                return RotateMutate(offspringA);
            }
        }

        private Individual SwapMutate(Individual individual)
        {
            var sequence = individual.Sequence.ToList();

            var (townA, townB) = GetUniqueTowns();

            sequence.SwapInPlace(townA, townB);

            return new Individual(sequence);
        }

        private Individual RotateMutate(Individual individual)
        {
            var (townA, townB) = GetUniqueTowns();

            var firstIndex = townA < townB ? townA : townB;
            var secondIndex = townA < townB ? townB : townA;

            var newSequence = individual.Sequence.Take(firstIndex).ToList();
            var middle = individual.Sequence.Skip(firstIndex).Take(secondIndex - firstIndex).Reverse();
            var tail = individual.Sequence.Skip(secondIndex);

            newSequence.AddRange(middle);
            newSequence.AddRange(tail);

            return new Individual(newSequence);
        }

        private (int, int) GetUniqueTowns()
        {
            var townA = random.Next(Configuration.TownCount);
            var townB = random.Next(Configuration.TownCount);

            while (townB == townA)
            {
                townB = random.Next(Configuration.TownCount);
            }

            return (townA, townB);
        }

        private (Individual, Individual) GetOffspring(Individual individualA, Individual individualB)
        {
            // Generate the offspring from our selected parents
            var offspringA = DoCrossover(individualA, individualB);
            var offspringB = DoCrossover(individualB, individualA);

            return (offspringA, offspringB);
        }

        private Individual DoCrossover(Individual individualA, Individual individualB)
        {
            // Generate a number between 1 and sequence length - 1 to be our crossover position
            var crossoverPosition = random.Next(1, individualA.Sequence.Count - 1);

            // Grab the head from the first individual
            var offspringSequence = individualA.Sequence.Take(crossoverPosition).ToList();

            // Create a hash for quicker 'exists in head' checks
            var appeared = offspringSequence.ToHashSet();

            // Append individualB to the head, skipping any values that have already shown up in the head
            foreach (var town in individualB.Sequence)
            {
                if (appeared.Contains(town))
                {
                    continue;
                }

                offspringSequence.Add(town);
            }

            // Return our new offspring!
            return new Individual(offspringSequence);
        }

        private Individual GetParent()
        {
            if (random.NextDouble() > 0.5)
            {
                //Tournament
                return TournamentSelection();
            }
            else
            {
                //Biased random
                return BiasedRandomSelection();
            }
        }

        private Individual TournamentSelection()
        {
            // Grab two random individuals from the population
            var candidate1 = Population[random.Next(PopulationCount)];
            var candidate2 = Population[random.Next(PopulationCount)];

            // Ensure that the two individuals are unique
            while(candidate1 == candidate2)
            {
                candidate2 = Population[random.Next(PopulationCount)];
            }

            // Return the individual that has the higher fitness value
            if (candidate1.GetFitness() > candidate2.GetFitness())
            {
                return candidate1;
            }
            else
            {
                return candidate2;
            }
        }

        private Individual BiasedRandomSelection()
        {
            // Generate a random number between 0 - 1
            // 0.4
            var selectedValue = random.NextDouble();

            // Loop through our cumulative values list until we find a value that is larger than the value we generated.
            // 0.25 < 0.4 - Nope!
            // 0.55 > 0.4 - Great!
            for (int i = 0; i < cumulativeProportions.Count(); i++)
            {
                var value = cumulativeProportions[i];

                if(value >= selectedValue)
                {
                    // Return the individual that is at this index.
                    return Population[i];
                }
            }

            // We either generated a number outside of our range or our values didnt sum to 1.
            // Both should be impossible so we hope to never see this.
            throw new Exception("Oh no what happened here!!!");
        }

        public Individual GetBestIndividual()
        {
            return Population.OrderBy(n => n.GetFitness()).First();
        }

        public void UpdateCumulativeProportions()
        {
            // Get the inverse proportion that each individual takes up of the total solution
            // The shorter the path, the larger the value - the fitter that solution is.
            var sum = Population.Sum(n => n.GetFitness());
            var proportions = Population.Select(n => sum / n.GetFitness());

            // Normalize these values to sum to 1
            // This allows us to randomly generate a number between 0-1 and select that individual
            // [0.25, 0.30, 0.45]
            var proportionSum = proportions.Sum();
            var normalizedProportions = proportions.Select(p => p / proportionSum);

            // Create a list to hold our cumulated values
            var cumulativeTotal = 0.0;

            // Populate the cumulated values
            // [0.25, 0.55, 1]
            foreach (var proportion in normalizedProportions)
            {
                cumulativeTotal += proportion;
                cumulativeProportions.Add(cumulativeTotal);
            }
        }
    }
}



