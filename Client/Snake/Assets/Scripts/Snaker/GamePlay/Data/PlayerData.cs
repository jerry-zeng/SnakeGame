using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace GamePlay
{
    [ProtoContract]
    public class PlayerData 
    {
        [ProtoMember(1)] public uint userID;
        [ProtoMember(2)] public string userName;

        /// <summary>
        /// The ID in this game, usually is the player index.
        /// </summary>
        [ProtoMember(3)] public int playerID;
        [ProtoMember(4)] public int teamID;
        [ProtoMember(5)] public int aiID;
        [ProtoMember(6)] public int score;

        [ProtoMember(7)] public SnakerData snakerData;

        public override string ToString()
        {
            return string.Format("[PlayerData] userId={0}, userName={1}, playerID={2}, teamID={3}, aiID={4}, score={5}",
                                userID, userName, playerID, teamID, aiID, score);
        }
    }
}
