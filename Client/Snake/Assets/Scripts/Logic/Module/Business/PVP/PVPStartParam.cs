using System.Collections.Generic;
using GameProtocol;
using ProtoBuf;

namespace GamePlay
{
    [ProtoContract]
	public class PVPStartParam
	{
		[ProtoMember(1)] public FSPParam fspParam = new FSPParam();
		[ProtoMember(2)] public GameParam gameParam = new GameParam();
        [ProtoMember(3)] public List<PlayerData> players = new List<PlayerData>();
	}
}