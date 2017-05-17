using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.WebService.GameDemo
{
    public class DemoGame
    {
        private Random _rnd;
        private EnPoint _target;

        public DemoGame(Random rnd, GameDemoReq req)
        {
            _rnd = rnd;
        }

        public GameDemoResp PerformMove(GameDemoReq req)
        {
            var resp = new GameDemoResp
            {
                Moves = new List<EnPlayerMove>()
            };
            bool dirtyTricks = req.Players[req.YourIndex].RatPositions.Length > 1;
            foreach (EnRatPosition rat in req.Players[req.YourIndex].RatPositions)
            {
                var move = MoveRat(req, rat.Position, rat.RatId, dirtyTricks);
                move.RatId = rat.RatId;
                resp.Moves.Add(move);
            }
            return resp;
        }

        private EnPlayerMove MoveRat(GameDemoReq view, EnPoint pos, int ratNo, bool dirtyTricks)
        {
            // Eat cookie by priority
            if (view.Map[pos.Row, pos.Col] == '.')
                return new EnPlayerMove
                {
                    Action = "Eat"
                };
            // Rat 0 is more intelligent than the others
            if (view.YourIndex == 0)
            {
                dirtyTricks = true;
            }
            else
            {
                if (ratNo == 0)
                    return MoveRatToTarget(view, pos, ratNo);
            }
            // Explode if another rat is in sight
            if (dirtyTricks &&
                view.Players.Where((p, i) => i != view.YourIndex).SelectMany(p => p.RatPositions).Any(p => p.Position.Row == pos.Row && p.Position.Col == pos.Col))
                return new EnPlayerMove
                {
                    Action = "Explode"
                };
            // Walk around
            var targets = new List<EnPoint>();
            bool hitWall;
            if (dirtyTricks && randomNumber(100) < 10)
                hitWall = true;
            else
                hitWall = false;
            CheckTarget(targets, view, pos.Row, pos.Col + 1, hitWall);
            CheckTarget(targets, view, pos.Row + 1, pos.Col, hitWall);
            CheckTarget(targets, view, pos.Row, pos.Col - 1, hitWall);
            CheckTarget(targets, view, pos.Row - 1, pos.Col, hitWall);
            switch (targets.Count)
            {
                case 0:
                    // Just stay
                    return new EnPlayerMove
                    {
                        Action = "Move",
                        Position = pos
                    };
                case 1:
                    // Go in the only direction possible
                    return new EnPlayerMove
                    {
                        Action = "Move",
                        Position = targets[0]
                    };
                default:
                    // Go random
                    return new EnPlayerMove
                    {
                        Action = "Move",
                        Position = targets[randomNumber(targets.Count)]
                    };
            }
        }

        private void CheckTarget(List<EnPoint> good, GameDemoReq view, int row, int col, bool hitWall)
        {
            if (row < 0 || row >= view.Map.Height)
                return;
            if (col < 0 || col >= view.Map.Width)
                return;
            switch (view.Map[row, col])
            {
                case ' ':
                case '.':
                    good.Add(new EnPoint { Row = row, Col = col });
                    break;
                case '#':
                    if (hitWall)
                        good.Add(new EnPoint { Row = row, Col = col });
                    break;
            }
        }

        private EnPlayerMove MoveRatToTarget(GameDemoReq req, EnPoint pos, int ratId)
        {
            // Find target
            if (_target == null || req.Map[_target.Row, _target.Col] != '.')
            {
                var options = new List<EnPoint>();
                for (int row = 0; row < req.Map.Height; row++)
                {
                    for (int col = 0; col < req.Map.Width; col++)
                    {
                        if (req.Map[row, col] != '.')
                            continue;
                        EnPoint target = new EnPoint(row, col);
                        int distance = shortestPath(req.Map, pos, target);
                        if (distance < 1000000)
                            options.Add(target);
                    }
                }
                if (options.Count == 1)
                {
                    _target = options[0];
                }
                else if (options.Count > 0)
                {
                    _target = options[_rnd.Next(options.Count)];
                }
                else
                {
                    _target = null;
                }
            }
            // Prepare to move
            var targets = new List<EnPoint>();
            CheckTarget(targets, req, pos.Row, pos.Col + 1, false);
            CheckTarget(targets, req, pos.Row + 1, pos.Col, false);
            CheckTarget(targets, req, pos.Row, pos.Col - 1, false);
            CheckTarget(targets, req, pos.Row - 1, pos.Col, false);
            // Limited options resolve immediately
            if (targets.Count == 1)
            {
                // Go in the only direction possible
                return new EnPlayerMove
                {
                    Action = "Move",
                    Position = targets[0]
                };
            }
            else if (targets.Count == 0)
            {
                // Just stay
                return new EnPlayerMove
                {
                    Action = "Move",
                    Position = pos
                };
            }
            // Move towards target
            if (_target != null)
            {
                Tuple<int, int>[] values = new Tuple<int, int>[targets.Count];
                for (int j = 0; j < values.Length; j++)
                {
                    values[j] = Tuple.Create(shortestPath(req.Map, targets[j], _target), j);
                }
                Array.Sort(values, (a, b) => a.Item1 - b.Item1);
                int good = values[0].Item1;
                values = values.Where(t => t.Item1 == good).ToArray();
                return new EnPlayerMove
                {
                    Action = "Move",
                    Position = targets[values[randomNumber(values.Length)].Item2]
                };
            }
            // Without target, move randomly
            return new EnPlayerMove
            {
                Action = "Move",
                Position = targets[randomNumber(targets.Count)]
            };
        }

        private int randomNumber(int maxValue)
        {
            lock (_rnd)
            {
                return _rnd.Next(maxValue);
            }
        }

        private int shortestPath(EnMapData map, EnPoint source, EnPoint target)
        {
            if (source.Equals(target))
                return 0;
            int[,] crumbs = new int[map.Height, map.Width];
            for (int row = 0; row < map.Height; row++)
            {
                for (int col = 0; col < map.Width; col++)
                {
                    crumbs[row, col] = 1000000;
                }
            }
            Queue<EnPoint> front = new Queue<EnPoint>();
            crumbs[source.Row, source.Col] = 0;
            front.Enqueue(source);
            while (front.Count > 0)
            {
                EnPoint a = front.Dequeue();
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr * dc != 0 || dr + dc == 0)
                            continue;
                        EnPoint b = new EnPoint(a.Row + dr, a.Col + dc);
                        if (b.Row < 0 || b.Row >= map.Height)
                            continue;
                        if (b.Col < 0 || b.Col >= map.Width)
                            continue;
                        if (map[b] == '#')
                            continue;
                        int v = crumbs[a.Row, a.Col] + 1;
                        if (crumbs[b.Row, b.Col] <= v)
                            continue;
                        crumbs[b.Row, b.Col] = v;
                        if (b.Equals(target))
                            return v;
                        front.Enqueue(b);
                    }
                }
            }
            return 2000000;
        }
    }
}