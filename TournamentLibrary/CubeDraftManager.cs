﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.ServiceModel;
using MTGBotWebsite.Hubs;
using MTGOLibrary;
using MTGOLibrary.Models;
using MTGOLibrary.TournamentLibrary;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.TournamentLibrary
{
    public class CubeDraftManager : ICubeDraftManager
    {
        private readonly MainDbContext _db;
        public List<Drafter> Players { get; set; }
        private int _currentPack = 0;
        private readonly CubeDraft _draft;
        private readonly object _threadLock = new object();
        private readonly IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<CubeHub>();
        public EventLog MyEventLog;
        private static IBotService BotService
        {
            get
            {
                return new ChannelFactory<IBotService>(
                    new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/MTGOBotService")
                ).CreateChannel();
            }
        }

        public void StartDraft()
        {
            var cardIds = _draft.CubeDraftCards.Select(c => c.CardId).ToArray();
            var cardObjects = _db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).Shuffle().ToArray();

            var players = _draft.CubeDraftPlayers.Where(p => p.Confirmed).ToList();

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
                _db.Entry(player).State = EntityState.Modified;
                position++;
            }

            _draft.Status = CubeDraftStatus.Drafting;
            _db.Entry(_draft).State = EntityState.Modified;
            _db.SaveChanges();

            _hubContext.Clients.Group(String.Format("draft/{0}/clients", _draft.Id)).draftStarted();
        }

        public void PlayerSubscribe(string username, string connectionId)
        {
            var player = Players.Single(p => p.PlayerName == username);
            player.ClientIds.Add(connectionId);

            MyEventLog.WriteEntry(player + " connected.", EventLogEntryType.Information);
            
            lock (_threadLock)
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
                            _hubContext.Clients.Client(clientId).waitingForPlayers(disconnectedPlayers);
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
            MyEventLog.WriteEntry("Recovery for draft id '" + _draft.Id + "' started", EventLogEntryType.Warning);

            var picks = _draft.CubeDraftPicks.Where(p => p.Pick == 1 || p.Pick == 16 || p.Pick == 31).Select(p => p.Picks);
            var cardIds = _draft.CubeDraftCards.Select(c => c.CardId).ToList();

            var allCardObjects = _db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();

            foreach (var pick in picks.SelectMany(c => c.Split(',')))
                cardIds.Remove(Convert.ToInt32(pick));

            var cardObjects = _db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).Shuffle().ToArray();

            var players = _draft.CubeDraftPlayers.Where(p => p.Confirmed).ToList();

            if (players.Count < 2 || players.Count > 10)
                throw new InvalidOperationException("Invalid # of players. Players must be between 2 and 10");

            //if (players.Count * 45 > cards.Length)
            //    throw new InvalidOperationException("Pool is not large enough for all players");

            var packPosition = -1;

            foreach (var player in players.Shuffle())
            {
                var playerPicks = player.CubeDraftPicks.Where(p => p.PickId != null).Select(p => p.PickId);
                var playerCardObjects = _db.Cards.Where(c => playerPicks.Contains(c.Id)).ToArray();

                var packs = new List<Pack>();

                for (var i = playerPicks.Count(); i <= 45; i += 15)
                {
                    packs.Add(new Pack(cards.Skip(++packPosition*15).Take(15).ToArray(), "Cube Draft"));
                }

                var newDrafter = new Drafter(
                    _draft,
                    player,
                    player.Position,
                    new DraftCollection(packs),
                    playerCardObjects.ToList()
                    );

                //Add Queued Picks to Drafters
                foreach (var pickIds in player.CubeDraftPicks.Where(p => p.PickId == null).OrderBy(p => p.Pick).Select(pick => pick.Picks.Split(',').Select(p => Convert.ToInt32(p))))
                {
                    newDrafter.QueuedPicks.Enqueue(pickIds.Select(c => allCardObjects.Single(co => co.Id == c)).ToList());
                }

                Players.Add(newDrafter);
            }

            var max = _draft.CubeDraftPicks.Select(c => c.Pick).DefaultIfEmpty().Max();

            _currentPack = (int)Math.Ceiling((decimal)max/15);

            if (!Players.All(p => p.Picks.Count % 15 == 0 && p.Picks.Count > 0)) return;

            //All players are done picking their cards

            //We done here?
            if (_currentPack == 3)
            {
                _draft.Status = CubeDraftStatus.ProductHandOut;
                _db.Entry(_draft).State = EntityState.Modified;
                _db.SaveChanges();
                _hubContext.Clients.Group(String.Format("draft/{0}/clients", _draft.Id)).statusUpdate(CubeDraftStatus.ProductHandOut);
                BotService.UpdateCubeStatus(_draft.Id);
                return;
            }

            //Update the current pack, and signal to open next packs
            _currentPack++;
            Players.ForEach(p => p.OpenNextPack());

            _hubContext.Clients.Group(String.Format("draft/{0}/clients", _draft.Id)).draftStarted();
        }

        //Overrideables
        public void Pick(string drafterName, int pickNumber, int pickId)
        {
            var drafter = Players.FirstOrDefault(p => p.PlayerName == drafterName);
            
            if ( drafter == null )
                throw new Exception("Unable to find drafter");

            lock (_threadLock)
            {
                var pack = drafter.TakePick(pickNumber, pickId);

                if (pack.Length > 0)
                {
                    var nextDrafter = FindNextPlayer(drafter);

                    nextDrafter.QueuedPicks.Enqueue(pack.ToList());
                    nextDrafter.NotifyPick();
                }

                if (!Players.All(p => p.Picks.Count % 15 == 0 && p.Picks.Count > 0)) return;

                //All players are done picking their cards
                
                //We done here?
                if (_currentPack == 3)
                {
                    _draft.Status = CubeDraftStatus.ProductHandOut;
                    _db.Entry(_draft).State = EntityState.Modified;
                    _db.SaveChanges();
                    _hubContext.Clients.Group(String.Format("draft/{0}/clients", _draft.Id)).statusUpdate(CubeDraftStatus.ProductHandOut);
                    BotService.UpdateCubeStatus(_draft.Id);
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
            try
            {
                if (!EventLog.SourceExists("CubeDraftManager"))
                    EventLog.CreateEventSource("CubeDraftManager", "Application");
            }
            catch (SecurityException)
            {
            }

            MyEventLog = new EventLog("Application", ".", "CubeDraftManager");

            Players = new List<Drafter>();
            _db = db;

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