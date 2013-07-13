using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MTGOLibrary;
using MTGOLibrary.Models;

namespace MTGBotWebsite.CubeDrafts
{
    public class CubeDraftManager : ICubeDraftManager
    {
        private static readonly MainDbContext DB = new MainDbContext();
        public List<Drafter> Players { get; set; }
        private int _currentPack = 1;
        private readonly CubeDraft _draft;

        public void StartDraft()
        {
            var cards = DB.Cards.Where(c => _draft.CubeDraftCards.Select(cdc => cdc.CardId).ToList().Shuffle().Contains(c.Id));

            var players = _draft.CubeDraftPlayers.ToList();

            if (players.Count < 2 || players.Count > 10)
                throw new InvalidOperationException("Invalid # of players. Players must be between 2 and 10");

            if (players.Count*45 > cards.Count())
                throw new InvalidOperationException("Pool is not large enough for all players");

            int position = 1;
            int packPosition = -1;

            foreach (var player in players.Shuffle())
            {
                Players.Add(
                    new Drafter(
                        _draft,
                        player,
                        position,
                        new DraftCollection(
                            new[]
                                {
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), 1, "Cube Draft"),
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), 2, "Cube Draft"),
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), 3, "Cube Draft")
                                }
                        )
                    )
                );

                player.Position = position;
                DB.Entry(player).State = EntityState.Modified;
                DB.SaveChanges();

                position++;
            }
        }

        //If a draft is in progress and failed for whatever reason, get back to original state
        public void Recover()
        {
            throw new NotImplementedException();
        }

        //Overrideables

        public void Pick(string drafterName, int pickNumber, int pickId)
        {
            var drafter = Players.FirstOrDefault(p => p.PlayerName == drafterName);

            if ( drafter == null )
                throw new Exception("Unable to find drafter");

            var pack = drafter.TakePick(pickNumber, pickId);

            if (pack.Length > 0)
            {
                var nextDrafter = FindNextPlayer(drafter);

                nextDrafter.QueuedPicks.Enqueue(pack);
                nextDrafter.NotifyPick();
            }

            if (Players.Any(p => p.Picks.Count != 0)) return;

            //All players are done picking their cards

            //We done here?
            if (_currentPack == 3)
            {
                _draft.Status = CubeDraftStatus.ProductHandOut;
                DB.Entry(_draft).State = EntityState.Modified;
                DB.SaveChanges();
                return;
            }

            //Update the current pack, and signal to open next packs
            _currentPack++;
            Players.ForEach(p => p.OpenNextPack());
        }

        private Drafter FindNextPlayer(Drafter drafter)
        {
            var curPick = drafter.CurrentPick - 1;

            var nextPosition = (curPick <= 15 || curPick >= 30) ? drafter.Position-1 : drafter.Position+1;

            if (nextPosition < 1) nextPosition = Players.Count;
            if (nextPosition > Players.Count) nextPosition = 1;

            return Players.Single(d => d.Position == nextPosition);
        }

        CubeDraftManager(CubeDraft draft)
        {
            Players = new List<Drafter>();

            _draft = draft;

            if (_draft == null)
                throw new ArgumentNullException("draft", "Draft cannot be null");

            switch (_draft.Status)
            {
                case CubeDraftStatus.PreStart:
                    StartDraft();
                    break;
                case CubeDraftStatus.Drafting:
                    Recover();
                    break;
                default:
                    throw new InvalidOperationException("Draft is in an invalid state.");
            }
        }
    }
}