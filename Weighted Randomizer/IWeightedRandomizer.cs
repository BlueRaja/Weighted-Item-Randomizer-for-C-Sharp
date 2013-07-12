using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weighted_Randomizer
{
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
