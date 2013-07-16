using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weighted_Randomizer
{
    /// <summary>
    /// Represents a class which can choose weighted items at random; that is, it can randomly choose items from a list, giving some items higher
    /// probability of being chosen than others.  It supports both choosing with replacement (so the same item can be chosen multiple times) and
    /// choosing with removal (so each item can be chosen only once).
    /// 
    /// Note that though this interface is enumerable, the enumeration is not necessarily ordered by anything.
    /// </summary>
    /// <typeparam name="TKey">The type of the objects to choose at random</typeparam>
    public interface IWeightedRandomizer<TKey> : ICollection<TKey>
    {
        /// <summary>
        /// The total weight of all the items added so far
        /// </summary>
        long TotalWeight { get; }

        /// <summary>
        /// Returns an item chosen randomly by weight (higher weights are more likely),
        /// and replaces it so that it can be chosen again
        /// </summary>
        TKey NextWithReplacement();

        /// <summary>
        /// Returns an item chosen randomly by weight (higher weights are more likely),
        /// and removes it so it cannot be chosen again
        /// </summary>
        TKey NextWithRemoval();

        /// <summary>
        /// Adds the given item with the given weight.  Higher weights are more likely to be chosen.
        /// Throw an ArgumentOutOfRange exception if weight &lt;= 0.
        /// </summary>
        void Add(TKey key, int weight);

        /// <summary>
        /// Shortcut syntax to add, remove, and update an item
        /// </summary>
        int this[TKey key] { get; set; }

        /// <summary>
        /// Returns the weight of the given item.  Throws an exception if the item is not added
        /// (use .Contains to check first if unsure)
        /// </summary>
        int GetWeight(TKey key);

        /// <summary>
        /// Updates the weight of the given item, or adds it if it has not already been added.
        /// If weight &lt;= 0, the item is removed.
        /// </summary>
        void SetWeight(TKey key, int weight);
    }
}
