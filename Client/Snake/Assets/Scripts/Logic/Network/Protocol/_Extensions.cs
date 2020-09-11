using EpochProtocol;
using LobbyProtocol;
using GameProtocol;
using System.Text;

namespace GameProtocol
{
    public partial class FSPVKey
    {
        public uint playerId
        {
            get { return playerIdOrClientFrameId; }
            set { playerIdOrClientFrameId = value; }
        }

        public uint clientFrameId
        {
            get { return playerIdOrClientFrameId; }
            set { playerIdOrClientFrameId = value; }
        }

        public override string ToString()
        {
            return string.Format("[vkey: {0}, arg: {1}, playerIdOrClientFrameId: {2}]", vkey, args[0], playerIdOrClientFrameId);
        }
    }

    public partial class FSPFrame
    {
        public bool IsEquals(FSPFrame obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.ToString() == this.ToString();
        }


        public bool IsEmpty()
        {
            if (vkeys == null || vkeys.Count == 0)
            {
                return true;
            }
            return false;
        }

        public bool ContainsVKey(int vkey)
        {
            if (!IsEmpty())
            {
                for (int i = 0; i < vkeys.Count; i++)
                {
                    if (vkeys[i].vkey == vkey)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (vkeys != null && vkeys.Count > 0)
            {
                for (int i = 0; i < vkeys.Count - 1; i++)
                {
                    sb.Append(vkeys[i].ToString());
                    sb.Append(",");
                }
                sb.Append(vkeys[vkeys.Count - 1].ToString());
            }
            return "{frameId: " + frameId.ToString() + ", vkeys: [" + sb.ToString() + "]}";
        }
    }
}
