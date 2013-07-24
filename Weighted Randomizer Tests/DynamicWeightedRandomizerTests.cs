using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Weighted_Randomizer;

namespace Weighted_Randomizer_Tests
{
    [TestFixture]
    public class DynamicWeightedRandomizerTests : WeightedRandomizerTestsBase
    {
        protected override IWeightedRandomizer<T> CreateTarget<T>()
        {
            return new DynamicWeightedRandomizer<T>();
        }
    }
}
