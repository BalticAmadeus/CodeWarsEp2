using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.WebService.Model
{
    [DataContract]
    public class PerformMoveReq : BaseReq
    {
        [DataMember]
        public int PlayerId;

        [DataMember]
        public List<EnPlayerMove> Moves;
    }

    [DataContract]
    public class PerformMoveResp : BaseResp
    {
        // default
    }
}
