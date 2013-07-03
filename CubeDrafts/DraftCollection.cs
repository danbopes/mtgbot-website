using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTGBotWebsite.CubeDrafts
{
    public class DraftCollection : Collection<Pack>
    {
        public DraftCollection(Pack[] packs)
        {
            base.InsertItem(1, packs.Single(p => p.Number == 1));
            base.InsertItem(2, packs.Single(p => p.Number == 2));
            base.InsertItem(3, packs.Single(p => p.Number == 3));
        }

        protected override void InsertItem(int index, Pack item)
        {
            throw new InvalidOperationException("Cannot add packs to a draft collection.");
        }

        protected override void RemoveItem(int index)
        {
            throw new InvalidOperationException("Cannot remove packs from a draft collection.");
        }
    }
}