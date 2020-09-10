using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Module;
using GameProtocol;
using Framework;

namespace GamePlay
{
    public class PVEGame : IGame
    {
        private int m_frameIndex = 0;
        private int _mainPlayerId = 0;
        
        private GameContext m_context;

        private bool m_pause = false;
        public bool isPaused 
        {
            get { return m_pause; }
        }

        private bool _isStoped = false;
        public bool isStoped
        {
            get { return _isStoped; }
        }

        private GameMode _gameMode = GameMode.TimelimitPVE;
        public GameMode gameMode
        {
            get { return _gameMode; }
        }


        public void Start(object param)
        {
            _isStoped = false;

            GameParam gameParam = param as GameParam;
            _gameMode = gameParam.mode;

            RegisterPlayer();

            GameInput.onVKey = OnVKey;
            GameInput.DisableInput();

            Scheduler.AddFixedUpdateListener(FixedUpdate);

            BattleEngine.Instance.EnterBattle(gameParam, this);

            m_context = BattleEngine.Instance.Context;
        }

        public void Stop()
        {
            _isStoped = true;

            GameInput.DisableInput();

            m_context = null;

            Scheduler.RemoveFixedUpdateListener(FixedUpdate);

            BattleEngine.Instance.ExitBattle();
        }

        void RegisterPlayer()
        {
            _mainPlayerId = BattleEngine.Instance.GenerateNewPlayerID();

            var userData = UserManager.Instance.UserData;
            int snakerID = UserManager.Instance.CurrentSnakerID;

            PlayerData player = new PlayerData();
            player.playerID = _mainPlayerId;
            player.teamID = 1;
            player.snakerData = new SnakerData(){id = snakerID, length = 0 };
            player.aiID = 0;
            player.userID = userData.id;
            player.userName = userData.userName;

            BattleEngine.Instance.RegPlayerData(player);
        }

        public void OnPlayerReady()
        {
            BattleEngine.Instance.InputVKey((int)GameVKey.CreatePlayer, 0, _mainPlayerId);

            GameInput.EnableInput();

            BattleView.Instance.FocusOnPlayer(_mainPlayerId);
        }

        public void RebornPlayer()
		{
			OnPlayerReady();
		}

        void OnVKey(int vkey, float arg)
        {
            BattleEngine.Instance.InputVKey(vkey, arg, _mainPlayerId);
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

			CheckTimeEnd();
		}

        /// <summary>
        /// 暂停游戏
        /// </summary>
		public void Pause()
		{
            m_pause = true;

            GameInput.DisableInput();
		}

        /// <summary>
        /// 恢复游戏
        /// </summary>
		public void Resume()
		{
			m_pause = false;

            GameInput.EnableInput();
		}


        /// <summary>
        /// 结束游戏
        /// </summary>
		public void Terminate()
		{
			Pause();

            BattleEngine.Instance.ClearBattle();
			EventManager.Instance.SendEvent(EventDef.OnGameEnd);
		}

        /// <summary>
        /// 检测游戏是否限时结束
        /// </summary>
		private void CheckTimeEnd()
		{
			if (m_context.IsTimelimited)
			{
				if (m_context.GetRemainTime() <= 0)
				{
					Terminate();
				}
			}
		}

        public bool EnablePause()
        {
            return !m_pause && !_isStoped;
        }
        public bool EnableRevive()
        {
            Snaker snaker = BattleEngine.Instance.GetSnakerByID(_mainPlayerId);
            return snaker != null && snaker.IsDead;
        }

        public void OnPlayerDie(int playerId)
		{
            Debuger.Log("PVEGame", "OnPlayerDie {0}, _mainPlayerId = {1}", playerId, _mainPlayerId);
			if (_mainPlayerId == playerId)
			{
				Pause();
			}
		}
    }
}