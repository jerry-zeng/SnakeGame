using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Framework.Network.Kcp;
using Framework.Network.RPC;
using GameProtocol;

namespace Framework.Network.FSP.Server
{
    public class FSPServer : Singleton<FSPServer>
    {
        //===========================基本设置================================
        // 启动参数
        private FSPParam m_Param = new FSPParam();
        // 帧间隔
        private long FRAME_TICK_INTERVAL = 666666;  // 除以
        //
        private bool m_UseExternFrameTick = false;
        //===========================================================
        //日志TAG'
        private string LOG_TAG_MAIN = "FSPServer_Main";
        private string LOG_TAG_SEND = "FSPServer_Send";
        private string LOG_TAG_RECV = "FSPServer_Recv";
        

        // 房间，在游戏之前就创建了
        private FSPRoom m_Room;
        public FSPRoom Room { get { return m_Room; } }


        // 游戏逻辑
        private FSPGame m_Game;
        public FSPGame Game { get { return m_Game; } }

        // 与玩家的连接信息
        private List<FSPSession> m_ListSession = new List<FSPSession>();

        // 与所有客户端的通讯
        private KCPSocket m_GameSocket;
        // 线程模块
        private Thread m_ThreadMain;

        private bool m_IsRunning = false;
        public bool IsRunning { get { return m_IsRunning; } }

        //=======================逻辑===================================
        private long m_RealTicksAtStart = 0L;  //启动时间
        private long m_LogicLastTicks = 0L;

        public int RealtimeSinceStartupMS
        {
            get
            {
                long dt = DateTime.Now.Ticks - m_RealTicksAtStart;
                return (int)(dt/10000);
            }
        }


        #region 参数设置
        // ms
        public void SetFrameInterval(int serverFrameInterval, int clientFrameRateMultiple)
        {
            FRAME_TICK_INTERVAL = serverFrameInterval * 333333*30/1000;
            FRAME_TICK_INTERVAL = serverFrameInterval * 10000;

            m_Param.serverFrameInterval = serverFrameInterval;
            m_Param.clientFrameRateMultiple = clientFrameRateMultiple;
        }

        public void SetServerTimeout(int serverTimeout)
        {
            m_Param.serverTimeout = serverTimeout;
        }

        // 外部Tick
        public bool UseExternFrameTick
        {
            get { return m_UseExternFrameTick; }
            set { m_UseExternFrameTick = value; }
        }

        public FSPParam GetParam()
        {
            m_Param.host = GameIP;
            m_Param.port = GamePort;
            return CloneParam(m_Param);
        }

        static FSPParam CloneParam(FSPParam param)
        {
            byte[] bytes = PBSerializer.Serialize(param);
            return PBSerializer.Deserialize<FSPParam>(bytes);
        }

        #endregion


        #region 通讯参数
        public string GameIP
        {
            get { return m_GameSocket != null? m_GameSocket.SelfIP : ""; }
        }
        public int GamePort
        {
            get { return m_GameSocket != null? m_GameSocket.SelfPort : 0; }
        }

        public string RoomIP
        {
            get { return m_Room != null? m_Room.SelfIP : ""; }
        }
        public int RoomPort
        {
            get { return m_Room != null? m_Room.SelfPort : 0; }
        }
        #endregion


        #region Session
        internal FSPSession GetSession(uint sid)
        {
            lock (m_ListSession)
            {
                for(int i = 0; i < m_ListSession.Count; i++)
                {
                    if (m_ListSession[i].Id == sid)
                        return m_ListSession[i];
                }
            }
            return null;
        }

        internal FSPSession AddSession(uint sid)
        {
            FSPSession session = GetSession(sid);
            if (session == null)
            {
                session = new FSPSession(sid, m_GameSocket);

                lock (m_ListSession)
                {
                    m_ListSession.Add(session);
                }
                Debuger.Log(LOG_TAG_MAIN, "AddSession() SID = " + sid);
            }
            return session;
        }

        internal void DelSession(uint sid)
        {
            lock (m_ListSession)
            {
                for(int i = 0; i < m_ListSession.Count; i++)
                {
                    if (m_ListSession[i].Id == sid)
                    {
                        m_ListSession[i].Close();

                        m_ListSession.RemoveAt(i);
                        Debuger.Log(LOG_TAG_MAIN, "DelSession() sid = {0}", sid);
                        return;
                    }
                }
            }
        }

        void DelAllSession()
        {
            Debuger.Log(LOG_TAG_MAIN, "DelAllSession()");
            lock (m_ListSession)
            {
                for (int i = 0; i < m_ListSession.Count; i++)
                {
                    m_ListSession[i].Close();
                }
                m_ListSession.Clear();
            }
        }
        #endregion


        #region 服务本身
        public bool Start(int port)
        {
            if (m_IsRunning)
            {
                Debuger.LogWarning(LOG_TAG_MAIN, "Start() 不能重复创建启动Server！");
                return false;
            }
            Debuger.Log(LOG_TAG_MAIN, "Start()  port = {0}", port);

            DelAllSession();

            try
            {
                // 初始化参数
                m_RealTicksAtStart = DateTime.Now.Ticks;
                m_LogicLastTicks = m_RealTicksAtStart;  //也从这个时候开始算

                //创建Game Socket
                m_GameSocket = new KCPSocket(0, 1);
                m_GameSocket.AddReceiveListener(OnReceive);

                m_IsRunning = true;

                // 创建房间先
                m_Room = new FSPRoom();
                m_Room.Start();

                // 创建线程
                Debuger.Log(LOG_TAG_MAIN, "Start()  创建服务器线程");
                m_ThreadMain = new Thread(Thread_Main) { IsBackground = true };
                m_ThreadMain.Start();
            }
            catch(System.Exception e)
            {
                Debuger.LogError(LOG_TAG_MAIN, "Start failed: " + e.Message);
                Close();
                return false;
            }

            //当用户直接用UnityEditor上的停止按钮退出游戏时，会来不及走完整的析构流程。
            //这里做一个监听保护
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlayModeChanged;
            #endif
            return true;
        }

        public void Close()
        {
            Debuger.Log(LOG_TAG_MAIN, "Close()");

            m_IsRunning = false;

            if (m_Game != null)
            {
                m_Game.Stop();
                m_Game = null;
            }

            if (m_Room != null)
            {
                m_Room.Destroy();
                m_Room = null;
            }

            DelAllSession();

            if (m_GameSocket != null)
            {
                m_GameSocket.Dispose();
                m_GameSocket = null;
            }

            if (m_ThreadMain != null)
            {
                // m_ThreadMain.Interrupt();
                m_ThreadMain.Abort();
                m_ThreadMain = null;
            }
        }

        #if UNITY_EDITOR
        private void OnEditorPlayModeChanged()
        {
            if (UnityEngine.Application.isPlaying == false)
            {
                UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
                Close();
            }
        }
        #endif
        #endregion


        #region Game Logic
        public FSPGame StartGame(FSPParam param)
        {
            if (m_Game != null)
                m_Game.Stop();
            
            m_Game = new FSPGame();
            m_Game.Start(m_Param);
            return m_Game;
        }

        public void StopGame()
        {
            if (m_Game != null)
            {
                m_Game.Stop();
                m_Game = null;
            }
        }
        #endregion


        #region 网络相关
        void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            FSPData_CS data = PBSerializer.Deserialize<FSPData_CS>(buffer);

            FSPSession session = GetSession(data.sid);
            if (session == null)
            {
                Debuger.LogWarning(LOG_TAG_RECV, "DoReceive() 收到一个未知的SID = " + data.sid);
                return;
            }

            session.EndPoint = remotePoint;
            session.Receive(data);
        }

        // 发送通过Session
        #endregion


        #region Tick
        void Thread_Main()
        {
            Debuger.Log(LOG_TAG_MAIN, "Thread_Main() Begin ......");

            while (m_IsRunning)
            {
                try
                {
                    DoMainLoop();
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG_MAIN, "Thread_Main() " + e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(10);
                }
            }

            Debuger.Log(LOG_TAG_MAIN, "Thread_Main() End!");
        }
        void DoMainLoop()
        {
            long nowticks = DateTime.Now.Ticks;
            if (nowticks - m_LogicLastTicks > FRAME_TICK_INTERVAL)
            {
                m_LogicLastTicks = nowticks - (nowticks % FRAME_TICK_INTERVAL);

                if (!m_UseExternFrameTick)
                {
                    EnterFrame();
                }
            }
        }

        // 方便给外部Tick?
        public void EnterFrame()
        {
            if (m_IsRunning)
            {
                m_GameSocket.Update();
                m_Room.RPCTick();

				if (m_Game != null) 
				{
					m_Game.EnterFrame();
				}
            }
        }
        #endregion

    }
}