using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class PlayerStateInfo
    {
        public PlayerCondition Condition;
        public string Comment;
        public int Score;
        public RatPosition[] RatPositions;

        public PlayerStateInfo(PlayerState ps, List<Point> rats)
        {
            Condition = ps.Condition;
            Comment = ps.Comment;
            Score = ps.Score;
            if (rats == null)
                RatPositions = null;
            else
            {
                RatPositions = rats.Select((p, i) => new RatPosition { RatId = i, Position = p }).Where(r => !r.Position.IsDead).ToArray();
            }
        }
    }

    public class RatPosition
    {
        public int RatId;
        public Point Position;
    }
}
