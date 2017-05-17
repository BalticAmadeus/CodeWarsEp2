using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnPlayerMove
    {
        [DataMember]
        public string Action;

        [DataMember]
        public int RatId;

        [DataMember]
        public EnPoint Position;

        public RatMove ToRatMove()
        {
            var move = new RatMove();
            move.Action = ConvertAction(Action);
            move.RatId = RatId;
            if (Position != null)
                move.Position = Position.ToPoint();
            return move;
        }

        private RatAction ConvertAction(string action)
        {
            if (action == null)
                throw new ArgumentException("PlayerMove.Action is missing");
            switch (action)
            {
                case "Move":
                    return RatAction.Move;
                case "Eat":
                    return RatAction.Eat;
                case "Explode":
                    return RatAction.Explode;
                default:
                    throw new ArgumentException("PlayerMove.Action must be one of Move, Eat or Explode");
            }
        }
    }
}
