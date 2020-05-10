using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GeneticAlgorithm
{
    public static class GAConfig
    {
        public static int MaxGenerations => 10000;

        public static double MutationChance => 0.5;

        public static int MaxNoImprovement => 20;
    }
}
