using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MTGBotWebsite.TournamentLibrary
{
    public class DraftCollection : Queue<Pack>
    {
        public DraftCollection(IEnumerable<Pack> packs)
        {
            //foreach ( var pack in packs )
            //    base.InsertItem();
            //base.InsertItem(0, pack1);
            //base.InsertItem(1, pack2);
            //base.InsertItem(2, pack3);
            foreach ( var pack in packs )
                Enqueue(pack);
        }

        public Pack GetNextPack()
        {
            try
            {
                return Dequeue();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /*protected override void InsertItem(int index, Pack item)
        {
            throw new InvalidOperationException("Cannot add packs to a draft collection.");
        }

        protected override void RemoveItem(int index)
        {
            throw new InvalidOperationException("Cannot remove packs from a draft collection.");
        }*/
    }
}