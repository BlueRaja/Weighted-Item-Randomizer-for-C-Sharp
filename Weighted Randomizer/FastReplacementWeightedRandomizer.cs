using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weighted_Randomizer
{
    class FastReplacementWeightedRandomizer<TKey> : IWeightedRandomizer<TKey>
    {
        private readonly ThreadSafeRandom _random;
        private readonly Dictionary<TKey, int> _weights;
        private bool _listNeedsRebuilding;

        private readonly IList<TKey> _probabilityBoxes;
        private readonly IList<TKey> _aliases;
        private long _heightPerBox;

        public FastReplacementWeightedRandomizer()
        {
            _random = new ThreadSafeRandom();
            _weights = new Dictionary<TKey, int>();
            _listNeedsRebuilding = true;
            TotalWeight = 0;

            _probabilityBoxes = new List<TKey>();
            _aliases = new List<TKey>();
            _heightPerBox = 0;
        }

        //public FastReplacementWeightedRandomizer(int seed)
        //{
        //    random = new ThreadSafeRandom(seed);
        //}

        #region ICollection<T> stuff
        /// <summary>
        /// Returns the number of items currently in the list
        /// </summary>
        public int Count { get { return _weights.Keys.Count;  } }

        /// <summary>
        /// Remove all items from the list
        /// </summary>
        public void Clear()
        {
            _weights.Clear();
            _listNeedsRebuilding = true;
        }

        /// <summary>
        /// Returns false.  Necessary for the ICollection&lt;T&gt; interface.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Copies the keys to an array, in order
        /// </summary>
        public void CopyTo(TKey[] array, int startingIndex)
        {
            int currentIndex = startingIndex;
            foreach (TKey key in this)
            {
                array[currentIndex] = key;
                currentIndex++;
            }
        }

        /// <summary>
        /// Returns true if the given item has been added to the list; false otherwise
        /// </summary>
        public bool Contains(TKey key)
        {
            return _weights.ContainsKey(key);
        }

        /// <summary>
        /// Adds the given item with a default weight of 1
        /// </summary>
        public void Add(TKey key)
        {
            Add(key, 1);
        }

        /// <summary>
        /// Adds the given item with the given weight
        /// </summary>
        public void Add(TKey key, int weight)
        {
            _weights.Add(key, weight);
            _listNeedsRebuilding = true;
            TotalWeight += weight;
        }

        /// <summary>
        /// Remoevs the given item from the list.
        /// </summary>
        /// <returns>Returns true if the item was successfully deleted, or false if it was not found</returns>
        public bool Remove(TKey key)
        {
            int weight;
            if (!_weights.TryGetValue(key, out weight))
            {
                return false;
            }

            TotalWeight -= weight;
            _listNeedsRebuilding = true;
            return _weights.Remove(key);
        }
        #endregion

        #region IEnumerable<T> stuff
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            return _weights.Keys.GetEnumerator();
        }
        #endregion

        #region IWeightedRandomizer<T> stuff
        /// <summary>
        /// The total weight of all the items added so far
        /// </summary>
        public long TotalWeight { get; private set; }

        /// <summary>
        /// Returns an item chosen randomly by weight (higher weights are more likely),
        /// and replaces it so that it can be chosen again
        /// </summary>
        public TKey NextWithReplacement()
        {
            if (Count <= 0)
                throw new InvalidOperationException("There are no items in the FastReplacementWeightedRandomizer");

            if (_listNeedsRebuilding)
            {
                RebuildProbabilityList();
            }

            //TODO:
        }

        private void RebuildProbabilityList()
        {
            long weightMultiplier = CalculateWeightMultiplier();
            _heightPerBox = weightMultiplier*TotalWeight/Count;

        }

        private long CalculateWeightMultiplier()
        {
            //We want the height of each box to be some multiple of the average which is whole number.
            //Since the average is TotalWeight/Count, we need to multiply it by Count/gcd(Count,TotalWeight)
            return (Count / GreatestCommonDenominator(Count, TotalWeight));
        }

        private static long GreatestCommonDenominator(long a, long b)
        {
            while(b > 0)
            {
                long remainder = a % b;
                if(remainder == 0)
                    return b;
                a = b;
                b = remainder;
            }

            return a;
        }

        /// <summary>
        /// Returns an item chosen randomly by weight (higher weights are more likely),
        /// and removes it so it cannot be chosen again
        /// </summary>
        public TKey NextWithRemoval()
        {
            if (Count <= 0)
                throw new InvalidOperationException("There are no items in the FastReplacementWeightedRandomizer");

            TKey randomKey = NextWithReplacement();
            Remove(randomKey);
            return randomKey;
        }

        /// <summary>
        /// Shortcut syntax to add, remove, and update an item
        /// </summary>
        public int this[TKey key]
        {
            get
            {
                return GetWeight(key);
            }
            set
            {
                SetWeight(key, value);
            }
        }

        /// <summary>
        /// Returns the weight of the given item.  Throws an exception if the item is not added
        /// (use .Contains to check first if unsure)
        /// </summary>
        public int GetWeight(TKey key)
        {
            Node node = FindNode(root, key);
            if (node == null)
                throw new ArgumentException("Key not found in FastRemovalWeightedRandomizer: " + key);
            return node.weight;
        }

        /// <summary>
        /// Updates the weight of the given item, or adds it if it has not already been added.
        /// If weight &lt;= 0, the item is removed.
        /// </summary>
        public void SetWeight(TKey key, int weight)
        {
            if (weight <= 0)
            {
                Remove(key);
            }
            else
            {
                Node node = FindNode(root, key);
                if (node == null)
                {
                    Add(key, weight);
                }
                else
                {
                    int weightDelta = weight - node.weight;

                    //This is a hack.  The point is to update this node's and all it's ancestors' subtreeWeights.
                    //We already have a method that will do that; however, it uses the value of node.weight, rather
                    //than a parameter.
                    node.weight = weightDelta;
                    UpdateSubtreeWeightsForInsertion(node);
                    
                    //Finally, set the node.weight to what it should be
                    node.weight = weight;
                    node.subtreeWeight += weightDelta;

#if DEBUG //TODO: DELETE
                    DebugCheckTree();
#endif
                }
            }
        }
        #endregion

        #region Debugging code
        /// <summary>
        /// Returns the height of the tree (very slow)
        /// </summary>
        private int Height
        {
            get
            {
                return GetNumLayers(root);
            }
        }

        private int GetNumLayers(Node node)
        {
            if (node == null || node == sentinel)
                return 0;
            return Math.Max(GetNumLayers(node.left), GetNumLayers(node.right)) + 1;
        }

        /// <summary>
        /// Quick hack to write quick tests
        /// </summary>
        private void Assert(bool condition)
        {
            if (!condition)
                throw new ArgumentException("Test case failed");
        }

        /// <summary>
        /// Make sure the entire tree is valid (correct subtreeWeights, valid BST, that sort of thing)
        /// </summary>
        private void DebugCheckTree()
        {
            DebugCheckNode(root);
            Assert(Count == 0 || Height <= 2 * Math.Ceiling(Math.Log(Count, 2) + 1));
        }

        private void DebugCheckNode(Node node)
        {
            if (node == null || node == sentinel)
                return;

            Assert(node.left == null || node.left == sentinel || node.left.key.CompareTo(node.key) < 0);
            Assert(node.right == null || node.right == sentinel || node.right.key.CompareTo(node.key) > 0);
            Assert(node.left.subtreeWeight + node.right.subtreeWeight + node.weight == node.subtreeWeight);

            DebugCheckNode(node.left);
            DebugCheckNode(node.right);
        }
        #endregion
    }
}
