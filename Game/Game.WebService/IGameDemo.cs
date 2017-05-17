using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Game.WebService.GameDemo
{
    [ServiceContract]
    public interface IGameDemo
    {
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        GameDemoResp PerformMove(GameDemoReq req);
    }

    [DataContract]
    public class GameDemoReq
    {
        [DataMember]
        public string GameUid;
        [DataMember]
        public int Turn;
        [DataMember]
        public string Mode;
        [DataMember]
        public EnMapData Map;
        [DataMember]
        public EnPlayerState[] Players;
        [DataMember]
        public int YourIndex;
        [DataMember]
        public int LastTurn;
    }

    [DataContract]
    public class EnMapData
    {
        [DataMember]
        public int Width;
        [DataMember]
        public int Height;
        [DataMember]
        public List<string> Rows;

        public char this[int row, int col]
        {
            get
            {
                return Rows[row][col];
            }
        }

        public char this[EnPoint point]
        {
            get
            {
                return Rows[point.Row][point.Col];
            }
        }
    }

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
    }

    [DataContract]
    public class EnRatPosition
    {
        [DataMember]
        public int RatId;
        [DataMember]
        public EnPoint Position;
    }

    [DataContract]
    public class EnPoint
    {
        [DataMember]
        public int Row;
        [DataMember]
        public int Col;

        public EnPoint()
        {
            // default
        }

        public EnPoint(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            EnPoint other = obj as EnPoint;
            if (other == null)
                return false;
            else
                return this.Row == other.Row && this.Col == other.Col;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Row.GetHashCode();
                hash = hash * 31 + Col.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({Row},{Col})";
        }
    }

    [DataContract]
    public class GameDemoResp
    {
        [DataMember]
        public List<EnPlayerMove> Moves;
    }

    [DataContract]
    public class EnPlayerMove
    {
        [DataMember]
        public string Action;
        [DataMember]
        public int RatId;
        [DataMember]
        public EnPoint Position;
    }

}
