using System.Collections;
using System.Collections.Generic;
using GameProtocol;
using ProtoBuf;

namespace GamePlay
{
    [ProtoContract]
    public class GameParam 
    {
        [ProtoMember(1)] public uint gameID;
        [ProtoMember(2)] public int randSeed;
        [ProtoMember(3)] public GameMode mode;
        [ProtoMember(4)] public int mapID;
        [ProtoMember(5)] public float limitTime;
        [ProtoMember(6)] public int limitPlayer;
    }
}
