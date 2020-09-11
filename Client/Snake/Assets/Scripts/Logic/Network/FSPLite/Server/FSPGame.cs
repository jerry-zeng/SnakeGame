using System;
using System.Collections.Generic;
using GameProtocol;

namespace Framework.Network.FSP.Server
{
    public class FSPGame
    {
        public string LOG_TAG = "FSPGame";

        /// <summary>
        /// 最大支持的玩家数：31
        /// 因为用来保存玩家Flag的Int只有31位有效位可用，不过31已经足够了
        /// </summary>
        public const int MaxPlayerNum = 31;
        
        //---------------------------------------------------------
        //基本数据
        FSPParam m_FSPParam;

        // 状态
        FSPGameState m_State = FSPGameState.None;
        int m_StateParam1;
        int m_StateParam2;

        public FSPGameState State { get{ return m_State; } }
        public int StateParam1 { get { return m_StateParam1; } }
        public int StateParam2 { get { return m_StateParam2; } }

        // VKey输入标识，对应FSPGameState
        private int m_GameBeginFlag = 0;
        private int m_RoundBeginFlag = 0;
        private int m_ControlStartFlag = 0;
        private int m_RoundEndFlag = 0;
        private int m_GameEndFlag = 0;

        // 回合
        private int m_CurRoundId = 0;
        public int CurrentRoundId { get { return m_CurRoundId; } }

        // 玩家列表
        List<FSPPlayer> m_ListPlayer = new List<FSPPlayer>();
        //等待删除的玩家
        List<FSPPlayer> m_ListPlayersExitOnNextFrame = new List<FSPPlayer>();

        // 当前帧
        private int m_CurFrameId = 0;
        public int CurrentFrameId { get { return m_CurFrameId; } }

        FSPFrame m_LockedFrame = new FSPFrame();
        // 所有帧
        List<FSPFrame> m_FrameRecords = new List<FSPFrame>();


        //有一个玩家退出游戏
		public event Action<uint> onGameExit;
		//游戏真正结束
		public event Action<int> onGameEnd;


        #region 生命周期
        public void Start(FSPParam param)
        {
            m_FSPParam = param;
            SetGameState(FSPGameState.Create);
            m_CurRoundId = 0;

            ClearRound();

            Debuger.Log(LOG_TAG, "Start()");
        }

        public void Stop()
        {
            SetGameState(FSPGameState.None);
            
            FSPServer server = FSPServer.Instance;
            foreach(FSPPlayer player in m_ListPlayer)
            {
                server.DelSession(player.Sid);
                player.Dispose();
            }
            m_ListPlayer.Clear();

            m_ListPlayersExitOnNextFrame.Clear();
            m_FrameRecords.Clear();

            onGameExit = null;
            onGameEnd = null;

            GC.Collect();
            Debuger.Log(LOG_TAG, "Stop()");
        }
        #endregion

        
        #region Player
        public bool AddPlayer(uint playerId, uint sid)
        {
            Debuger.Log(LOG_TAG, "AddPlayer() playerId: {0}, sid: {1}", playerId, sid);

            if (m_State != FSPGameState.Create)
            {
                Debuger.LogError(LOG_TAG, "AddPlayer() 当前状态下无法AddPlayer! State = {0}", m_State);
                return false;
            }

            // 移除老的
            RemovePlayer(playerId);

            if (m_ListPlayer.Count >= MaxPlayerNum)
            {
                Debuger.LogError(LOG_TAG, "AddPlayer() 玩家数达到上限 {0}", MaxPlayerNum);
                return false;
            }

            FSPSession session = FSPServer.Instance.AddSession(sid);
            FSPPlayer player = new FSPPlayer(playerId, session, m_FSPParam.serverTimeout, OnPlayerReceive);
            m_ListPlayer.Add(player);

            return true;
        }

        void RemovePlayer(uint playerId)
        {
            for(int i = 0; i < m_ListPlayer.Count; i++)
            {
                FSPPlayer player = m_ListPlayer[i];
                if (player.Id == playerId)
                {
                    m_ListPlayer.RemoveAt(i);

                    FSPServer.Instance.DelSession(player.Sid);
                    player.Dispose();
                    return;
                }
            }
        }

        public FSPPlayer GetPlayer(uint playerId)
        {
            for(int i = 0; i < m_ListPlayer.Count; i++)
            {
                FSPPlayer player = m_ListPlayer[i];
                if (player.Id == playerId)
                {
                    return player;
                }
            }
            return null;
        }

        internal int GetPlayerCount()
        {
            return m_ListPlayer.Count;
        }

        public List<FSPPlayer> GetPlayerList()
        {
            return m_ListPlayer;
        }
        #endregion


        #region 输入
        void OnPlayerReceive(FSPPlayer player, FSPVKey cmd)
        {
            HandleClientCmd(player, cmd);
        }

        /// <summary>
        /// 处理来自客户端的 Cmd
        /// 对其中的关键VKey进行处理
        /// 并且收集业务VKey
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cmd"></param>
        protected virtual void HandleClientCmd(FSPPlayer player, FSPVKey cmd)
        {
            uint playerId = player.Id;

            if (!player.HasAuth)
            {
                Debuger.Log(LOG_TAG, "HandleClientCmd() hasAuth = false! Wait AUTH!");
                if (cmd.vkey == (int)FSPVKeyBase.AUTH)
                {
                    player.HasAuth = true;
                    // player.HasAuth = cmd.args[0] > 0;
                    Debuger.Log(LOG_TAG, "HandleClientCmd() AUTH, playerId={0}", playerId);
                }
                return;
            }

            switch(cmd.vkey)
            {
                case (int)FSPVKeyBase.GAME_BEGIN:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() GAME_BEGIN, playerId = {0}, cmd = {1}", playerId, cmd);
                    SetFlag(playerId, ref m_GameBeginFlag, "m_GameBeginFlag");
                break;
                case (int)FSPVKeyBase.ROUND_BEGIN:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() ROUND_BEGIN, playerId = {0}, cmd = {1}", playerId, cmd);
                    SetFlag(playerId, ref m_RoundBeginFlag, "m_RoundBeginFlag");
                break;
                case (int)FSPVKeyBase.CONTROL_START:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() CONTROL_START, playerId = {0}, cmd = {1}", playerId, cmd);
                    SetFlag(playerId, ref m_ControlStartFlag, "m_ControlStartFlag");
                break;
                case (int)FSPVKeyBase.ROUND_END:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() ROUND_END, playerId = {0}, cmd = {1}", playerId, cmd);
                    SetFlag(playerId, ref m_RoundEndFlag, "m_RoundEndFlag");
                break;
                case (int)FSPVKeyBase.GAME_END:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() GAME_END, playerId = {0}, cmd = {1}", playerId, cmd);
                    SetFlag(playerId, ref m_GameEndFlag, "m_GameEndFlag");
                break;

                case (int)FSPVKeyBase.GAME_EXIT:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() GAME_EXIT, playerId = {0}, cmd = {1}", playerId, cmd);
                    HandleGameExit(playerId, cmd);
                break;

                default:
                    Debuger.Log(LOG_TAG, "HandleClientCmd() other, playerId = {0}, cmd = {1}", playerId, cmd);
                    AddCmdToCurrentFrame(playerId, cmd);
                break;
            }
        }

        void HandleGameExit(uint playerId, FSPVKey cmd)
        {
            AddCmdToCurrentFrame(playerId, cmd);

            // 退出
            FSPPlayer player = GetPlayer(playerId);
            if (player != null)
            {
                player.WaitForExit = true;

                if (onGameExit != null)
                    onGameExit(player.Id);
            }
        }

        // 收到客户端的数据
        protected void AddCmdToCurrentFrame(uint playerId, FSPVKey cmd)
        {
            cmd.playerId = playerId;
            m_LockedFrame.vkeys.Add(cmd);
        }


        // 服务端下推
        protected void AddCmdToCurrentFrame(FSPVKeyBase vkey, int arg = 0)
        {
            FSPVKey cmd = new FSPVKey();
            cmd.vkey = (int)vkey;
            cmd.args.Add(arg);
            AddCmdToCurrentFrame(0, cmd);
        }
        #endregion


        #region Player状态标志
        void SetFlag(uint playerId, ref int flag, string flagName)
        {
            flag |= (0x01 << ((int)playerId - 1));
            Debuger.Log(LOG_TAG, "SetFlag() player = {0}, flag = {1}", playerId, flagName);
        }
        void ClearFlag(int playerId, ref int flag, string flagName)
        {
            flag &= (~(0x01 << (playerId - 1)));
            Debuger.Log(LOG_TAG, "ClearFlag() player = {0}, flag = {1}", playerId, flagName);
        }

        public bool IsFlagFull(int flag)
        {
			if (m_ListPlayer.Count > 1)
			{
				for (int i = 0; i < m_ListPlayer.Count; i++)
				{
					FSPPlayer player = m_ListPlayer[i];
					int playerId = (int)player.Id;
					if ((flag & (0x01 << (playerId - 1))) == 0)
					{
						return false;
					}
				}
				return true;
			}
			return false;
        }
        #endregion


        #region 主循环
        public void EnterFrame()
        {
            // 延迟移除，因为还要给些时间发包
            for (int i = 0; i < m_ListPlayersExitOnNextFrame.Count; i++)
            {
                FSPPlayer player = m_ListPlayersExitOnNextFrame[i];
                FSPServer.Instance.DelSession(player.Sid);
                player.Dispose();
            }
            m_ListPlayersExitOnNextFrame.Clear();

            // 处理状态变化
            HandleGameState();

            //经过上面状态处理之后，有可能状态还会发生变化
            if (m_State == FSPGameState.None)
            {
                return;
            }

            if ( m_LockedFrame.frameId != 0 //已经在跑逻辑了
                || !m_LockedFrame.IsEmpty()  //可能还在准备阶段
            ){
                for(int i = m_ListPlayer.Count-1; i >= 0; i--)
                {
                    FSPPlayer player = m_ListPlayer[i];
                    player.SendToClient(m_LockedFrame);

                    if (player.WaitForExit)
                    {
                        m_ListPlayer.RemoveAt(i);
                        m_ListPlayersExitOnNextFrame.Add(player);
                    }
                }
            }
            //0帧每个循环需要额外清除掉再重新统计
            if (m_LockedFrame.frameId == 0)
            {
                // 为了保留m_LockedFrame.vkeys，没有用Clear()来清除
                m_LockedFrame = new FSPFrame();
                m_FrameRecords.Add(m_LockedFrame);
            }

            //在这个阶段，帧号才会不停往上加
            if (m_State == FSPGameState.RoundBegin || m_State == FSPGameState.ControlStart)
            {
                m_CurFrameId++;

                m_LockedFrame = new FSPFrame();
                m_LockedFrame.frameId = m_CurFrameId;

                m_FrameRecords.Add(m_LockedFrame);
            }
        }
        #endregion


        #region 状态
        protected void SetGameState(FSPGameState state, int param1 = 0, int param2 = 0)
        {
            Debuger.Log(LOG_TAG, "SetGameState() >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Debuger.Log(LOG_TAG, "SetGameState() {0} -> {1}, param1 = {2}, param2 = {3}", m_State, state, param1, param2);
            Debuger.Log(LOG_TAG, "SetGameState() <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

            m_State = state;
            m_StateParam1 = param1;
            m_StateParam2 = param2;
        }

        public bool IsGameEnd()
        {
            return m_State == FSPGameState.GameEnd;
        }

        /// <summary>
        /// 检测游戏是否异常结束
        /// </summary>
        bool CheckGameAbnormalEnd()
        {
            //判断还剩下多少玩家，如果玩家少于2，则表示至少有玩家主动退出
            if (m_ListPlayer.Count < 2)
            {
                //直接进入GameEnd状态
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.AllOtherExit);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndReason.AllOtherExit);
                return true;
            }

            // 检测玩家在线状态
            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                FSPPlayer player = m_ListPlayer[i];
                if (player.IsLost())
                {
                    m_ListPlayer.RemoveAt(i);
                    FSPServer.Instance.DelSession(player.Sid);
                    player.Dispose();
                    --i;
                }
            }
            //判断还剩下多少玩家，如果玩家少于2，则表示有玩家掉线了
            if (m_ListPlayer.Count < 2)
            {
                //直接进入GameEnd状态
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.AllOtherLost);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndReason.AllOtherLost);
                return true;
            }

            return false;
        }


        void HandleGameState()
        {
            switch(m_State)
            {
                case FSPGameState.None:
                    //进入这个状态的游戏，马上将会被回收
                    //这里是否要考虑session中的所有消息都发完了？
                break;
                case FSPGameState.Create: //游戏刚创建，未有任何玩家加入, 这个阶段等待玩家加入
                    OnState_Create();
                break;
                case FSPGameState.GameBegin: //游戏开始，等待RoundBegin
                    OnState_GameBegin();
                break;
                case FSPGameState.RoundBegin: //回合已经开始，开始加载资源等，等待ControlStart
                    OnState_RoundBegin();
                break;
                case FSPGameState.ControlStart: //在这个阶段可操作，这时候接受游戏中的各种行为包，并等待RoundEnd
                    OnState_ControlStart();
                break;
                case FSPGameState.RoundEnd: //回合已经结束，判断是否进行下一轮，即等待RoundBegin，或者GameEnd
                    OnState_RoundEnd();
                break;
                case FSPGameState.GameEnd://游戏结束
                    OnState_GameEnd();
                break;
                default: break;
            }
        }

        /// <summary>
        /// 游戏创建状态
        /// 只有在该状态下，才允许加入玩家
        /// 当所有玩家都发VKey.GameBegin后，进入下一个状态
        /// </summary>
        protected virtual int OnState_Create()
        {
            //如果有任何一方已经鉴权完毕，则游戏进入GameBegin状态准备加载
            if (IsFlagFull(m_GameBeginFlag))
            {
                ResetRoundFlag();
                SetGameState(FSPGameState.GameBegin);
				AddCmdToCurrentFrame(FSPVKeyBase.GAME_BEGIN);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 游戏开始状态
        /// 在该状态下，等待所有玩家发VKey.RoundBegin，或者 判断玩家是否掉线
        /// 当所有人都发送VKey.RoundBegin，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_GameBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }
            
            if (IsFlagFull(m_RoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                GotoNextRound();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_BEGIN, m_CurRoundId);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 回合开始状态
        /// （这个时候客户端可能在加载资源）
        /// 在该状态下，等待所有玩家发VKey.ControlStart， 或者 判断玩家是否掉线
        /// 当所有人都发送VKey.ControlStart，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundBegin()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(m_ControlStartFlag))
            {
                SetGameState(FSPGameState.ControlStart);
                AddCmdToCurrentFrame(FSPVKeyBase.CONTROL_START);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 可以开始操作状态
        /// （因为每个回合可能都会有加载过程，不同的玩家加载速度可能不同，需要用一个状态统一一下）
        /// 在该状态下，接收玩家的业务VKey， 或者 VKey.RoundEnd，或者VKey.GameExit
        /// 当所有人都发送VKey.RoundEnd，进入下一个状态
        /// 当有玩家掉线，或者发送VKey.GameExit，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_ControlStart()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            if (IsFlagFull(m_RoundEndFlag))
            {
                SetGameState(FSPGameState.RoundEnd);
                ClearRound();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_END, m_CurRoundId);
                return 1;
            }
            return 0;
        }


        /// <summary>
        /// 回合结束状态
        /// （大部分游戏只有1个回合，也有些游戏有多个回合，由客户端逻辑决定）
        /// 在该状态下，等待玩家发送VKey.GameEnd，或者 VKey.RoundBegin（如果游戏不只1个回合的话）
        /// 当所有人都发送VKey.GameEnd，或者 VKey.RoundBegin时，进入下一个状态
        /// 当有玩家掉线，则从FSPGame中删除该玩家：
        ///     判断如果只剩下1个玩家了，则直接进入GameEnd状态，否则不影响游戏状态
        /// </summary>
        protected virtual int OnState_RoundEnd()
        {
            if (CheckGameAbnormalEnd())
            {
                return 0;
            }

            // 这是正常GameEnd
            if (IsFlagFull(m_GameEndFlag))
            {
                SetGameState(FSPGameState.GameEnd, (int)FSPGameEndReason.Normal);
                AddCmdToCurrentFrame(FSPVKeyBase.GAME_END, (int)FSPGameEndReason.Normal);
                return 1;
            }

            // 下一局
            if (IsFlagFull(m_RoundBeginFlag))
            {
                SetGameState(FSPGameState.RoundBegin);
                GotoNextRound();
                AddCmdToCurrentFrame(FSPVKeyBase.ROUND_BEGIN, m_CurRoundId);
                return 1;
            }
            return 0;
        }

        protected virtual int OnState_GameEnd()
        {
            //到这里就等业务层去读取数据了 
			if (onGameEnd != null) 
			{
				onGameEnd(m_StateParam1);
				onGameEnd = null;
			}
            return 0;
        }
        #endregion


        #region Round
        void ClearRound()
        {
            m_LockedFrame = new FSPFrame();
            m_CurFrameId = 0;
            
            ResetRoundFlag();

            for (int i = 0; i < m_ListPlayer.Count; i++)
            {
                if (m_ListPlayer[i] != null)
                {
                    m_ListPlayer[i].ClearRound();
                }
            }
        }

        void ResetRoundFlag()
        {
            m_RoundBeginFlag = 0;
            m_ControlStartFlag = 0;
            m_RoundEndFlag = 0;
            m_GameEndFlag = 0;
        }

        void GotoNextRound()
        {
            ++m_CurRoundId;
        }
        #endregion
    }
}