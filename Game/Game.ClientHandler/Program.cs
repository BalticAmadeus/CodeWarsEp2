using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Game.ClientHandler
{
    class Program
    {
        private static Random rnd = new Random();

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("USAGE: Game.ClientHandler input.json output.json");
                return;
            }
            // Read player view
            string inputStr = File.ReadAllText(args[0], Encoding.UTF8);
            var playerView = JsonConvert.DeserializeObject<PlayerView>(inputStr);
            // Do the logic
            var playerMove = PerformMove(playerView);
            // Write move
            string outputStr = JsonConvert.SerializeObject(playerMove);
            File.WriteAllText(args[1], outputStr, Encoding.UTF8);
        }

        static PerformMoveReq PerformMove(PlayerView view)
        {
            var resp = new PerformMoveReq
            {
                Moves = new List<EnPlayerMove>()
            };
            foreach (EnRatPosition rat in view.Players[view.YourIndex].RatPositions)
            {
                var move = MoveRat(view, rat.Position, rat.RatId);
                move.RatId = rat.RatId;
                resp.Moves.Add(move);
            }
            return resp;
        }

        static EnPlayerMove MoveRat(PlayerView view, EnPoint pos, int ratNo)
        {
            // Eat cookie by priority
            if (view.Map[pos.Row, pos.Col] == '.')
                return new EnPlayerMove
                {
                    Action = "Eat"
                };
            // Explode if another rat is in sight
            if (view.Players.Where((p, i) => i != view.YourIndex).SelectMany(p => p.RatPositions).Any(p => p.Position.Row == pos.Row && p.Position.Col == pos.Col))
                return new EnPlayerMove
                {
                    Action = "Explode"
                };
            // Walk around
            var targets = new List<EnPoint>();
            bool hitWall;
            if (rnd.Next(100) < 10)
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
                        Position = targets[rnd.Next(targets.Count)]
                    };
            }
        }

        static void CheckTarget(List<EnPoint> good, PlayerView view, int row, int col, bool hitWall)
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
    }

    public class PlayerView
    {
        public int Turn;
        public string Mode;
        public EnMapData Map;
        public EnPlayerState[] Players;
        public int YourIndex;
    }

    public class EnPlayerState
    {
        public string Condition;
        public string Comment;
        public int Score;
        public EnRatPosition[] RatPositions;
    }

    public class EnRatPosition
    {
        public int RatId;
        public EnPoint Position;
    }

    public class EnPoint
    {
        public int Row;
        public int Col;
    }

    public class EnMapData
    {
        public int Width;
        public int Height;
        public List<string> Rows;

        public char this[int row, int col] {
            get
            {
                return Rows[row][col];
            }
        }
    }

    public class PerformMoveReq
    {
        public List<EnPlayerMove> Moves;
    }

    public class EnPlayerMove
    {
        public string Action;
        public int RatId;
        public EnPoint Position;
    }
}