using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weighted_Randomizer;

namespace Weighted_Randomizer_Tests
{
    public class SpeedTests
    {
        public static void Main(string[] args)
        {
            TestSpeed(new FastRemovalWeightedRandomizer<int>());
            TestSpeed(new FastReplacementWeightedRandomizer<int>());

            Console.ReadKey();
        }

        private static void TestSpeed(IWeightedRandomizer<int> randomizer)
        {
            const int numIterations = 10000;
            Stopwatch timer = new Stopwatch();
            Console.WriteLine("Testing {0}:", randomizer.GetType().Name);

            //Adding items (ordered)
            timer.Reset();
            timer.Start();
            for (int i = 1; i <= numIterations; i++)
            {
                randomizer.Add(i, i);
            }
            timer.Stop();
            Console.WriteLine("Adding {0} items (ordered): {1} ms", numIterations, timer.ElapsedMilliseconds);

            //Adding items (unordered)
            randomizer.Clear();
            timer.Reset();
            timer.Start();
            int cheapPrng = 1;
            for (int i = 1; i <= numIterations; i++)
            {
                cheapPrng = (22695477*cheapPrng+1)&0x7FFFFFFF;
                randomizer.Add(i, cheapPrng);
            }
            timer.Stop();
            Console.WriteLine("Adding {0} items (unordered): {1} ms", numIterations, timer.ElapsedMilliseconds);

            //NextWithReplacement()
            timer.Reset();
            timer.Start();
            for (int i = 1; i <= numIterations; i++)
            {
                randomizer.NextWithReplacement();
            }
            timer.Stop();
            Console.WriteLine("NextWithReplacement() {0} times: {1} ms", numIterations, timer.ElapsedMilliseconds);

            //NextWithRemoval()
            timer.Reset();
            timer.Start();
            for (int i = 1; i <= numIterations; i++)
            {
                randomizer.NextWithRemoval();
            }
            timer.Stop();
            Console.WriteLine("NextWithRemoval() {0} times: {1} ms", numIterations, timer.ElapsedMilliseconds);

            //NextWithReplacement()
            randomizer.Clear();
            timer.Reset();
            timer.Start();
            for (int i = 1; i <= numIterations; i++)
            {
                randomizer.Add(i, i);
                randomizer.NextWithReplacement();
            }
            timer.Stop();
            Console.WriteLine("Add() + NextWithReplacement() (interleaved) {0} times: {1} ms", numIterations, timer.ElapsedMilliseconds);


            Console.WriteLine();
        }
    }
}
