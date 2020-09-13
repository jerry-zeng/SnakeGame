using System.Collections.Generic;
using GameProtocol;
using Framework;
using Framework.Network;
using Framework.Network.RPC;
using System.Net;

namespace Framework.Network.FSP.Server
{
    public class FSPRoom : RPCService
    {
        FSPRoomData m_data = new FSPRoomData();
        Dictionary<uint, IPEndPoint> m_mapUserId2Address = new Dictionary<uint, IPEndPoint>();
        byte[] m_customGameParam;

        public uint Id { get{ return m_data.id;} }


        public FSPRoom() : base(0){

        }

        public void Start()
        {
            m_data.id = 1;
        }

        public void Destroy()
        {
            Dispose();
        }

        //???
        public void SetCustomGameParam(byte[] gameParam)
        {
            m_customGameParam = gameParam;
        }

        // 逻辑接口------------------------------------------------------------------------------------
        FSPPlayerData GetPlayerInfoByUserId(uint userId)
        {
            for (int i = 0; i < m_data.players.Count; i++)
            {
                if (m_data.players[i].userId == userId)
                {
                    return m_data.players[i];
                }
            }
            return null;
        }
        int GetPlayerIndexByUserId(uint userId)
        {
            for (int i = 0; i < m_data.players.Count; i++)
            {
                if (m_data.players[i].userId == userId)
                {
                    return i;
                }
            }
            return -1;
        }
        
        List<IPEndPoint> GetAllAddress()
        {
            List<IPEndPoint> list = new List<IPEndPoint>();

            var players = m_data.players;
            for (int i = 0; i <players.Count; i++)
            {
                uint userId = players[i].userId;
                // 这里如果报错，说明前面添加和删除出错了
                if ( m_mapUserId2Address.ContainsKey(userId) )
                    list.Add(m_mapUserId2Address[userId]);
            }

            return list;
        }


        void AddPlayer(IPEndPoint remote, uint userId, string name, byte[] customPlayerData)
        {
            FSPPlayerData player = GetPlayerInfoByUserId(userId);
            if (player == null)
            {
                player = new FSPPlayerData();
                m_data.players.Add(player);

                player.id = (uint)m_data.players.Count;
                player.sid = player.id;
            }
            
            player.userId = userId;
            player.name = name;
            player.customPlayerData = customPlayerData;

            // 刚进来不准备
            player.isReady = false;

            m_mapUserId2Address[userId] = remote;
        }

        void RemovePlayerById(uint playerId)
        {
            var players = m_data.players;

            for (int i = players.Count-1; i >= 0; i--)
            {
                if (players[i].id == playerId)
                {
                    uint userId = players[i].userId;
                    if ( m_mapUserId2Address.ContainsKey(userId) )
                        m_mapUserId2Address.Remove(userId);

                    players.RemoveAt(i); 
                }
            }
        }
        void RemovePlayerByUserId(uint userId)
        {
            var players = m_data.players;

            for (int i = players.Count-1; i >= 0; i--)
            {
                if (players[i].userId == userId)
                {
                    if ( m_mapUserId2Address.ContainsKey(userId) )
                        m_mapUserId2Address.Remove(userId);

                    players.RemoveAt(i); 
                }
            }
        }

        void SetReady(uint userId, bool ready)
        {
            FSPPlayerData player = GetPlayerInfoByUserId(userId);
            if (player != null)
            {
                player.isReady = ready;
            }
        }

        bool CanStartGame()
        {
            return m_data.players.Count > 1 && IsAllReady();
        }
        bool IsAllReady()
        {
            var players = m_data.players;

            for (int i = players.Count-1; i >= 0; i--)
            {
                if (!players[i].isReady)
                {
                    return false;
                }
            }
            return true;
        }

        // RPC绑定-------------------------------------------------------------------------------------------------
        void _RPC_JoinRoom(IPEndPoint remote, uint userId, string name, byte[] customPlayerData)
        {
            Debuger.Log(LOG_TAG, "_RPC_JoinRoom: userId={0}, name={1}", userId, name);
            AddPlayer(remote, userId, name, customPlayerData);

            RPC(remote, RoomRPC.RPC_OnJoinRoom);
            Call_UpdateRoomInfo();
        }

        void _RPC_ExitRoom(IPEndPoint remote, uint userId)
        {
            RemovePlayerByUserId(userId);

            RPC(remote, RoomRPC.RPC_OnExitRoom);
            Call_UpdateRoomInfo();

            // 有人退出可能就可以开始了
            if (CanStartGame())
            {
                Call_NotifyGameStart();
            }
        }

        void _RPC_RoomReady(IPEndPoint remote, uint userId)
        {
            Debuger.Log(LOG_TAG, "_RPC_RoomReady: userId={0}", userId);
            SetReady(userId, true);
            Call_UpdateRoomInfo();

            if (CanStartGame())
            {
                Call_NotifyGameStart();
            }
        }

        void _RPC_CancelReady(IPEndPoint remote, uint userId)
        {
            Debuger.Log(LOG_TAG, "_RPC_CancelReady: userId={0}", userId);
            SetReady(userId, false);
            Call_UpdateRoomInfo();
        }

        void _RPC_Ping(IPEndPoint remote, int pingArg)
        {
            RPC(remote, RoomRPC.RPC_Pong, pingArg);
        }

        void Call_UpdateRoomInfo()
        {
            var players = m_data.players;

			for (int i = 0; i < players.Count; ++i) 
			{
				Debuger.Log(LOG_TAG, "Call_UpdateRoomInfo() Player: {0}-{1}", players[i].userId, players[i].name);
			}

            byte[] param = PBSerializer.Serialize(m_data);
            RPC(GetAllAddress(), RoomRPC.RPC_UpdateRoomInfo, param);
        }

        void Call_NotifyGameStart()
        {
            Debuger.Log(LOG_TAG, "Call_NotifyGameStart()");

            var players = m_data.players;

            FSPGameStartParam param = new FSPGameStartParam();
            param.fspParam = FSPServer.Instance.GetParam();

            foreach(var player in players)
            {
                param.players.Add(player);
            }
            param.customGameParam = m_customGameParam;

            FSPServer.Instance.StartGame();
            FSPServer.Instance.Game.onPlayerExit += _OnPlayerExit;
            FSPServer.Instance.Game.onGameEnd += _OnGameEnd;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                
                param.fspParam.sid = player.sid;

                //将玩家加入到FSPServer中
                FSPServer.Instance.Game.AddPlayer(player.id, player.sid);

                if (m_mapUserId2Address.ContainsKey(player.userId))
                {
                    IPEndPoint address = m_mapUserId2Address[player.userId];

                    byte[] buff = PBSerializer.Serialize(param);
                    RPC(address, RoomRPC.RPC_NotifyGameStart, buff);
                }
                else
                {
                    Debuger.LogError(LOG_TAG, "User {0} does not have net address", player.userId);
                }
            }
        }

        //游戏逻辑--------------------------------------------------
        void _OnPlayerExit(uint playerId)
		{
			RemovePlayerById(playerId);
			Call_UpdateRoomInfo();
		}

		void _OnGameEnd(int reason)
		{
			FSPServer.Instance.StopGame();

			RPC(GetAllAddress(), RoomRPC.RPC_NotifyGameResult, reason);
		}

    }
}