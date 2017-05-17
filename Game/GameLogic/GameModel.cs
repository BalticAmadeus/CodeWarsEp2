using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GameLogic
{
    public class GameModel : IDisposable
    {
        public int GameId { get; private set; }
        public Team Owner { get; private set; }
        public long GameUid { get; private set; }
        
        private GameState _state;
        private readonly List<Player> _players;
        private readonly List<ObserverQueue> _observers;
        private MapData _map;
        private readonly GameProtocol _proto;

        private object _liveLock;
        private Dictionary<int, int> _indexes;
        private PlayerState[] _playerStates;
        private int _gameTurnEnded;
        private int _gameTurnStarted;
        private DateTime _turnStart;
        private DateTime _turnEnd;
        private readonly int _turnDuration;

        public GameModel(long uidBase, int gameId, Team owner)
        {
            GameId = gameId;
            Owner = owner;
            GameUid = uidBase + GameId;
            State = GameState.Setup;
            _players = new List<Player>();
            _observers = new List<ObserverQueue>();
            _proto = new GameProtocol(this);
            _turnDuration = Settings.DefaultGameTurnDurationMillis;
        }

        public GameState State
        {
            get
            {
                return _state;
            }

            private set
            {
                _state = value;
                _proto?.LogGameState(_state);
            }
        }

        public string Label
        {
            get
            {
                var label = new StringBuilder($"Game #{GameId} [{Owner}]");

                if (_players != null && _players.Any())
                {
                    label.Append(": ");
                    bool first = true;
                    bool sameTeam = !_players.Any(p => !p.Team.Equals(_players[0].Team));
                    if (Owner.PowerTeam && sameTeam)
                        label.Append("[").Append(_players[0].Team.Name).Append("] ");
                    foreach (Player p in _players)
                    {
                        if (!first)
                            label.Append(" vs ");

                        if (Owner.PowerTeam && !sameTeam)
                            label.Append(p.Team.Name);
                        else
                            label.Append(p.Name);

                        first = false;
                    }
                }

                return label.ToString();
            }
        }

        public void Dispose()
        {
            _proto.Dispose();
        }

        private void CheckSetupState()
        {
            if (State != GameState.Setup)
                throw new ApplicationException("Game must be in SETUP state");
        }

        public void CheckRunState()
        {
            if (State == GameState.Setup)
                throw new ApplicationException("Game must be started");
        }

        public void CheckPlayState()
        {
            if (State != GameState.Play && State != GameState.Pause)
                throw new ApplicationException("Game must be playing");
        }

        public void CheckGameDeletable()
        {
            if (State == GameState.Setup)
                return;
            lock (_liveLock)
            {
                if (State != GameState.Finish)
                    throw new ApplicationException("Game must be finished");
                //if (_playerStates.Any(p => p.IsPresent)) // Should never happen but just as a precaution
                //    throw new ApplicationException("All players must leave");
            }
        }

        public void AddPlayer(Player player)
        {
            CheckSetupState();
            if (player.Game != null)
                throw new ApplicationException("Player already in a game");
            _players.Add(player);
            player.Game = this;
        }

        public void RemovePlayer(Player player)
        {
            CheckSetupState();
            if (player.Game != this)
                throw new ApplicationException("Player is not in this game");
            _players.Remove(player);
            player.Game = null;
        }

        public IEnumerable<Player> ListPlayers()
        {
            return _players;
        }

        public void SetMap(MapData mapData)
        {
            CheckSetupState();
            _map = mapData;
        }

        public void Start()
        {
            CheckSetupState();
            if (_players.Count < 1)
                throw new ApplicationException("Number of players in the game must be at least 1");
            if (_map == null)
                throw new ApplicationException("Map is not loaded");
            if (_map.RatPositions.Length < _players.Count)
                throw new ApplicationException($"There are {_players.Count} players in the game but only {_map.RatPositions.Length} starting positions on the map");
            int cookies = 0;
            for (int row = 0; row < _map.Height; row++)
            {
                for (int col = 0; col < _map.Width; col++)
                {
                    if (_map[row, col] == TileType.Cookie)
                        cookies++;
                }
            }
            _map.RemainingCookies = cookies;

            _indexes = _players.Select((p, i) => Tuple.Create(p.PlayerId, i)).ToDictionary(k => k.Item1, v => v.Item2);

            // Setup player initial states
            _playerStates = new PlayerState[_players.Count];
            for (int p = 0; p < _playerStates.Length; p++)
            {
                _playerStates[p] = new PlayerState
                {
                    Index = p,
                    PlayerId = _players[p].PlayerId,
                    Condition = PlayerCondition.Play,
                    IsPresent = true,
                    TurnFinTime = default(DateTime),
                    PenaltyPoints = 0,
                    BonusPoints = 0,
                    OvertimeTurnMsec = 0,
                    OvertimeTurnTurn = -1,
                    PenaltyThresholdReachedTurn = -1,
                    Score = 0
                };
            }

            _gameTurnEnded = 0;
            _gameTurnStarted = 0;
            PrepareTurnTimings();
            _liveLock = new object();

            // Start paused
            State = GameState.Pause;
            _proto.LogGameStart(this);
        }

        private void PrepareTurnTimings()
        {
            _turnStart = DateTime.Now;
            _turnEnd = _turnStart.AddMilliseconds(_turnDuration);
        }

        private int FindPlayer(int playerId)
        {
            int index;
            if (!_indexes.TryGetValue(playerId, out index))
                throw new ApplicationException("Player is not in this game");
            return index;
        }

        private ObserverQueue FindObserver(int observerId)
        {
            ObserverQueue q = _observers.FirstOrDefault(p => p.Observer.ObserverId == observerId);
            if (q == null)
                throw new ApplicationException("This observer does not watch this game");
            return q;
        }

        public GameViewInfo GetGameView(int playerId)
        {
            lock (_liveLock)
            {
                var gv = new GameViewInfo();

                gv.GameUid = GameUid;

                if (playerId > 0)
                {
                    int p = FindPlayer(playerId);
                    if (!_playerStates[p].IsActive || _playerStates[p].TurnCompleted >= _gameTurnStarted)
                        throw new WaitException();
                    gv.PlayerIndex = p;
                }

                gv.GameState = State;
                gv.Turn = _gameTurnStarted;
                gv.PlayerStates = _playerStates.Select(s => new PlayerStateInfo(s, _map.RatPositions[s.Index])).ToArray();
                gv.Map = (MapData)_map.Clone();
                gv.LastTurn = Settings.GameTurnLimit;
                return gv;
            }
        }

        public void PerformMove(int playerId, RatMove[] moves)
        {
            lock (_liveLock)
            {
                CheckPlayState();
                int p = FindPlayer(playerId);
                if (!_playerStates[p].IsActive)
                    throw new ApplicationException("You cannot make more moves");

                if (_playerStates[p].TurnCompleted >= _gameTurnStarted)
                    throw new WaitException();

                // Just record the move
                _playerStates[p].TurnMoves = moves;
                _proto.LogMove(p, playerId, moves);
            }
        }

        private bool PlayerExitTest(WaitTurnInfo wi)
        {
            int p = wi.PlayerIndex;
            if (!_playerStates[p].IsActive)
            {
                wi.TurnComplete = true;
                wi.GameFinished = true;
                wi.FinishCondition = _playerStates[p].Condition;
                wi.FinishComment = _playerStates[p].Comment;
                return true;
            }
            return false;
        }

        public WaitTurnInfo CompletePlayerTurn(int playerId, int refTurn, DateTime callTimestamp)
        {
            lock (_liveLock)
            {
                WaitTurnInfo wi = new WaitTurnInfo();
                int player = FindPlayer(playerId);
                if (_playerStates[player].IsPresent == false)
                    throw new ApplicationException("Player was dropped from the game");
                wi.PlayerIndex = player;

                if (refTurn == 0)
                {
                    // Crash recovery logic
                    if (_playerStates[player].TurnCompleted < _gameTurnStarted)
                    {
                        wi.TurnComplete = true;
                        return wi;
                    }
                    else
                    {
                        wi.Turn = _playerStates[player].TurnCompleted;
                        return wi;
                    }
                }

                if (_playerStates[player].TurnCompleted == refTurn)
                {
                    wi.Turn = refTurn;
                    return wi;
                }

                if (PlayerExitTest(wi))
                    return wi;

                if (_playerStates[player].TurnCompleted != refTurn - 1)
                    throw new ApplicationException($"Player is confusing turns: completed={_playerStates[player].TurnCompleted} refTurn={refTurn}");
                if (refTurn > _gameTurnStarted)
                    throw new ApplicationException($"Player skipping ahead of game progress: gameTurnStarted={_gameTurnStarted} refTurn={refTurn}");

                _playerStates[player].TurnCompleted = _gameTurnStarted;
                _playerStates[player].TurnFinTime = callTimestamp;

                #region Penalty logic
                int totalMsec = (int)(_playerStates[player].TurnFinTime - _turnStart).TotalMilliseconds;
                if (totalMsec > Settings.TurnResponseThresholdMillis)
                {
                    _playerStates[player].PenaltyPoints += (totalMsec - Settings.TurnResponseThresholdMillis) / 100;
                    // TODO OvertimeTurn stuff is of questionable value to us. Ditto Penalty Threshold, but the latter is at least understandable.
                    _playerStates[player].OvertimeTurnMsec = totalMsec;
                    _playerStates[player].OvertimeTurnTurn = _playerStates[player].TurnCompleted;
                    if (_playerStates[player].PenaltyPoints > Settings.PenaltyPointsThreshold && _playerStates[player].PenaltyThresholdReachedTurn < 0)
                        _playerStates[player].PenaltyThresholdReachedTurn = _playerStates[player].TurnCompleted;
                }
                else
                {
                    _playerStates[player].BonusPoints += (Settings.TurnResponseThresholdMillis - totalMsec) / 100;
                }
                #endregion

                _proto.LogPlayerTurnComplete(_playerStates[player], _turnStart);

                CompleteTurn();

                if (PlayerExitTest(wi))
                    return wi;

                wi.Turn = refTurn;
                return wi;
            }
        }

        private bool StartNextTurnMaybe()
        {
            if (_gameTurnStarted > _gameTurnEnded)
                return true;
            if (State != GameState.Play)
                return false;
            if ((_turnEnd - DateTime.Now).TotalMilliseconds > Settings.GameTurnDurationAccuracyMillis)
                return false;
            _gameTurnStarted = _gameTurnEnded + 1;
            PrepareTurnTimings();
            foreach (PlayerState ps in _playerStates)
            {
                ps.TurnMoves = null;
            }
            Monitor.PulseAll(_liveLock);
            _proto.LogGameTurnStart(_gameTurnStarted);
            return true;
        }

        private void CompleteTurn()
        {
            if (StartNextTurnMaybe() == false)
                return;
            if (_playerStates.Any(t => t.IsActive && t.TurnCompleted < _gameTurnStarted))
                return;

            var mapChanges = new List<ObservedChange>();
            var newPositions = _map.RatPositions.Select(p => new List<Point>(p)).ToArray();
            var killedRats = new HashSet<Point>(); /* [playerIndex, ratIndex] */

            // Validate moves
            foreach (PlayerState ps in _playerStates.Where(p => p.IsActive))
            {
                try
                {
                    if (ps.TurnMoves == null)
                        throw new RulesException("PerformMove wasn't called or move information was not specified");

                    var ratsMoved = new HashSet<int>();

                    // Check every move
                    foreach (RatMove move in ps.TurnMoves)
                    {
                        if (move == null)
                            throw new RulesException($"PerformMoveReq.Moves contains null elements");
                        int ratId = move.RatId;
                        if (ratsMoved.Contains(ratId))
                            throw new RulesException($"Duplicate move for rat {ratId}");
                        ratsMoved.Add(ratId);
                        if (ratId < 0 || ratId >= _map.RatPositions[ps.Index].Count)
                            throw new RulesException($"RatId of {ratId} is invalid");
                        if (_map.RatPositions[ps.Index][ratId].IsDead)
                            throw new RulesException($"Tried to move a dead rat {ratId}");
                        switch (move.Action)
                        {
                            case RatAction.Move:
                                if (!move.Position.HasValue)
                                    throw new RulesException($"Rat {ratId} moved to unspecified Position");
                                if (!_map.InBounds(move.Position.Value))
                                    throw new RulesException($"Rat {ratId} moved out of map bounds");
                                if (GetMovementDirection(_map.RatPositions[ps.Index][ratId], move.Position.Value) == MovementDirection.Invalid)
                                    throw new RulesException($"Rat {ratId} made an illegal move on the map");
                                break;
                            case RatAction.Eat:
                                // It is always a correct move even if there is nothing to eat
                                break;
                            case RatAction.Explode:
                                // It is always a correct move, although a sad one
                                break;
                            default:
                                // This is bad but rather than crashing a server we kill the player and complain about it
                                throw new RulesException($"Server Error: unsupported move.Action {move.Action}");
                        }
                    }
                }
                catch (RulesException re)
                {
                    ps.Condition = PlayerCondition.Draw;
                    ps.Comment = re.Message;
                    _proto.LogPlayerCondition(ps);
                    // Also kill all player's rats
                    killedRats.UnionWith(_map.RatPositions[ps.Index].Select((r, i) => new Point(ps.Index, i)));
                    continue;
                }
            }

            // Do the cookie eating
            {
                var cookieBites = new Dictionary<Point, HashSet<int>>(); /* cookie position -> players who want to eat it */

                // -- Gather all bites on each cookie
                foreach (PlayerState ps in _playerStates.Where(p => p.IsActive))
                {
                    foreach (RatMove move in ps.TurnMoves.Where(m => m.Action == RatAction.Eat))
                    {
                        Point where = _map.RatPositions[ps.Index][move.RatId];

                        // There must be a cookie there in the first place
                        if (_map[where] != TileType.Cookie)
                            continue;

                        // Register a bite
                        HashSet<int> biters;
                        if (!cookieBites.TryGetValue(where, out biters))
                        {
                            biters = new HashSet<int>();
                            cookieBites[where] = biters;
                        }
                        biters.Add(ps.Index);
                    }
                }

                // -- Eat the cookies
                foreach (KeyValuePair<Point, HashSet<int>> bite in cookieBites)
                {
                    Point where = bite.Key;
                    HashSet<int> biters = bite.Value;
                    // Count scores
                    if (biters.Count == 1)
                    {
                        int playerIdx = biters.First();
                        _playerStates[playerIdx].Score += 2;
                    }
                    else
                    {
                        foreach (int playerIdx in biters)
                        {
                            _playerStates[playerIdx].Score += 1;
                        }
                    }
                    // Clear the cookie
                    _map[where] = TileType.Empty;
                    _map.RemainingCookies--;
                    mapChanges.Add(new ObservedChange
                    {
                        ChangeType = ObservedChangeType.Eat,
                        Target = where
                    });
                }
            }

            // Resolve explosions that happen as an action
            {
                var killZone = new HashSet<Point>();

                // -- Build kill zone
                foreach (PlayerState ps in _playerStates.Where(p => p.IsActive))
                {
                    foreach (RatMove move in ps.TurnMoves.Where(m => m.Action == RatAction.Explode))
                    {
                        Point where = _map.RatPositions[ps.Index][move.RatId];

                        // Mark as kill zone
                        if (!killZone.Contains(where))
                        {
                            killZone.Add(where);
                            mapChanges.Add(new ObservedChange
                            {
                                ChangeType = ObservedChangeType.Explode,
                                Target = where
                            });
                        }
                    }
                }

                // -- Kill everyone in kill zone
                foreach (PlayerState ps in _playerStates.Where(p => p.IsActive))
                {
                    for (int i = 0; i < _map.RatPositions[ps.Index].Count; i++)
                    {
                        Point where = _map.RatPositions[ps.Index][i];
                        if (killZone.Contains(where))
                            killedRats.Add(new Point(ps.Index, i));
                    }
                }
            }

            // Process moves on the map
            {
                var wallBreaks = new HashSet<Point>();

                // -- Process move actions
                foreach (PlayerState ps in _playerStates.Where(p => p.IsActive))
                {
                    foreach (RatMove move in ps.TurnMoves.Where(m => m.Action == RatAction.Move))
                    {
                        Point whereTo = move.Position.Value;

                        // Check if we are moving into a wall
                        if (_map[whereTo] == TileType.Wall)
                        {
                            // Break a wall
                            wallBreaks.Add(whereTo);
                            // Kill the rat
                            killedRats.Add(new Point(ps.Index, move.RatId));
                        }
                        else
                        {
                            // Just move
                            newPositions[ps.Index][move.RatId] = whereTo;
                        }
                        mapChanges.Add(new ObservedChange
                        {
                            ChangeType = ObservedChangeType.Move,
                            PlayerIndex = ps.Index,
                            RatIndex = move.RatId,
                            Target = whereTo
                        });
                    }
                }

                // -- Break the walls
                foreach (Point where in wallBreaks)
                {
                    _map[where] = TileType.Empty;
                    mapChanges.Add(new ObservedChange
                    {
                        ChangeType = ObservedChangeType.Demolish,
                        Target = where
                    });
                }
            }

            // Resolve new rat positions and remove dead ones
            {
                foreach (Point dr in killedRats)
                {
                    newPositions[dr.Row][dr.Col] = Point.DeadRat;
                    mapChanges.Add(new ObservedChange
                    {
                        ChangeType = ObservedChangeType.Casualty,
                        PlayerIndex = dr.Row,
                        RatIndex = dr.Col
                    });
                }
                _map.RatPositions = newPositions;
            }

            // Check fair game finish conditions
            if (_playerStates.Where(p => p.IsActive).Any())
            {
                do
                {
                    List<Tuple<int, int>> scores; /* [playerIndex, score] */
                    scores = _playerStates.Where(p => p.IsActive).Select(p => Tuple.Create(p.Index, p.Score)).ToList();
                    scores.Sort((a, b) => b.Item2 - a.Item2); // by score descending
                    // Game ends when:
                    // - no cookies are left
                    // - turn limit is reached
                    // - all rats have died
                    if (_map.RemainingCookies <= 0
                        || _gameTurnStarted >= Settings.GameTurnLimit
                        || _playerStates.Where(p => p.IsActive).All(p => _map.RatPositions[p.Index].All(r => r.IsDead)))
                    {
                        int winnerScore = scores[0].Item2;
                        PlayerCondition winnerCondition;
                        if (scores.Count > 1 && scores[1].Item2 == winnerScore)
                            winnerCondition = PlayerCondition.Draw;
                        else if (_playerStates.Length == 1 && _map.RemainingCookies > 0)
                            winnerCondition = PlayerCondition.Lost;
                        else
                            winnerCondition = PlayerCondition.Won;
                        string comment;
                        if (_map.RemainingCookies <= 0)
                            comment = "No cookies left";
                        else if (_gameTurnStarted >= Settings.GameTurnLimit)
                            comment = "Game turn limit reached";
                        else
                            comment = "All rats have died";
                        foreach (Tuple<int, int> e in scores)
                        {
                            int p = e.Item1;
                            int score = e.Item2;
                            if (score == winnerScore)
                            {
                                _playerStates[p].Condition = winnerCondition;
                                _playerStates[p].Comment = comment;
                                _proto.LogPlayerCondition(_playerStates[p]);
                            }
                            else
                            {
                                _playerStates[p].Condition = PlayerCondition.Lost;
                                _playerStates[p].Comment = comment;
                                _proto.LogPlayerCondition(_playerStates[p]);
                            }
                        }
                        break;
                    }
                } while (false);
            }

            // Handle technical game finish condition
            var activePlayers = _playerStates.Where(p => p.IsActive);
            int activePlayerCount = activePlayers.Count();
            if (_playerStates.Length == 1 &&  activePlayerCount == 0 ||
                _playerStates.Length > 1 && activePlayerCount <= 1)
            {
                // Game finishes
                State = GameState.Finish;
                if (activePlayers.Count() == 1)
                {
                    // We have a winner
                    PlayerState winner = activePlayers.Single();
                    winner.Condition = PlayerCondition.Won;
                    winner.Comment = "Congratulations !";
                    _proto.LogPlayerCondition(winner);
                    foreach (PlayerState p in _playerStates)
                    {
                        if (p.Condition == PlayerCondition.Draw)
                        {
                            p.Condition = PlayerCondition.Lost;
                            _proto.LogPlayerCondition(p);
                        }
                    }
                }
                // else we are in a draw
            }

            // Complete the turn
            _gameTurnEnded = _gameTurnStarted;
            _proto.LogGameTurnEnd(_gameTurnEnded);

            // Notify observers
            ObservedTurnInfo ot = new ObservedTurnInfo
            {
                Turn = _gameTurnEnded,
                GameState = State,
                PlayerStates = _playerStates.Select(p => new PlayerStateInfo(p, _map.RatPositions[p.Index])).ToArray(),
                MapChanges = mapChanges.ToArray(),
            };

            foreach (ObserverQueue queue in _observers)
            {
                queue.Push(ot);
            }

            // Continue
            StartNextTurnMaybe();
        }

        private MovementDirection GetMovementDirection(Point from, Point to)
        {
            int rowDiff = to.Row - from.Row;
            int colDiff = to.Col - from.Col;
            if (rowDiff == 0 && colDiff == 0)
                return MovementDirection.Stay;
            else if (rowDiff != 0 && colDiff != 0)
                return MovementDirection.Invalid;
            else if (rowDiff == 1)
                return MovementDirection.Down;
            else if (rowDiff == -1)
                return MovementDirection.Up;
            else if (colDiff == 1)
                return MovementDirection.Right;
            else if (colDiff == -1)
                return MovementDirection.Left;
            else
                return MovementDirection.Invalid;
        }

        public void WaitNextTurn(WaitTurnInfo wi)
        {
            lock (_liveLock)
            {
                StartNextTurnMaybe();

                if (PlayerExitTest(wi))
                    return;
                if (_gameTurnStarted > wi.Turn)
                {
                    wi.TurnComplete = true;
                    return;
                }

                CheckRunState();
                DateTime dtNow = DateTime.Now;
                if (_turnEnd > dtNow)
                {
                    int waitMillis = (int)(_turnEnd - dtNow).TotalMilliseconds;
                    if (waitMillis > Settings.NextTurnPollTimeoutMillis)
                        waitMillis = Settings.NextTurnPollTimeoutMillis;
                    else if (waitMillis < Settings.MinimumSleepMillis)
                        waitMillis = Settings.MinimumSleepMillis;
                    Monitor.Wait(_liveLock, waitMillis);
                }
                else
                {
                    Monitor.Wait(_liveLock, Settings.NextTurnPollTimeoutMillis);
                }

                StartNextTurnMaybe();

                if (PlayerExitTest(wi))
                    return;
                if (_gameTurnStarted > wi.Turn)
                    wi.TurnComplete = true;
            }
        }

        public void DropPlayer(Player player, string reason)
        {
            lock (_liveLock)
            {
                CheckRunState();
                int p = FindPlayer(player.PlayerId);
                if (!_playerStates[p].IsPresent)
                    return; // It can't happen but we should be ok anyway
                if (_playerStates[p].IsActive)
                {
                    if (_playerStates[p].TurnCompleted < _gameTurnStarted)
                        CompletePlayerTurn(player.PlayerId, _gameTurnStarted, DateTime.Now);
                    _playerStates[p].Condition = PlayerCondition.Lost;
                    _playerStates[p].Comment = string.Format("Dropped from the game ({0})", reason);
                    _proto.LogPlayerCondition(_playerStates[p]);
                    if (State != GameState.Finish && !_playerStates.Any(s => s.IsActive))
                    {
                        State = GameState.Finish;
                    }
                }
                _playerStates[p].IsPresent = false;
                _proto.LogPlayerDrop(p, player.PlayerId, reason);
                player.Game = null;
            }
        }

        public void Pause()
        {
            lock (_liveLock)
            {
                CheckPlayState();
                State = GameState.Pause;
            }
        }

        public void Resume()
        {
            lock (_liveLock)
            {
                CheckPlayState();
                State = GameState.Play;
                Monitor.PulseAll(_liveLock);
            }
        }

        private void RemoveObserver(int observerId)
        {
            _observers.RemoveAll(q => q.Observer.ObserverId == observerId);
        }

        private GameViewInfo AddObserver(Observer observer)
        {
            _observers.Add(new ObserverQueue(observer));
            return GetGameView(-1);
        }

        public GameViewInfo StartObserving(Observer observer)
        {
            lock (_liveLock)
            {
                CheckRunState();
                RemoveObserver(observer.ObserverId);
                return AddObserver(observer);
            }
        }

        public ObservedGameInfo ObserveNextTurn(Observer observer)
        {
            ObserverQueue queue;
            List<int> lags = new List<int>();
            DateTime now = DateTime.Now;
            lock (_liveLock)
            {
                CheckRunState();
                queue = FindObserver(observer.ObserverId);
                for (int i = 0; i < _playerStates.Length; i++)
                {
                    if (_playerStates[i].TurnCompleted >= _gameTurnStarted)
                        continue;
                    double delayMillis = (now - _turnStart).TotalSeconds;
                    if (delayMillis > Settings.SlowTurnIntervalSeconds)
                        lags.Add(i);
                }
            }
            int queueLength;
            ObservedTurnInfo ot = queue.Pop(out queueLength); // This has its own blocking
            ObservedGameInfo gi = new ObservedGameInfo
            {
                GameId = GameId,
                GameState = State,
                QueuedTurns = queueLength,
                TurnInfo = ot,
                SlowPlayers = lags
            };
            return gi;
        }

        public GameLiveInfo GetLiveInfo()
        {
            lock (_liveLock)
            {
                CheckRunState();
                GameLiveInfo gi = new GameLiveInfo
                {
                    GameState = State,
                    Turn = _gameTurnStarted,
                    TurnStartTime = _turnStart
                };

                gi.PlayerStates = _playerStates.Select(p => new PlayerLiveInfo
                {
                    PlayerId = p.PlayerId,
                    Team = _players[p.Index].Team,
                    Name = _players[p.Index].Name,
                    Condition = p.Condition,
                    Comment = p.Comment,
                    TurnCompleted = p.TurnCompleted,
                    TurnFinTime = p.TurnFinTime,
                    PenaltyPoints = p.PenaltyPoints,
                    BonusPoints = p.BonusPoints,
                    OvertimeTurnMsec = p.OvertimeTurnMsec,
                    OvertimeTurnTurn = p.OvertimeTurnTurn,
                    PenaltyThresholdReachedTurn = p.PenaltyThresholdReachedTurn,
                    Score = p.Score

                }).ToArray();

                return gi;
            }
        }
    }

    public enum GameState
    {
        Setup, Play, Pause, Finish
    }

    public class PlayerState
    {
        public int Index;
        public int PlayerId;
        public PlayerCondition Condition;
        public bool IsActive => Condition == PlayerCondition.Play;
        public bool IsPresent;
        public string Comment;
        public int TurnCompleted;

        public RatMove[] TurnMoves;

        public DateTime TurnFinTime;
        public int PenaltyPoints;
        public int BonusPoints;
        public int OvertimeTurnMsec;
        public int OvertimeTurnTurn;
        public int PenaltyThresholdReachedTurn;

        public int Score;
    }

    public enum PlayerCondition
    {
        Play, Won, Lost, Draw
    }

    internal enum MovementDirection
    {
        Invalid, Up, Down, Left, Right, Stay
    }

    public class RatMove
    {
        public RatAction Action;
        public int RatId;
        public Point? Position;
    }

    public enum RatAction
    {
        Move, Eat, Explode
    }
}
