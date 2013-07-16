using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Weighted_Randomizer;

namespace Weighted_Randomizer_Tests
{
    public class SpeedTests
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Warming up, please wait...");
            TestSpeed(new DynamicWeightedRandomizer<int>(), false);
            TestSpeed(new StaticWeightedRandomizer<int>(), false);
            Console.Clear();

            //Above lines are to force JIT to kick in.  Below two lines are the REAL benchmark:
            TestSpeed(new DynamicWeightedRandomizer<int>(), true);
            TestSpeed(new StaticWeightedRandomizer<int>(), true);

            Console.WriteLine("Complete!");
            Console.ReadKey();

            /*
            Example output:
            
            Testing DynamicWeightedRandomizer`1
            --------------------------------
            Add()x10000 + NextWithReplacement()x10: 4 ms
            Add()x10000 + NextWithReplacement()x10000: 7 ms
            Add()x10000 + NextWithReplacement()x100000: 35 ms
            ( Add() + NextWithReplacement() )x10000 (interleaved): 8 ms
            Add()x10000 + NextWithRemoval()x10000: 10 ms


            Testing StaticWeightedRandomizer`1
            --------------------------------
            Add()x10000 + NextWithReplacement()x10: 2 ms
            Add()x10000 + NextWithReplacement()x10000: 4 ms
            Add()x10000 + NextWithReplacement()x100000: 28 ms
            ( Add() + NextWithReplacement() )x10000 (interleaved): 5403 ms
            Add()x10000 + NextWithRemoval()x10000: 5948 ms
            */
        }

        private static void TestSpeed(IWeightedRandomizer<int> randomizer, bool displayText)
        {
            GCLatencyMode oldLatencyMode = GCSettings.LatencyMode;

            try
            {
                //Prevent garbage collection during tests
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;

                const int numIterations = 10000;
                Stopwatch timer = new Stopwatch();
                if(displayText)
                {
                    Console.WriteLine("Testing {0}", randomizer.GetType().Name);
                    Console.WriteLine("--------------------------------");
                }

                //NextWithReplacement()/1000
                randomizer.Clear();
                timer.Reset();
                timer.Start();
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.Add(i, i);
                }
                for(int i = 1; i <= numIterations / 1000; i++)
                {
                    randomizer.NextWithReplacement();
                }
                timer.Stop();
                GC.Collect();
                if(displayText)
                {
                    Console.WriteLine("Add()x{0} + NextWithReplacement()x{1}: {2} ms", numIterations, numIterations / 1000,
                        timer.ElapsedMilliseconds);
                }

                //NextWithReplacement()
                randomizer.Clear();
                timer.Reset();
                timer.Start();
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.Add(i, i);
                }
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.NextWithReplacement();
                }
                timer.Stop();
                GC.Collect();
                if(displayText)
                {
                    Console.WriteLine("Add()x{0} + NextWithReplacement()x{1}: {2} ms", numIterations, numIterations,
                        timer.ElapsedMilliseconds);
                }

                //NextWithReplacement() * 10
                randomizer.Clear();
                timer.Reset();
                timer.Start();
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.Add(i, i);
                }
                for(int i = 1; i <= 10 * numIterations; i++)
                {
                    randomizer.NextWithReplacement();
                }
                timer.Stop();
                GC.Collect();
                if(displayText)
                {
                    Console.WriteLine("Add()x{0} + NextWithReplacement()x{1}: {2} ms", numIterations, 10 * numIterations,
                        timer.ElapsedMilliseconds);
                }

                //NextWithReplacement() (interleaved)
                randomizer.Clear();
                timer.Reset();
                timer.Start();
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.Add(i, i);
                    randomizer.NextWithReplacement();
                }
                timer.Stop();
                GC.Collect();
                if(displayText)
                {
                    Console.WriteLine("( Add() + NextWithReplacement() )x{0} (interleaved): {1} ms", numIterations,
                        timer.ElapsedMilliseconds);
                }

                //NextWithRemoval()
                randomizer.Clear();
                timer.Reset();
                timer.Start();
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.Add(i, i);
                }
                for(int i = 1; i <= numIterations; i++)
                {
                    randomizer.NextWithRemoval();
                }
                timer.Stop();
                GC.Collect();
                if(displayText)
                {
                    Console.WriteLine("Add()x{0} + NextWithRemoval()x{1}: {2} ms", numIterations, numIterations,
                        timer.ElapsedMilliseconds);

                    Console.WriteLine();
                    Console.WriteLine();
                }
            } //end try
            finally
            {
                // ALWAYS set the latency mode back
                GCSettings.LatencyMode = oldLatencyMode;
            }
        }
    }
}
