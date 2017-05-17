using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnMapChange
    {
        [DataMember]
        public string ChangeType;

        [DataMember]
        public int PlayerIndex;

        [DataMember]
        public int RatIndex;

        [DataMember]
        public EnPoint Target;

        public EnMapChange()
        {
            // default
        }

        public EnMapChange(ObservedChange oc)
        {
            ChangeType = oc.ChangeType.ToString();
            PlayerIndex = oc.PlayerIndex;
            RatIndex = oc.RatIndex;
            Target = (oc.Target.HasValue) ? new EnPoint(oc.Target.Value) : null;
        }
    }
}