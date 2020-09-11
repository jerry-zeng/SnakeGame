using System;
using System.Net;
using Framework.Network.Kcp;
using GameProtocol;

namespace Framework.Network.FSP.Server
{
    // 保存对玩家的连接信息，代理发送数据包
    public class FSPSession
    {
        private string LOG_TAG = "FSPSession";

        public uint m_Sid;
        public uint Id { get{ return m_Sid; } }

        private KCPSocket m_Socket;
        private byte[] m_SendBuffer =  new byte[40960];
        private Action<FSPData_CS> m_RecvListener;


        private bool m_IsEndPointChanged = false;
        public bool IsEndPointChanged { get { return m_IsEndPointChanged; } }

        private IPEndPoint m_EndPoint;
        public IPEndPoint EndPoint
        {
            get { return m_EndPoint; }
            set
            {
                if (m_EndPoint == null || !m_EndPoint.Equals(value))
                {
                    m_IsEndPointChanged = true;
                }
                else
                {
                    m_IsEndPointChanged = false;
                }

                m_EndPoint = value;
            }
        }


        public FSPSession(uint sid,  KCPSocket socket)
        {
            m_Sid = sid;
            m_Socket = socket;
            LOG_TAG = "FSPSession<" + m_Sid.ToString("d4") + ">";
        }

        public virtual void Close()
        {
            if (m_Socket != null)
            {
                m_Socket.CloseKcp(EndPoint);
                m_Socket = null;
            }
        }

        //------------------------------------------------------------

        public void SetReceiveListener(Action<FSPData_CS> listener)
        {
            m_RecvListener = listener;
        }

        public void Receive(FSPData_CS data)
        {
            if (m_RecvListener != null)
                m_RecvListener(data);
        }

        public bool Send(FSPFrame frame)
        {
            if (m_Socket != null)
            {
                FSPData_SC data = new FSPData_SC();
                data.frames.Add(frame);

                int len = PBSerializer.Serialize(data, m_SendBuffer);
                return m_Socket.SendTo(m_SendBuffer, len, m_EndPoint);
            }
            return false;
        }
    }
}