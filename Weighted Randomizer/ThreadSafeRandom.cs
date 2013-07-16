using System;

namespace Weighted_Randomizer
{
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();

        [ThreadStatic]
        private static Random _local;

        public ThreadSafeRandom()
        {
            if(_local == null)
            {
                //Instantiating multiple Random() instances in a row very quickly will result in
                //all of them returning the same numbers.  This is a workaround for that problem.
                int seed;
                lock(_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }
        }

        public int Next()
        {
            return _local.Next();
        }

        public int Next(int maxValue)
        {
            return _local.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _local.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return _local.NextDouble();
        }

        public long NextLong()
        {
            return _local.NextLong();
        }

        public long NextLong(long max)
        {
            return _local.NextLong(max);
        }

        public long NextLong(long min, long max)
        {
            return _local.NextLong(min, max);
        }
    }
}
