using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace GamePlay
{
    [ProtoContract]
    public class SnakerData 
    {
        [ProtoMember(1)] public int id;
        [ProtoMember(2)] public int length;
    }
}
