using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Weighted_Randomizer;

namespace Weighted_Randomizer_Tests
{
    [Explicit("Don't run tests on the base class")]
    public abstract class WeightedRandomizerTestsBase
    {
        /// <summary>
        /// Used to create the target of the tests.  Needs to be overriden by child test-class.
        /// </summary>
        protected abstract IWeightedRandomizer<T> CreateTarget<T>() where T : IComparable<T>;

        /// <summary>
        /// The weighted randomizer class to be tested.
        /// </summary>
        private IWeightedRandomizer<int> Target { get; set; }

        [SetUp]
        public void Setup()
        {
            Target = CreateTarget<int>();
        }

        [Test]
        public void TestCountSimple()
        {
            Assert.AreEqual(0, Target.Count);
            Target.Add(1, 1);
            Assert.AreEqual(1, Target.Count);
        }

        [Test]
        public void TestCountRemovals()
        {
            Target.Add(1, 1);
            Target.Add(2, 2);
            Assert.AreEqual(2, Target.Count);
            Target.Remove(2);
            Assert.AreEqual(1, Target.Count);
            Target.Remove(1);
            Assert.AreEqual(0, Target.Count);
        }

        [Test]
        public void TestCountNextWithReplacements()
        {
            Target.Add(1, 1);
            Target.Add(2, 2);
            Assert.AreEqual(2, Target.Count);
            Target.NextWithReplacement();
            Assert.AreEqual(2, Target.Count);
        }

        [Test]
        public void TestCountNextWithRemovals()
        {
            Target.Add(1, 1);
            Target.Add(2, 2);
            Assert.AreEqual(2, Target.Count);
            Target.NextWithRemoval();
            Assert.AreEqual(1, Target.Count);
        }

        [Test]
        public void TestSingleItemReplacement()
        {
            Target.Add(1, 100);
            Assert.AreEqual(1, Target.NextWithReplacement());
        }

        [Test]
        public void TestSingleItemRemoval()
        {
            Target.Add(1, 100);
            Assert.AreEqual(1, Target.NextWithReplacement());
        }

        [Test]
        public void TestNextWithRemovalOnlyValidItems()
        {
            ISet<int> items = new HashSet<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            //Add the items to target
            foreach(int item in items)
            {
                Target.Add(item, 1);
            }

            //Remove the items from target one-by-one.  Verify only *exactly* those items are removed, in any order
            while(items.Any())
            {
                int next = Target.NextWithRemoval();
                CollectionAssert.Contains(items, next);
                items.Remove(next);
            }

            Assert.AreEqual(0, Target.Count);
        }

        [Test]
        public void TestGetWeight()
        {
            Target.Add(1, 10);
            Target.Add(2, 20);
            Target.Add(3, 30);

            Assert.AreEqual(10, Target.GetWeight(1));
            Assert.AreEqual(20, Target.GetWeight(2));
            Assert.AreEqual(30, Target.GetWeight(3));
        }

        [Test]
        public void TestSetWeightAdds()
        {
            Target.SetWeight(1, 10);
            Assert.AreEqual(1, Target.Count);
            Assert.AreEqual(10, Target.GetWeight(1));
        }

        [Test]
        public void TestSetWeightUpdates()
        {
            Target.Add(1, 5);
            Target.SetWeight(1, 10);
            Assert.AreEqual(1, Target.Count);
            Assert.AreEqual(10, Target.GetWeight(1));
            Assert.AreEqual(10, Target.TotalWeight);
        }

        [Test]
        public void TestIndexingNotation()
        {
            Target.Add(1, 10);
            Target.Add(2, 20);

            Assert.AreEqual(10, Target[1]);
            Assert.AreEqual(20, Target[2]);

            Target[1] = 11;
            Target[2] = 12;

            Assert.AreEqual(11, Target[1]);
            Assert.AreEqual(12, Target[2]);
        }

        [Test]
        public void TestOtherAddMethod()
        {
            Target.Add(10);
            Assert.AreEqual(1, Target.GetWeight(10));
        }

        [Test]
        public void TestContains()
        {
            Assert.IsFalse(Target.Contains(1));
            Assert.IsFalse(Target.Contains(2));

            Target.Add(2);

            Assert.IsFalse(Target.Contains(1));
            Assert.IsTrue(Target.Contains(2));
        }

        [Test]
        public void TestClear()
        {
            Target.Add(1);
            Assert.AreEqual(1, Target.Count);

            Target.Clear();
            Assert.AreEqual(0, Target.Count);
            Assert.AreEqual(0, Target.TotalWeight);
            Assert.IsFalse(Target.Contains(1));
        }

        [Test]
        public void TestTotalWeight()
        {
            Target.Add(1, 10);
            Assert.AreEqual(10, Target.TotalWeight);

            Target.Add(2, 10);
            Assert.AreEqual(20, Target.TotalWeight);

            Target.Add(3, 15);
            Assert.AreEqual(35, Target.TotalWeight);
        }

        [Test]
        public void TestTotalWeightDoesNotThrowWithNoItems()
        {
            Assert.AreEqual(0, Target.TotalWeight);
        }

        [Test]
        public void TestEnumeration()
        {
            ISet<int> items = new HashSet<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            foreach(int item in items)
            {
                Target.Add(item);
            }

            //Enumeration is not ordered, so we just need to make sure that all items we added are actually in there
            foreach(int key in Target)
            {
                CollectionAssert.Contains(items, key);
                items.Remove(key);
            }

            CollectionAssert.IsEmpty(items);
        }

        //Tests with TestProb are only extremely LIKELY to pass.
        //Can't test in isolation from randomizer since there is no IRandom interface (and this codebase introduces enough extra stuff as it is)

        [Test]
        public void TestProbNextWithReplacementTwoItems()
        {
            Target.Add(1, 1);
            Target.Add(2, 999999);
            Assert.AreEqual(2, Target.NextWithReplacement());
        }

        [Test]
        public void TestProbNextWithRemovalTwoItems()
        {
            Target.Add(1, 1);
            Target.Add(2, 999888);
            Assert.AreEqual(2, Target.NextWithRemoval());
        }

        [Test]
        public void TestProbNextWithReplacementThreeItems()
        {
            Target.Add(1, 1);
            Target.Add(2, 99888777);
            Target.Add(3, 1);

            for(int i = 0; i < 10; i++)
                Assert.AreEqual(2, Target.NextWithReplacement());
        }

        [Test]
        public void TestProbNextWithRemovalThreeItems()
        {
            Target.Add(1, 1);
            Target.Add(2, 999888777);
            Target.Add(3, 555444);
            Assert.AreEqual(2, Target.NextWithRemoval());
            Assert.AreEqual(3, Target.NextWithRemoval());
            Assert.AreEqual(1, Target.NextWithRemoval());
        }

        [Test]
        public void TestProbNextWithReplacementManyIterations()
        {
            Target.Add(1, 1);
            Target.Add(2, 99);
            int oneCount = 0;
            for(int i = 0; i < 10000; i++)
            {
                if(Target.NextWithReplacement() == 1)
                    oneCount++;
            }

            //Around a one-in-a-million chance of this happening, based on normal distribution approximation
            Assert.Greater(oneCount, 50); //...NUnit, why are the arguments suddenly reversed!?
            Assert.Less(oneCount, 150);
        }

        [Test]
        public void TestProbNextWithReplacementManyIterations2()
        {
            Target.Add(1, 1);
            Target.Add(2, 1);
            int oneCount = 0;
            for(int i = 0; i < 10000; i++)
            {
                if(Target.NextWithReplacement() == 1)
                    oneCount++;
            }

            //Around a one-in-a-million chance of this happening, based on normal distribution approximation
            Assert.Greater(oneCount, 4700);
            Assert.Less(oneCount, 5300);
        }

        [Test]
        public void TestProbNextWithReplacementMultipleItems()
        {
            ISet<int> items = new HashSet<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            foreach (int item in items)
            {
                Target.Add(item);
            }

            //Chances of not seeing each one at least one are less than one-in-a-million
            //(see http://en.wikipedia.org/wiki/Coupon_collector%27s_problem#Calculating_the_variance )
            int[] counts = new int[10];
            for(int i = 0; i <= 14200; i++)
            {
                int index = Target.NextWithReplacement();
                counts[index-1]++;
            }

            Assert.That(counts.All(o => o != 0));
        }

        [Test]
        public void TestAddAllows0Weight()
        {
            Target.Add(1, 0);
            Assert.AreEqual(1, Target.Count);
            Assert.AreEqual(0, Target.TotalWeight);
        }

        [Test]
        public void TestSetWeightAllows0Weight()
        {
            Target.Add(1, 10);
            Target.SetWeight(1, 0);
            Target.SetWeight(2, 0);
            Assert.AreEqual(2, Target.Count);
            Assert.AreEqual(0, Target.TotalWeight);
        }

        [Test]
        public void TestBracketNotationAllows0Weight()
        {
            Target[1] = 0;
            Assert.AreEqual(1, Target.Count);
            Assert.AreEqual(0, Target.TotalWeight);
        }

        [Test]
        public void TestGetWeightWith0Weight()
        {
            Target.SetWeight(1, 1);
            Target.SetWeight(2, 0);
            Assert.AreEqual(1, Target.GetWeight(1));
            Assert.AreEqual(0, Target.GetWeight(2));
        }

        [Test]
        public void TestNextWithReplacementsWorksWithA0Weight()
        {
            Target.Add(1, 1);
            Target.Add(2, 0);

            for(int i = 0; i < 100; i++)
            {
                Assert.AreEqual(1, Target.NextWithReplacement());
            }
        }

        [Test]
        public void TestNextWithRemovalWorksWithA0Weight()
        {
            Target.Add(1, 1);
            Target.Add(2, 0);

            int returnValue = Target.NextWithRemoval();
            Assert.AreEqual(1, returnValue);
            Assert.AreEqual(1, Target.Count);
            Assert.AreEqual(0, Target.TotalWeight);
        }

        #region Exceptions
        [Test]
        public void TestAddThrowsOnNegativeWeight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.Add(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.Add(1, int.MinValue));
        }

        [Test]
        public void TestAddThrowsOnDuplicateKey()
        {
            Target.Add(1, 2);
            Assert.Throws<ArgumentException>(() => Target.Add(1, 2));
            Assert.Throws<ArgumentException>(() => Target.Add(1, 1));
            Assert.Throws<ArgumentException>(() => Target.Add(1));
        }

        private class ExampleClass : IComparable<ExampleClass>
        {
            public int CompareTo(ExampleClass other)
            {
                return 1;
            }
        }

        [Test]
        public void TestAddThrowsOnNullKey()
        {
            IWeightedRandomizer<ExampleClass> newTarget = CreateTarget<ExampleClass>();
            Assert.Throws<ArgumentNullException>(() => newTarget.Add(null, 2));
            Assert.Throws<ArgumentNullException>(() => newTarget.Add(null));
        }

        [Test]
        public void TestNextWithRemovalThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Target.NextWithRemoval());
        }

        [Test]
        public void TestNextWithReplacementThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Target.NextWithReplacement());
        }

        [Test]
        public void TestNextWithRemovalThrowsWith0Weight()
        {
            Target.Add(1, 0);
            Assert.Throws<InvalidOperationException>(() => Target.NextWithRemoval());
        }

        [Test]
        public void TestNextWithReplacementThrowsWith0Weight()
        {
            Target.Add(1, 0);
            Assert.Throws<InvalidOperationException>(() => Target.NextWithReplacement());
        }

        [Test]
        public void TestBracketShorthandThrowsOnKeyNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => { int a = Target[1]; });
        }

        [Test]
        public void TestBracketShorthandThrowsOnNegativeWeight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Target[1] = -1 );
            Assert.Throws<ArgumentOutOfRangeException>(() => Target[2] = Int32.MinValue);
        }

        [Test]
        public void TestBracketShorthandThrowsOnNullKey()
        {
            IWeightedRandomizer<ExampleClass> newTarget = CreateTarget<ExampleClass>();
            Assert.Throws<ArgumentNullException>(() => newTarget[null] = 1);
            Assert.Throws<ArgumentNullException>(() => { int a = newTarget[null]; });
        }

        [Test]
        public void TestGetWeightThrowsOnKeyNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => Target.GetWeight(1));
        }

        [Test]
        public void TestGetWeighthandThrowsOnNullKey()
        {
            IWeightedRandomizer<ExampleClass> newTarget = CreateTarget<ExampleClass>();
            Assert.Throws<ArgumentNullException>(() => newTarget.GetWeight(null));
        }

        [Test]
        public void TestSetWeightThrowsOnNegativeWeight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.SetWeight(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.SetWeight(1, int.MinValue));
        }

        [Test]
        public void TestSetWeightThrowsOnNullKey()
        {
            IWeightedRandomizer<ExampleClass> newTarget = CreateTarget<ExampleClass>();
            Assert.Throws<ArgumentNullException>(() => newTarget.SetWeight(null, 1));
        }
        #endregion
    }
}
