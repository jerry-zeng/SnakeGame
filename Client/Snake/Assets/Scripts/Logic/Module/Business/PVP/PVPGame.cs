using System.Collections.Generic;
using GameProtocol;
using Framework;
using GameProtocol;

namespace GamePlay
{
    public class PVPGame : IGame
    {
        private int m_frameIndex = 0;
        private int _mainPlayerId = 0;

        private GameContext m_context;


        public bool isPaused 
        {
            get { return false; }
        }

        private bool _isStoped = false;
        public bool isStoped
        {
            get { return _isStoped; }
        }

        private GameMode _gameMode = GameMode.TimelimitPVP;
        public GameMode gameMode
        {
            get { return _gameMode; }
        }


        public void Start(object param)
        {
            _isStoped = false;

            PVPStartParam startParam = param as PVPStartParam;

            _gameMode = startParam.gameParam.mode;

            // 注册玩家
            uint mainUserId = UserManager.Instance.UserData.id;

            var players = startParam.players;
            for(int i = 0; i < players.Count; i++)
            {
                if (players[i].userID == mainUserId)
                {
                    _mainPlayerId = players[i].playerID;
                }

                //注册玩家数据，为在帧同步过程中创建玩家提供数据
                //因为帧同步协议非常精简，不包含具体的玩家数据
				BattleEngine.Instance.RegPlayerData(players[i]);
            }

            // 创建游戏
            BattleEngine.Instance.EnterBattle(startParam.gameParam, this);
            m_context = BattleEngine.Instance.Context;

            // 帧同步
            StartFSP();

            // 接收输入
            GameInput.onVKey = OnVKey;
            GameInput.DisableInput();

            Scheduler.AddFixedUpdateListener(FixedUpdate);
        }

        public void Stop()
        {
            _isStoped = true;
            GameInput.DisableInput();

            m_context = null;

            Scheduler.RemoveFixedUpdateListener(FixedUpdate);

            BattleEngine.Instance.ExitBattle();

            StopFSP();
        }

        //----------------------------------帧同步 & 输入------------------------------------
        void StartFSP()
        {

        }

        void StopFSP()
        {

        }

        void OnVKey(int vkey, float arg)
        {
            // BattleEngine.Instance.InputVKey(vkey, arg, _mainPlayerId);
        }

        public void OnPlayerReady()
        {
            
        }

        public void RebornPlayer()
		{
			this.LogError("PVPGame can't reborn");
		}


        /// <summary>
        /// 驱动游戏逻辑循环
        /// </summary>
		void FixedUpdate()
		{
			if (m_context.isPaused)
			{
				return;
			}

			m_frameIndex++;

			BattleEngine.Instance.EnterFrame(m_frameIndex);

			// CheckTimeEnd();
		}

        //------------------------------------基类接口----------------------------------
        public void Pause()
		{
            // can't pause
		}
		public void Resume()
		{
			// can't pause
		}
		public void Terminate()
		{
            // can't handle by manual
		}
        public bool EnablePause()
        {
            return false;
        }
        public bool EnableRevive()
        {
            return false;
        }

        //--------------------------------------其它事件--------------------------------
        public void OnPlayerDie(int playerId)
		{
            Debuger.Log("PVPGame", "OnPlayerDie {0}, _mainPlayerId = {1}", playerId, _mainPlayerId);
			if (_mainPlayerId == playerId)
			{
				GameInput.DisableInput();

                // 可以观战
                EventManager.Instance.SendEvent("OpenGodOfView");
			}
		}


        //----------------------------------------------------------------------
    }
}