using System;
using System.Net;
using System.Collections.Generic;
using Framework.Network.Kcp;
using GameProtocol;

namespace Framework.Network.FSP.Client
{
    // FSPSession for client
    public class FSPClient
    {
        //===========================================================
        //日志
        public string LOG_TAG_SEND = "FSPClient_Send";
        public string LOG_TAG_RECV = "FSPClient_Recv";
        public string LOG_TAG_MAIN = "FSPClient_Main";
        public string LOG_TAG = "FSPClient";

        //----------------------------------------------------
        FSPParam m_Param;
        bool m_IsRunning = false;

        public bool IsRunning { get { return m_IsRunning; } }

        // 通信
        uint m_SessionId;
        KCPSocket m_Socket;
        IPEndPoint m_HostEndPoint = null;

        public string IP {get { return m_Socket != null? m_Socket.SelfIP : "";}}
        public int Port {get { return m_Socket != null? m_Socket.SelfPort : 0;}}

        // 接收
        byte[] m_TempRecvBuf = new byte[10240];
        Action<FSPFrame> m_RecvListener;

        // 发送
        int m_AuthId;
        FSPData_CS m_TempSendData = new FSPData_CS();
        byte[] m_TempSendBuf = new byte[128];

        // 重连
        bool m_WaitForReconnect = false;
        bool m_WaitForSendAuth = false;

        

        #region 生命周期
        public FSPClient(Action<FSPFrame> listener)
        {
            m_RecvListener = listener;
            if (m_RecvListener == null)
                Debuger.LogError(LOG_TAG, "The listener mustn't be null!");
        }

        public bool Start(FSPParam param)
        {
            if (m_IsRunning)
            {
                return true;
            }

            m_Param = param;
            SetSessionId(param.sid);
            SetFSPAuthInfo(param.authId);

            if ( Connect(param.host, param.port) )
            {
                // 必须要在发包之前设置为true
                m_IsRunning = true;

                VerifyAuth();

                return true;
            }
            return false;
        }

        public void Stop()
        {
            m_Param = null;

            Disconnect();
            m_WaitForReconnect = false;
            m_WaitForSendAuth = false;

            m_TempSendData.vkeys.Clear();
            m_RecvListener = null;
            
            m_IsRunning = false;
        }

        public void EnterFrame()
        {
            if (!m_IsRunning)
                return;
            
            m_Socket.Update();

            if ( m_WaitForReconnect )
            {
                Reconnect();
            }
            else if( m_WaitForSendAuth )
            {
                VerifyAuth();
            }
        }
        #endregion


        #region 设置通用参数
        void SetSessionId(uint sid)
        {
            LOG_TAG_MAIN = "FSPClient_Main<" + sid.ToString("d4") + ">";
            LOG_TAG_SEND = "FSPClient_Send<" + sid.ToString("d4") + ">";
            LOG_TAG_RECV = "FSPClient_Recv<" + sid.ToString("d4") + ">";
            LOG_TAG = LOG_TAG_MAIN;

            m_SessionId = sid;
            
            m_TempSendData.vkeys.Clear();
            m_TempSendData.vkeys.Add(new FSPVKey());
            m_TempSendData.sid = sid;
        }
        #endregion


        #region 设置FSP参数
        void SetFSPAuthInfo(int authId)
        {
            Debuger.Log(LOG_TAG_MAIN, "SetFSPAuthInfo() " + authId);
            m_AuthId = authId;
        }
        #endregion



        #region 收发
        void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            if (m_RecvListener != null)
            {
                FSPData_SC data = PBSerializer.Deserialize<FSPData_SC>(buffer);

                var frames = data.frames;
                for (int i = 0; i < frames.Count; i++)
                {
                    m_RecvListener(frames[i]);
                }
            }
            else
            {
                Debuger.LogWarning(LOG_TAG, "no receive listener!");
            }
        }

        public bool SendFSP(FSPVKeyBase vkey, int arg, int clientFrameId)
        {
            return SendFSP((int)vkey, arg, clientFrameId);
        }
        public bool SendFSP(int vkey, int arg, int clientFrameId)
        {
            if (m_IsRunning)
            {
                FSPVKey cmd = m_TempSendData.vkeys[0];
                cmd.vkey = vkey;
                cmd.args.Add(arg);
                cmd.clientFrameId = (uint)clientFrameId;
                
                int len = PBSerializer.Serialize(m_TempSendData, m_TempSendBuf);
                return m_Socket.SendTo(m_TempSendBuf, len, m_HostEndPoint);
            }
            return false;
        }

        public void SetWaitForReconnect()
        {
            m_WaitForReconnect = true;
        }

        public void SetWaitForAuth()
        {
            m_WaitForSendAuth = true;
        }
        #endregion


        #region 连接
        public void Reconnect()
        {
            Debuger.Log(LOG_TAG_MAIN, "Reconnect() 重新连接");
            m_WaitForReconnect = false;

            Disconnect();

            if( Connect(m_Param.host, m_Param.port) )
            {
                // 必须要在发包之前设置为true
                m_IsRunning = true;
                
                VerifyAuth();
            }
            else
            {
                m_WaitForReconnect = true;
                // 超出最大重连次数
            }
        }

        public bool Connect(string host, int port)
        {
            if (m_Socket != null)
            {
                Debuger.LogError(LOG_TAG_MAIN, "Connect() 无法建立连接，需要先关闭上一次连接！"); 
                return false;
            }

            Debuger.Log(LOG_TAG_MAIN, "Connect() 尝试建立基础连接， host = {0}, port = {1}", host, port);

            try
            {
                //获取Host对应的IPEndPoint
                Debuger.Log(LOG_TAG_MAIN, "Connect() 获取Host对应的IPEndPoint");
                m_HostEndPoint = IPUtils.GetHostEndPoint(host, port);
                if (m_HostEndPoint == null)
                {
                    Debuger.LogError(LOG_TAG_MAIN, "Connect() 无法将Host解析为IP！");
                    Close();
                    return false;
                }
                Debuger.Log(LOG_TAG_MAIN, "Connect() HostEndPoint = {0}", m_HostEndPoint.ToString());

                // 创建socket
                m_Socket = new KCPSocket(0, 1);
                m_Socket.Connect(m_HostEndPoint);
                m_Socket.AddReceiveListener(m_HostEndPoint, OnReceive);
            }
            catch(System.Exception e)
            {
                Debuger.LogError(LOG_TAG_MAIN, "Connect() " + e.Message + e.StackTrace);
                Close();
                return false;
            }

            return true;
        }

        void Disconnect()
        {
            Debuger.Log(LOG_TAG_MAIN, "Disconnect()");

            // 断开就不跑了
            m_IsRunning = false;

            if (m_Socket != null)
            {
                m_Socket.Dispose();
                m_Socket = null;
            }
            m_HostEndPoint = null;
        }

        void Close()
        {
            Stop();
        }

        void VerifyAuth()
        {
            m_WaitForSendAuth = false;
            SendFSP(FSPVKeyBase.AUTH, m_AuthId, 0);
        }
        #endregion


        public override string ToString()
        {
            return string.Format("FSPClinet: IsRunning={0}, HostEndPoint={1}, AuthId={2}, WaitForReconnect={3}, WaitForSendAuth={4}",
                                IsRunning, m_HostEndPoint, m_AuthId, m_WaitForReconnect, m_WaitForSendAuth);
        }
    }
}