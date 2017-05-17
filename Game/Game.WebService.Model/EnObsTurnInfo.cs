using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnObsTurnInfo
    {
        [DataMember]
        public int Turn;

        [DataMember]
        public string GameState;

        [DataMember]
        public EnPlayerState[] PlayerStates;

        [DataMember]
        public EnMapChange[] MapChanges;

        public EnObsTurnInfo()
        {
            // default
        }

        public EnObsTurnInfo(ObservedTurnInfo ti)
        {
            Turn = ti.Turn;
            GameState = ti.GameState.ToString();
            PlayerStates = ti.PlayerStates.Select(p => new EnPlayerState(p)).ToArray();
            MapChanges = ti.MapChanges.Select(p => new EnMapChange(p)).ToArray();
        }
    }
}