using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MTGBotWebsite.Hubs;
using MTGOLibrary;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.TournamentLibrary
{
    public class CubeDraftManager : ICubeDraftManager
    {
        private readonly MainDbContext DB;
        public List<Drafter> Players { get; set; }
        private int _currentPack = 0;
        private readonly CubeDraft _draft;
        private readonly object ThreadLock = new object();
        private readonly IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<CubeHub>();

        public void StartDraft()
        {
            var cardIds = _draft.CubeDraftCards.Select(c => c.CardId).ToArray();
            var cardObjects = DB.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).Shuffle().ToArray();

            var players = _draft.CubeDraftPlayers.ToList();

            if (players.Count < 2 || players.Count > 10)
                throw new InvalidOperationException("Invalid # of players. Players must be between 2 and 10");

            if (players.Count*45 > cards.Length)
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
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), "Cube Draft"),
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), "Cube Draft"),
                                    new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), "Cube Draft")
                                }
                        )
                    )
                );

                player.Position = position;
                DB.Entry(player).State = EntityState.Modified;
                position++;
            }

            _draft.Status = CubeDraftStatus.Drafting;
            DB.Entry(_draft).State = EntityState.Modified;
            DB.SaveChanges();

            hubContext.Clients.Group(String.Format("draft/{0}/clients", _draft.Id)).draftStarted();
        }

        public void PlayerSubscribe(string username, string connectionId)
        {
            var player = Players.Single(p => p.PlayerName == username);
            player.ClientIds.Add(connectionId);
            
            lock (ThreadLock)
            {
                if (_currentPack == 0)
                {
                    if (Players.All(p => p.ClientIds.Count > 0))
                    {
                        _currentPack++;
                        Players.ForEach(p => p.NotifyPick());
                    }
                    else
                    {
                        var disconnectedPlayers = Players.Where(p => p.ClientIds.Count == 0).Select(p => p.PlayerId).ToArray();
                        foreach (
                            var clientId in
                                Players.Where(p => p.ClientIds.Count > 0).SelectMany(p => p.ClientIds))
                        {
                            hubContext.Clients.Client(clientId).waitingForPlayers(disconnectedPlayers);
                        }
                    }
                    return;
                }
            }
            player.NotifyPick(true);
        }

        //TODO: Later...much later
        public void PlayerUnSubscribe(string username, string connectionId)
        {
            
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

                nextDrafter.QueuedPicks.Enqueue(pack.ToList());
                nextDrafter.NotifyPick();
            }

            lock (ThreadLock)
            {
                if (!Players.All(p => p.Picks.Count % 15 == 0 && p.Picks.Count > 0)) return;

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
        }

        private Drafter FindNextPlayer(Drafter drafter)
        {
            var curPick = drafter.CurrentPick - 1;

            var nextPosition = (curPick <= 15 || curPick >= 30) ? drafter.Position-1 : drafter.Position+1;

            if (nextPosition < 1) nextPosition = Players.Count;
            if (nextPosition > Players.Count) nextPosition = 1;

            return Players.Single(d => d.Position == nextPosition);
        }

        public CubeDraftManager(ref MainDbContext db, CubeDraft draft)
        {
            Players = new List<Drafter>();
            DB = db;

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