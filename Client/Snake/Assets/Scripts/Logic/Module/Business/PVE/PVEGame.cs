using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Module;
using GameProtocol;
using Framework;

namespace GamePlay
{
    public class PVEGame
    {
        private int m_frameIndex = 0;
        private int _mainPlayerId = 0;
        private GameContext m_context;

        public bool isPaused 
        {
            get { return m_context.isPaused; }
        }


        public void Start(GameParam param)
        {
            RegisterPlayer();

            GameInput.onVKey = OnVKey;
            GameInput.DisableInput();

            Scheduler.AddFixedUpdateListener(FixedUpdate);

            BattleEngine.Instance.EnterBattle(param);

            m_context = BattleEngine.Instance.Context;
        }

        public void Stop()
        {
            GameInput.DisableInput();

            m_context = null;

            Scheduler.RemoveFixedUpdateListener(FixedUpdate);

            BattleEngine.Instance.EndBattle();
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
		private void FixedUpdate()
		{
			if (m_context.isPaused)
			{
				return;
			}


			m_frameIndex++;

			BattleEngine.Instance.EnterFrame(m_frameIndex);

			CheckTimeEnd ();
		}

        /// <summary>
        /// 暂停游戏
        /// </summary>
		public void Pause()
		{
            m_context.Pause();

            GameInput.DisableInput();
		}

        /// <summary>
        /// 恢复游戏
        /// </summary>
		public void Resume()
		{
			m_context.Resume();

            GameInput.EnableInput();
		}


        /// <summary>
        /// 结束游戏
        /// </summary>
		public void Terminate()
		{
			Pause();

            BattleEngine.Instance.EndBattle();
			EventManager.Instance.SendEvent(EventDef.OnGameEnd);
		}

        /// <summary>
        /// 检测游戏是否限时结束
        /// </summary>
		private void CheckTimeEnd()
		{
			if (IsTimelimited)
			{
				if (GetRemainTime() <= 0)
				{
					Terminate();
				}
			}
		}


        /// <summary>
        /// 是否为限时模式
        /// </summary>
		public bool IsTimelimited
		{
			get
			{
				return m_context.Param.mode == GameMode.TimelimitPVE;
			}
		}

        /// <summary>
        /// 如果是限时模式，还剩下多少时间
        /// </summary>
        /// <returns></returns>
		public int GetRemainTime()
		{
			if (m_context.Param.mode == GameMode.TimelimitPVE)
			{
				return (int)(m_context.Param.limitTime - m_context.CurrentFrame * BattleEngine.TICK_INTERVAL);
			}
			return 0;
		}

        /// <summary>
        /// 游戏经过了多少时间
        /// </summary>
        /// <returns></returns>
		public int GetElapsedTime()
		{
			return (int)(m_context.CurrentFrame * BattleEngine.TICK_INTERVAL);
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