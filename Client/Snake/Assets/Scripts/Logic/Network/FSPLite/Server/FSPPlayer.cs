using System;
using System.Collections.Generic;
using GameProtocol;

namespace Framework.Network.FSP.Server
{
    public class FSPPlayer
    {
        private string LOG_TAG = "FSPPlayer";

        private uint _id;
        public uint Id 
        { 
            get{ return _id; } 
        }

        FSPSession m_Session;
        bool m_HasAuth = false;
        Action<FSPPlayer, FSPVKey> m_ReceiveListener;

        public uint Sid
        {
            get { return m_Session != null? m_Session.Id : 0; }
        }
        public bool HasAuth
        {
            get { return m_HasAuth; }
            set { m_HasAuth = value; }
        }

        private int m_LastRecvTime = 0;
        private int m_Timeout = 0;

        Queue<FSPFrame> m_FrameCache = new Queue<FSPFrame>();
        int m_LastAddFrameId = 0;

        public bool WaitForExit = false;  //标记


        public FSPPlayer(uint playerId, FSPSession session, int timeOut, Action<FSPPlayer, FSPVKey> listener)
        {
            _id = playerId;
            m_Session = session;
            m_Session.SetReceiveListener(OnReceive);
            m_Timeout = timeOut;

            m_ReceiveListener = listener;
            WaitForExit = false;

            LOG_TAG = LOG_TAG + "[" + playerId + "]";
        }

        public void Dispose()
        {
            m_Session = null;
            m_ReceiveListener = null;

            m_FrameCache.Clear();
        }


        void OnReceive(FSPData_CS data)
        {
            // 在这边接收主要是为了算超时
            m_LastRecvTime = FSPServer.Instance.RealtimeSinceStartupMS;

            if (m_Session.IsEndPointChanged)
            {
                m_HasAuth = false;
                Debuger.LogWarning(LOG_TAG, "The EndPoint is changed, cancel it's auth !");
            }

            if (m_ReceiveListener != null)
            {
                var vkeys = data.vkeys;
                for(int i = 0; i < vkeys.Count; i++)
                {
                    m_ReceiveListener(this, vkeys[i]);
                }
            }
        }

        public bool IsLost()
        {
            return m_Timeout > 0 && FSPServer.Instance.RealtimeSinceStartupMS - m_LastRecvTime >= m_Timeout;
        }

        public void ClearRound()
        {
            m_FrameCache.Clear();
            m_LastAddFrameId = 0;
        }


        public int SendToClient(FSPFrame frame)
        {
            if (frame != null)
            {
                if (!m_FrameCache.Contains(frame))
                {
                    m_FrameCache.Enqueue(frame);
                }
            }

            while (m_FrameCache.Count > 0)
            {
                if (TryAddToSession(m_FrameCache.Peek()))
                {
                    m_FrameCache.Dequeue();
                }
                else
                {
                    return -1;
                }
            }
            return 1;
        }
        bool TryAddToSession(FSPFrame frame)
        {
            //已经Add过了
            if (frame.frameId != 0 && frame.frameId <= m_LastAddFrameId)
            {
                return true;
            }

            if (m_Session != null)
            {
                if (m_Session.Send(frame))
                {
                    m_LastAddFrameId = frame.frameId;
                    return true;
                }
            }
            return false;
        }

    }
}