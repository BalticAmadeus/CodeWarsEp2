using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.WebService.Model
{
    [DataContract]
    public class GetPlayerViewReq : BaseReq
    {
        [DataMember]
        public int PlayerId;
    }

    [DataContract]
    public class GetPlayerViewResp : BaseResp
    {
        [DataMember]
        public string GameUid;

        [DataMember]
        public int Turn;

        [DataMember]
        public EnMapData Map;

        [DataMember]
        public EnPlayerState[] Players;

        [DataMember]
        public int YourIndex;

        [DataMember]
        public int LastTurn;
    }
}
