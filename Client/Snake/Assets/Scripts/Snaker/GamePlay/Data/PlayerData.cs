using System.Collections;
using System.Collections.Generic;

namespace GamePlay
{
    public class PlayerData 
    {
        public uint userID;
        public string userName;

        /// <summary>
        /// The ID in this game, usually is the player index.
        /// </summary>
        public int playerID;
        public int teamID;
        public int aiID;
        public int score;

        public SnakerData snakerData;
    }
}
