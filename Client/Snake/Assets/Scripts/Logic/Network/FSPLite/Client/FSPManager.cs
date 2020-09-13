using System;
using System.Collections.Generic;
using GameProtocol;

namespace Framework.Network.FSP.Client
{
    // FSPGame for client
    public class FSPManager
    {
        public string LOG_TAG = "FSPManager";

        FSPParam m_Param;

        uint _mainPlayerId = 0;
        FSPClient m_Client;
        FSPController m_Controller;
        Dictionary<int, FSPFrame> m_FrameBuffer;
        bool m_IsRunning = false;

        public bool IsRunning { get {return m_IsRunning;} }
        public string IP {get { return m_Client != null? m_Client.IP : "";}}
        public int Port {get { return m_Client != null? m_Client.Port : 0;}}

        // 状态
        FSPGameState m_GameState = FSPGameState.None;
        public FSPGameState GameState { get {return m_GameState;} }

        // 帧相关
        private int m_ClientLockedFrame = -1;  // 当前时刻最多能到达的帧，其实是服务器下推的最大帧，客户端真正逻辑不能比服务端的逻辑快
        private int m_CurrentFrameIndex = 0;  // 当前时刻实际到达的帧
        private Action<int, FSPFrame> m_FrameListener;  //外部接口
        // 本地跑
        FSPFrame m_NextLocalFrame;

        // 事件
        private IGameStateListener m_GameStateListener;


        #region 生命周期
        // 游戏正式开始
        public void Start(FSPParam param, uint playerId)
        {
            m_Param = param;
            _mainPlayerId = playerId;
            LOG_TAG = "FSPManager[" + playerId + "]";

            // 开房间形式
            if ( !param.useLocal )
            {
                m_Client = new FSPClient(OnFSPListener);
                m_Client.Start(param);
            }

            m_Controller = new FSPController();
            m_Controller.Start(param);

            m_GameState = FSPGameState.Create;
            ResetRound();

            m_IsRunning = true;
        }

        public void Stop()
        {
            m_GameState = FSPGameState.None;

            if (m_Controller != null)
            {
                m_Controller.Stop();
                m_Controller = null;
            }

            if (m_Client != null)
            {
                m_Client.Stop();
                m_Client = null;
            }

            m_FrameBuffer.Clear();
            m_CurrentFrameIndex = 0;
            m_FrameListener = null;

            m_NextLocalFrame = null;

            _mainPlayerId = 0;
            m_Param = null;
            m_GameStateListener = null;

            m_IsRunning = false;
        }

        /// <summary>
        /// 由外界驱动
        /// </summary>
        public void EnterFrame()
        {
            if (!m_IsRunning)
            {
                return;
            }
        
            // 本地跑
            if (m_Param.useLocal)
            {
                // 只要游戏没结束就可以一直跑，无限模式不能用吧，不然可能int溢出
                if (m_ClientLockedFrame <= 0 || m_CurrentFrameIndex < m_ClientLockedFrame)
                {
                    m_CurrentFrameIndex ++;

                    FSPFrame frame;
                    m_FrameBuffer.TryGetValue(m_CurrentFrameIndex, out frame);
                    ExecuteFrame(m_CurrentFrameIndex, frame);
                }
            }
            else
            {
                // 驱动FSPClient
                m_Client.EnterFrame();

                // 在网络正常的情况下，speed一般是默认的1，表示不加速；如果网络较差，可能没有帧可以消耗了，会变成0，游戏卡住；
                // 然后网络恢复，一下子来了大量的帧，需要一下消耗多个帧。
                int speed = m_Controller.GetFrameSpeed(m_CurrentFrameIndex);
                while (speed > 0)
                {
                    if (m_CurrentFrameIndex < m_ClientLockedFrame)
                    {
                        m_CurrentFrameIndex ++;

                        FSPFrame frame;
                        m_FrameBuffer.TryGetValue(m_CurrentFrameIndex, out frame);
                        ExecuteFrame(m_CurrentFrameIndex, frame);
                    }

                    speed--;
                }

                // TODO: 这里可能加预表现，服务器没下推数据，客户端自己先预测，等实际下推了再对比，差异较大的要回滚

            }
        }
        
        public void SetGameStateListener(IGameStateListener listener)
        {
            m_GameStateListener = listener;
        }
        #endregion


        #region 数据的收发
        /// <summary>
        /// 设置帧数据的监听
        /// </summary>
        /// <param name="listener"></param>
        public void SetFrameListener(Action<int, FSPFrame> listener)
        {
            m_FrameListener = listener;
        }

        /// <summary>
        /// 监听来自FSPClient的帧数据
        /// </summary>
        /// <param name="frame"></param>
        private void OnFSPListener(FSPFrame frame)
        {
            AddServerFrame(frame);
        }


        void AddServerFrame(FSPFrame frame)
        {
            // 一些状态事件，没有frameId的
            if (frame.frameId <= 0)
            {
                ExecuteFrame(frame.frameId, frame);
                return;
            }

            // 客户端的帧率比服务端的要大，这边需要转换一下，等于说
            // 客户端的帧序列其实是：0 1 0 1 0 1 0 1...
            // 1是服务器下发的实际帧，0是客户端自己插入的空帧，具体有几个0由clientFrameRateMultiple决定
            // 服务器逻辑帧一般是15帧，即帧间隔是66.666667ms，因为客户端设置一般是30帧或60帧，刚好是倍数关系，方便计算。
            frame.frameId = frame.frameId * m_Param.clientFrameRateMultiple;

            // 客户端逻辑能到的最大帧，除了服务器最大帧，还可以加上客户端自己插入的空白帧。
            m_ClientLockedFrame = frame.frameId + (m_Param.clientFrameRateMultiple - 1);

            // 记录最新的帧数据
            m_FrameBuffer[frame.frameId] = frame;
            m_Controller.AddFrameId(frame.frameId); 
        }


        /// <summary>
        /// 发送输入
        /// </summary>
        public bool SendFSP(FSPVKeyBase vkey, int arg = 0)
        {
            return SendFSP((int)vkey, arg);
        }
        public bool SendFSP(int vkey, int arg = 0)
        {
            if (!m_IsRunning)
            {
                return false;
            }

            // 发给本地
            if (m_Param.useLocal)
            {
                return SendFSPLocal(vkey, arg);
            }
            else
            {
                return m_Client.SendFSP(vkey, arg, m_CurrentFrameIndex);
            }
        }
        
        /// <summary>
        /// 用于本地兼容，比如打PVE的时候，也可以用帧同步兼容
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        bool SendFSPLocal(int vkey, int arg)
        {
            Debuger.Log(LOG_TAG, "SendFSPLocal() vkey={0}, arg={1}", vkey, arg);

            // 新的帧
            int nextFrameId = m_CurrentFrameIndex + 1;
            if (m_NextLocalFrame == null || m_NextLocalFrame.frameId != nextFrameId)
            {
                m_NextLocalFrame = new FSPFrame();
                m_NextLocalFrame.frameId = nextFrameId;
                m_FrameBuffer[m_NextLocalFrame.frameId] = m_NextLocalFrame;
            }

            // 添加输入
            FSPVKey cmd = new FSPVKey();
            cmd.playerId = _mainPlayerId;
            cmd.vkey = vkey;
            cmd.args.Add(arg);
            
            m_NextLocalFrame.vkeys.Add(cmd);

            return true;
        }
        #endregion


        #region 游戏状态逻辑
        /// <summary>
        /// 执行每一帧
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="frame"></param>
        void ExecuteFrame(int frameId, FSPFrame frame)
        {
            //优先处理流程VKey
            if (frame != null && frame.vkeys != null)
            {
                for(int i = 0; i < frame.vkeys.Count; i++)
                {
                    FSPVKey cmd = frame.vkeys[i];

                    if (cmd.vkey == (int)FSPVKeyBase.GAME_BEGIN)
                    {
                        Handle_GameBegin(cmd);
                    }
                    else if (cmd.vkey == (int)FSPVKeyBase.ROUND_BEGIN) 
					{
						Handle_RoundBegin(cmd);
					}
                    else if (cmd.vkey == (int)FSPVKeyBase.CONTROL_START)
                    {
                        Handle_ControlStart(cmd);
                    }
                    else if (cmd.vkey == (int)FSPVKeyBase.ROUND_END) 
					{
						Handle_RoundEnd(cmd);
					} 
					else if (cmd.vkey == (int)FSPVKeyBase.GAME_END)
					{
						Handle_GameEnd(cmd);
					}
                    else if (cmd.vkey == (int)FSPVKeyBase.PLAYER_EXIT)
                    {
                        Handle_PlayerExit(cmd);
                    }
                }
            }

            //再将其它VKey抛给外界处理
            if (m_FrameListener != null)
            {
                m_FrameListener(frameId, frame);
            }
        }


        public void SendGameBegin()
        {
            SendFSP(FSPVKeyBase.GAME_BEGIN);
        }
        void Handle_GameBegin(FSPVKey cmd)
        {
            m_GameState = FSPGameState.GameBegin;
            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnGameBegin(cmd);
            }
        }

        public void SendRoundBegin()
        {
            SendFSP(FSPVKeyBase.ROUND_BEGIN);
        }
        void Handle_RoundBegin(FSPVKey cmd)
        {
            m_GameState = FSPGameState.RoundBegin;

            // 这里需要重置一些数据
            ResetRound();

            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnRoundBegin(cmd);
            }
        }

        public void SendControlStart()
        {
            SendFSP(FSPVKeyBase.CONTROL_START);
        }
        void Handle_ControlStart(FSPVKey cmd)
        {
            m_GameState = FSPGameState.ControlStart;
            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnControlStart(cmd);
            }
        }
        
        public void SendRoundEnd()
        {
            SendFSP(FSPVKeyBase.ROUND_END);
        }
        void Handle_RoundEnd(FSPVKey cmd)
        {
            m_GameState = FSPGameState.RoundEnd;
            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnRoundEnd(cmd);
            }
        }

        public void SendGameEnd()
        {
            SendFSP(FSPVKeyBase.GAME_END);
        }
        void Handle_GameEnd(FSPVKey cmd)
        {
            m_GameState = FSPGameState.GameEnd;
            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnGameEnd(cmd);
            }
        }

        // 主动退出
        public void SendPlayerExit()
        {
            SendFSP(FSPVKeyBase.PLAYER_EXIT);
        }
         void Handle_PlayerExit(FSPVKey cmd)
        {
            if (m_GameStateListener != null)
            {
                m_GameStateListener.OnPlayerExit(cmd);
            }
        }
        #endregion


        #region Round逻辑
        void ResetRound()
        {
            m_CurrentFrameIndex = 0;

            if (m_Param.useLocal)
            {
                m_ClientLockedFrame = m_Param.maxFrameId;
            }
            else
            {
                m_ClientLockedFrame = m_Param.clientFrameRateMultiple - 1;
            }

            if (m_FrameBuffer == null)
                m_FrameBuffer = new Dictionary<int, FSPFrame>();
            else
                m_FrameBuffer.Clear();
        }
        #endregion

        // debug
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("FSPManager: ");

            if (m_Controller != null)
            {
                sb.Append("PlayedFrameId: ");
                sb.Append(m_CurrentFrameIndex);
                sb.Append("; ");

                sb.Append("LatestFrameId: ");
                sb.Append(m_Controller.LatestFrameId);
                sb.Append("; ");

                sb.Append("IsInSpeedUp: ");
                sb.Append(m_Controller.IsInSpeedUp);
                sb.Append("; ");

                sb.Append("IsInBuffing: ");
                sb.Append(m_Controller.IsInBuffing);
                sb.Append("; ");

                sb.Append("FrameBufferSize: ");
                sb.Append(m_Controller.FrameBufferSize);
                sb.Append("; ");
            }

            if (m_Client != null)
            {
                sb.Append("\n");
                sb.Append(m_Client.ToString());
            }

            return sb.ToString();
        }
    }
}