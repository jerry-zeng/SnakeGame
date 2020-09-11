using System.Collections;
using System.Collections.Generic;
using GameProtocol;
using ProtoBuf;

namespace GamePlay
{
    [ProtoContract]
    public class GameParam 
    {
        [ProtoMember(1)] public uint gameID = 1;
        [ProtoMember(2)] public int randSeed = 0;
        [ProtoMember(3)] public GameMode mode = GameMode.EndlessPVE;
        [ProtoMember(4)] public int mapID;
        [ProtoMember(5)] public float limitTime; //Second
        [ProtoMember(6)] public int limitPlayer;
    }
}
