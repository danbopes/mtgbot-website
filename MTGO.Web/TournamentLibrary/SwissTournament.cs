using System;
using System.Collections.Generic;
using System.Linq;
using MTGO.Common;
using MTGO.Common.TournamentLibrary;

namespace MTGO.Web.TournamentLibrary
{
    public class SwissTournament
    {
        public ITournPlayer[] Standings
        {
            get { return Players.OrderByDescending(p => p.Points).ToArray(); }
        }

        public TournPlayerArray Players { get; internal set; }
        public int CurrentRound { get; internal set; }
        public TournMatchArray Matches = new TournMatchArray();
        private List<TournMatch> _pairings;
        private int _nextRound;
        private List<ITournPlayer> _tempPlayers;

        public bool OutstandingMatches { get { return Matches.GetByRound(CurrentRound).Any(m => !m.ReportedResult); } }

        public SwissTournament()
        {
            Players = new TournPlayerArray();

            CurrentRound = 0;
        }

        public void AddPlayer(IPlayer player)
        {
            Players.AddPlayer(new TournPlayer(player));
        }

        public void AddMatch(TournMatch match)
        {
            Matches.AddMatch(match);
            CurrentRound = Matches.Count == 0 ? 0 : Matches.Max(m => m.Round);
        }
        
        public TournMatch UpdateMatch(int round, int player1Id, int player2Id, int currentGame, int player1Wins, int player2Wins, int ties)
        {
            var player1 = Players.Single(c => c.PlayerId == player1Id);
            var player2 = Players.Single(c => c.PlayerId == player2Id);
            var match = Matches.FirstOrDefault(m => m.Round == round && Equals(m.Player1, player1) &&
                                                    Equals(m.Player2, player2));

            if (match == null)
                return null;

            match.CurrentGame = currentGame;
            match.Player1Wins = player1Wins;
            match.Player2Wins = player2Wins;
            match.Ties = ties;

            return match;
        }

        public object GetStandings()
        {
            var roundToGrab = CurrentRound;

            if (roundToGrab <= 0)
                return null;

            if (Matches.GetByRound(CurrentRound).Any(c => !c.ReportedResult))
            {
                roundToGrab = CurrentRound - 1;

                if (roundToGrab <= 0)
                    return null;
            }

            var matches = Matches.GetByRound(1, roundToGrab);

            foreach (var player in Players)
            {
                player.Losses = 0;
                player.GameLosses = 0;
                player.Wins = 0;
                player.GameWins = 0;
                player.Ties = 0;
            }

            foreach (var match in matches)
            {
                var player1 = Players.FindById(match.Player1.PlayerId);
                ITournPlayer player2 = null;
                player1.GameWins += match.Player1Wins;

                if (match.Player2 != null)
                {
                    player2 = Players.FindById(match.Player2.PlayerId);
                    player2.GameWins += match.Player2Wins;
                    player2.GameLosses += match.Player1Wins;
                    player1.GameLosses += match.Player2Wins;
                }

                if (match.Player2 == null || match.Player1Wins > match.Player2Wins)
                {
                    player1.Wins++;
                }
                else if (player2 != null && match.Player2Wins > match.Player1Wins)
                {
                    player2.Wins++;
                }
            }

            foreach (var player in Players)
            {
                var opponents =
                    matches.Where(p => p.Player2 != null && (p.Player1.PlayerId == player.PlayerId || p.Player2.PlayerId == player.PlayerId))
                           .Select(p => Players.FindById(new[] {p.Player1, p.Player2}.Single(sp => sp.PlayerId != player.PlayerId).PlayerId))
                           .Distinct()
                           .ToArray();


                player.TieBreaker1 = (float)Math.Round((opponents.Select(o => Math.Max(0.33f, (float) o.Points/(3*CurrentRound))).Sum()/
                                     opponents.Count())*100, 2);

                player.TieBreaker2 = (float)Math.Round(((float)player.GameWins/(player.GameWins + player.GameLosses))*100, 2);

                player.TieBreaker3 = (float)Math.Round((opponents.Select(o => Math.Max(0.33f, (float)o.GameWins / (o.GameWins+o.GameLosses))).Sum() /
                                     opponents.Count())*100, 2);

                if (float.IsNaN(player.TieBreaker1))
                    player.TieBreaker1 = 33.33f;

                if (float.IsNaN(player.TieBreaker2))
                    player.TieBreaker2 = 33.33f;

                if (float.IsNaN(player.TieBreaker3))
                    player.TieBreaker3 = 33.33f;
            }

            var i = 0;

            return Players
                .OrderByDescending(p => p.Points)
                .ThenByDescending(p => p.TieBreaker1)
                .ThenByDescending(p => p.TieBreaker2)
                .ThenByDescending(p => p.TieBreaker3)
                .Select(p => new
                    {
                        Place = ++i,
                        Player = p
                    }).ToArray();
        }

        public void DropPlayer(int playerId)
        {
            Players.Single(p => p.PlayerId == playerId).DropRound = CurrentRound;
        }

        /*public TournMatchArray PairNextRound()
        {
            if (Players.Count < 2)
                throw new InvalidOperationException("Need more then 2 players to pair.");

            if (CurrentRound > 0 && Matches.GetByRound(CurrentRound).Any(c => !c.ReportedResult))
                throw new InvalidOperationException("There are still outstanding results.");

            var pairings = new List<TournMatch>();
            var nextRound = CurrentRound + 1;

            var tempPlayers = Players.Shuffle().OrderByDescending(p => p.Points).ToList();

            //var tempPlayers = ((TournPlayer[])Players.Clone).Shuffle().OrderByDescending(p => p.Points).ToList();
            while (tempPlayers.Count > 1)
            {
                var player = tempPlayers.First();
                var previousOpponents = Matches.Where(p => p.Player1 == player || p.Player2 == player)
                                              .Select(p => p.Player1 == player ? p.Player2 : p.Player1);

                //Find the best player
                var match = tempPlayers.FirstOrDefault(p => p != player);

                if (match == null)
                    throw new InvalidOperationException("Unable to find match for player.");

                TournMatch thispair = new TournMatch(player, match, nextRound);
                if (!previousOpponents.Contains(match))
                {
                    pairings.Add(thispair);
                    tempPlayers.Remove(player);
                    tempPlayers.Remove(match);
                }
                else
                {
                    Console.WriteLine("Played Previously: {0}", thispair);
                    int samepoints = player.Wins;
                    int samepoints2 = match.Wins;
                    var playertier =
                        pairings.Where(
                            tierpair => (tierpair.Player1.Wins == samepoints || tierpair.Player2.Wins == samepoints || tierpair.Player1.Wins == samepoints2 || tierpair.Player2.Wins == samepoints2)).ToArray();

                    foreach (var thistierpair in playertier)
                    {
                        pairings.Remove(thistierpair);
                        tempPlayers.Add(thistierpair.Player1);
                        tempPlayers.Add(thistierpair.Player2);
                        tempPlayers.Shuffle();
                    }
                }
            }

            if (tempPlayers.Count > 0)
                pairings.Add(new TournMatch(tempPlayers.First(), null, nextRound));
            
            foreach (var pairing in pairings)
            {
                AddMatch(pairing);
            }

            return new TournMatchArray(pairings);
        }*/


        public TournMatchArray PairNextRound()
        {
            if (Players.Count < 2)
                throw new InvalidOperationException("Need more then 2 players to pair.");

            if (CurrentRound > 0 && Matches.GetByRound(CurrentRound).Any(c => !c.ReportedResult))
                throw new InvalidOperationException("There are still outstanding results.");

            _pairings = new List<TournMatch>();
            _nextRound = CurrentRound + 1;

            _tempPlayers = Players.Where(p => p.DropRound == -1).Shuffle().OrderByDescending(p => p.Wins).ThenBy(p => Guid.NewGuid()).ToList();

            //var tempPlayers = ((TournPlayer[])Players.Clone).Shuffle().OrderByDescending(p => p.Points).ToList();
            while (_tempPlayers.Count > 1)
            {
                NextPair();
            }

            if (_tempPlayers.Count > 0)
                _pairings.Add(new TournMatch(_tempPlayers.First(), null, _nextRound));

            foreach (var pairing in _pairings)
            {
                AddMatch(pairing);
            }

            return new TournMatchArray(_pairings);
        }

        private void NextPair()
        {
            var player = _tempPlayers.Shuffle().OrderByDescending(p => p.Wins).First();

            var previousOpponents = Matches.Where(p => p.Player1 == player || p.Player2 == player)
                                          .Select(p => p.Player1 == player ? p.Player2 : p.Player1);

            //Find the best player
            //can't add ThenBy points as it can create infinte loops
            var match = _tempPlayers.OrderByDescending(p => p.Wins).ThenBy(p => Guid.NewGuid()).ToList().FirstOrDefault(p => !previousOpponents.Contains(player) && p != player);



            if (match == null)
                throw new InvalidOperationException("Unable to find match for player:" + player.PlayerName + "tempPlayers: " + _tempPlayers.Count + " " + _tempPlayers[0].PlayerName + " " + _tempPlayers[1].PlayerName);

            TournMatch thispair = new TournMatch(player, match, _nextRound);
            if (!previousOpponents.Contains(match))
            {
                _pairings.Add(thispair);
                _tempPlayers.Remove(player);
                _tempPlayers.Remove(match);
            }
            else
            {
                Console.WriteLine("Caught a Re-pair: {0}", thispair.ToString());
                /*if (cycle >= 10)
                {
                    Thread.Sleep(5000);
                }*/
                int samepoints = player.Wins;
                var playertier =
                    _pairings.Where(
                        tierpair => (tierpair.Player1.Wins == samepoints || tierpair.Player2.Wins == samepoints)).ToArray();

                foreach (TournMatch thistierpair in playertier)
                {
                    _pairings.Remove(thistierpair);
                    _tempPlayers.Add(thistierpair.Player1);
                    _tempPlayers.Add(thistierpair.Player2);
                }

                var equals = _tempPlayers.Where(equalplayer => equalplayer.Wins == player.Wins).ToArray();
                if (equals.Length == 2)
                {
                    Console.WriteLine("Pair Down");
                    _tempPlayers.Remove(player);
                    NextPair();
                    _tempPlayers.Add(player);
                }
            }
            return;
        }

        /*private TournMatchArray ResolveSwaps(List<PlayerWithOpponents> players, TournMatchArray pairings, TournMatch badMatch, int lowerPointTier, int upperPointTier)
        {
            foreach (var pair in pairings.Where(pair => pair.Player2 != null &&
                                                        (pair.Player1.Points >= lowerPointTier ||
                                                         pair.Player2.Points >= lowerPointTier) &&
                                                        (pair.Player2.Points <= upperPointTier ||
                                                         pair.Player2.Points >= upperPointTier)))
            {
                //See if we can't swap
                if (Opponents(badMatch.Player1).Contains(pair.Player1) ||
                    Opponents(badMatch.Player2).Contains(pair.Player2)) continue;

                //These two can be swapped safely
                var tempPlayer = badMatch.Player1;
                badMatch.Player1 = pair.Player1;
                pair.Player1 = tempPlayer;
                return pairings;
            }

            foreach (var pair in pairings.Where(pair => pair.Player2 != null &&
                                                        (pair.Player1.Points >= lowerPointTier ||
                                                         pair.Player2.Points >= lowerPointTier) &&
                                                        (pair.Player2.Points <= upperPointTier ||
                                                         pair.Player2.Points >= upperPointTier)))
            {

            }

            if ( lowerPointTier == 0 && upperPointTier == Players.Max(p => p.Points) )
                throw new Exception("Unable to find match");

            /*foreach (var pair in pairings.Where(pair => pair.Player2 != null && players.Single(p => p.Player == pair.Player1).Opponents.Contains(pair.Player2)))
            {
                //Bad Pair
            }

            return pairings;
        }*/
    }
}