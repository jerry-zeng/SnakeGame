using System.Collections.Generic;
using GameProtocol;
using Framework;
using Framework.Network;
using Framework.Network.RPC;
using System.Net;
using UnityEngine;

namespace GamePlay
{
    public class PVPRoom : RPCService
    {
        #region 成员变量
        private IPEndPoint m_roomAddress;

        private uint m_mainUserId;
		private string m_mainUserName = "";

        private bool m_isInRoom = false;
        private bool m_isReady = false;
        private int m_pingValue = 0;
        private List<FSPPlayerData> m_listPlayerInfo = new List<FSPPlayerData>();


        public bool IsReady{ get { return m_isReady; } }
        public bool IsInRoom{ get { return m_isInRoom; } }
        public int PingValue { get { return m_pingValue;} }
		public List<FSPPlayerData> players{ get{ return m_listPlayerInfo;}}
        #endregion

        #region 生命周期
        public PVPRoom() : base(0){
        }

        public void Start()
        {
            m_mainUserId = UserManager.Instance.UserData.id;
            m_mainUserName = UserManager.Instance.UserData.userName;
            Debuger.Log(LOG_TAG, "Start: userId={0}, name={1}", m_mainUserId, m_mainUserName);

            Scheduler.AddUpdateListener(OnUpdate);
        }

        public void Destroy()
        {
            Scheduler.RemoveUpdateListener(OnUpdate);

            Dispose();
        }

        void OnUpdate()
        {
            RPCTick();
        }

        void Reset()
		{
			m_isReady = false;
			m_isInRoom = false;
			m_listPlayerInfo.Clear();
		}
        #endregion

        //================================逻辑=============================
        public void JoinRoom(string ip, int port)
        {
            m_roomAddress = IPUtils.GetHostEndPoint(ip, port);

            PlayerData player = new PlayerData();
            player.userID = m_mainUserId;
            player.userName = m_mainUserName;
            byte[] customPlayerData = PBSerializer.Serialize(player);

            RPC(m_roomAddress, RoomRPC.RPC_JoinRoom, m_mainUserId, m_mainUserName, customPlayerData);
        }
        void _RPC_OnJoinRoom(IPEndPoint remote)
        {
            EventManager.Instance.SendEvent("OnJoinRoom");
        }

        public void ExitRoom()
        {
            RPC(m_roomAddress, RoomRPC.RPC_ExitRoom, m_mainUserId);

            Reset();

            // 等到下推再发事件？
            EventManager.Instance.SendEvent("OnExitRoom");
        }

        public void GetReady()
        {
            RPC(m_roomAddress, RoomRPC.RPC_RoomReady, m_mainUserId);
        }

        public void CancelReady()
        {
            RPC(m_roomAddress, RoomRPC.RPC_CancelReady, m_mainUserId);
        }

        public void Ping()
        {
            int t = (int)(Time.realtimeSinceStartup*1000);
            RPC(m_roomAddress, RoomRPC.RPC_Ping, t);
        }
        void _RPC_Pong(IPEndPoint remote, int pingArg)
        {
            m_pingValue = (int)(Time.realtimeSinceStartup*1000) - pingArg;
        }

        void _RPC_UpdateRoomInfo(IPEndPoint remote, byte[] bytes)
        {
            FSPRoomData data = PBSerializer.Deserialize<FSPRoomData>(bytes);
            m_listPlayerInfo = data.players;

            m_isInRoom = false;
            m_isReady = false;

            for (int i = 0; i < m_listPlayerInfo.Count; ++i)
            {
                if(m_listPlayerInfo[i].userId == m_mainUserId)
                {
                    m_isInRoom = true;
                    m_isReady = m_listPlayerInfo[i].isReady;
                }
            }

            EventManager.Instance.SendEvent("OnRoomUpdate");
        }

        void _RPC_NotifyGameStart(IPEndPoint remote, byte[] bytes)
        {
            // Debuger.Log(LOG_TAG, "_RPC_NotifyGameStart()");
            FSPGameStartParam data = PBSerializer.Deserialize<FSPGameStartParam>(bytes);

            PVPStartParam startParam = new PVPStartParam();
            startParam.fspParam = data.fspParam;
            startParam.gameParam = PBSerializer.Deserialize<GameParam>(data.customGameParam);

            for (int i = 0; i < data.players.Count; i++)
            {
                FSPPlayerData player = data.players[i];
                byte[] buff = player.customPlayerData;

                PlayerData pb = PBSerializer.Deserialize<PlayerData>(buff);
                pb.playerID = (int)player.id;
				pb.userID = (uint)player.userId;
				pb.userName = player.name;
				pb.teamID = (int)player.id;
                startParam.players.Add(pb);

                Debuger.Log(LOG_TAG, "_RPC_NotifyGameStart: {0}", pb.ToString());
            }
            
            // EventManager.Instance.SendEvent<PVPStartParam>("OnGameStart", startParam);
        }

        void _RPC_NotifyGameResult(IPEndPoint remote, int reason)
        {
            Debuger.Log(LOG_TAG, "_RPC_NotifyGameResult: {0}", reason);
            EventManager.Instance.SendEvent<int>("OnGameResult", reason);
        }
    }
}