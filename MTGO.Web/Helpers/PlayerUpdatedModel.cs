﻿using System;
using System.Linq;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Web.Helpers
{
    public class PlayerUpdatedModel
    {
        public int Id { get; internal set; }
        public int UserId { get; internal set; }
        public bool Confirmed { get; internal set; }
        public int RequireCollateral { get; internal set; }
        public string Username { get; internal set; }
        public string MtgoUsername { get; internal set; }
        public int MtgoId { get; internal set; }
        public string DeckStatus { get; internal set; }
        public bool Trading { get; internal set; }
        public bool DeckBuilt { get; internal set; }

        public PlayerUpdatedModel(CubeDraftPlayer player, CubeDraft draft, bool trading = false)
        {
            if ( player.MtgoLink == null )
                throw new InvalidOperationException();

            Id = player.Id;
            UserId = player.MtgoLink.Id;
            Username = player.MtgoLink.User.Username;
            Confirmed = player.Confirmed;
            DeckBuilt = player.DeckBuilt;
            DeckStatus = "none";
            Trading = trading;

            if (player.Confirmed)
            {
                MtgoUsername = player.MtgoLink.User.Username;
                MtgoId = player.MtgoLink.Id;

                RequireCollateral = player.RequireCollateral;

                var mtgoCount = draft.CubeDraftCards.Count(c => c.Location == player.MtgoLink.Id);

                var pickCount = player.CubeDraftPicks.Count;

                if (pickCount > 0)
                {
                    if (mtgoCount > 0 && mtgoCount < pickCount)
                        DeckStatus = "partial";
                    else if (mtgoCount > 0 && mtgoCount >= pickCount)
                        DeckStatus = "full";
                }
            }
            else
            {
                RequireCollateral = 0;
            }
        }
    }
}
