using GameLogic;
using System.Linq;
using System.Runtime.Serialization;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnPlayerState
    {
        [DataMember]
        public string Condition;

        [DataMember]
        public string Comment;

        [DataMember]
        public int Score;

        [DataMember]
        public EnRatPosition[] RatPositions;

        public EnPlayerState()
        {
            //default
        }

        public EnPlayerState(PlayerStateInfo ps)
        {
            Condition = ps.Condition.ToString();
            Comment = ps.Comment;
            Score = ps.Score;
            RatPositions = ps.RatPositions.Select(p => new EnRatPosition(p)).ToArray();
        }
    }

    public class EnRatPosition
    {
        public int RatId;
        public EnPoint Position;

        public EnRatPosition()
        {
            // default
        }

        public EnRatPosition(RatPosition rp)
        {
            RatId = rp.RatId;
            Position = new EnPoint(rp.Position);
        }
    }
}