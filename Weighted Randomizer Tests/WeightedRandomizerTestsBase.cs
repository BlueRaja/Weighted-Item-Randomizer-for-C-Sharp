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
    public class WeightedRandomizerTestsBase
    {
        /// <summary>
        /// The weighted randomizer class to be tested.  Needs to be set by inherited class in test-setup phase
        /// </summary>
        protected IWeightedRandomizer<int> Target { get; set; }

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
            foreach (int item in items)
            {
                Target.Add(item, 1);
            }

            //Remove the items from target one-by-one.  Verify only *exactly* those items are removed, in any order
            while (items.Any())
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
        public void TestSetWeight()
        {
            Target.Add(1, 10);
            Target.Add(2, 20);

            Assert.AreEqual(10, Target.GetWeight(1));
            Assert.AreEqual(20, Target.GetWeight(2));

            Target.SetWeight(1, 11);
            Target.SetWeight(2, 12);

            Assert.AreEqual(11, Target.GetWeight(1));
            Assert.AreEqual(12, Target.GetWeight(2));
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
        public void TestEnumeration()
        {
            ISet<int> items = new HashSet<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            foreach (int item in items)
            {
                Target.Add(item);
            }

            //Enumeration is not ordered, so we just need to make sure that all items we added are actually in there
            foreach (int key in Target)
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
            for (int i = 0; i < 10000; i++)
            {
                if (Target.NextWithReplacement() == 1)
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
            for (int i = 0; i < 10000; i++)
            {
                if (Target.NextWithReplacement() == 1)
                    oneCount++;
            }

            //Around a one-in-a-million chance of this happening, based on normal distribution approximation
            Assert.Greater(oneCount, 4700);
            Assert.Less(oneCount, 5300);
        }

        //Exceptions
        [Test]
        public void TestAddThrowsExceptionOnNegativeWeight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.Add(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Target.Add(1, 0));
        }

        //TODO: Make sure SetWeight() works in all the other cases it's supposed to work in
    }
}
