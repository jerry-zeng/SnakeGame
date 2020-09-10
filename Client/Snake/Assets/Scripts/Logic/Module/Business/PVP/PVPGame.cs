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

        private GameMode _gameMode = GameMode.TimelimitPVP;
        public GameMode gameMode
        {
            get { return _gameMode; }
        }


        public void Start(object param)
        {
            PVPStartParam startParam = param as PVPStartParam;

            // RegisterPlayer();

            // GameInput.onVKey = OnVKey;
            // GameInput.DisableInput();

            // Scheduler.AddFixedUpdateListener(FixedUpdate);

            // BattleEngine.Instance.EnterBattle(startParam);

        }

        public void Stop()
        {
            // GameInput.DisableInput();

            // m_context = null;

            // Scheduler.RemoveFixedUpdateListener(FixedUpdate);

            // BattleEngine.Instance.EndBattle();
        }


        public void OnPlayerReady()
        {
            // BattleEngine.Instance.InputVKey((int)GameVKey.CreatePlayer, 0, _mainPlayerId);

            // GameInput.EnableInput();

            // BattleView.Instance.FocusOnPlayer(_mainPlayerId);
        }

        public void RebornPlayer()
		{
			this.LogError("PVPGame can't reborn");
		}

        public void Pause()
		{
            // can't pause
		}

        /// <summary>
        /// 恢复游戏
        /// </summary>
		public void Resume()
		{
			// can't pause
		}

        /// <summary>
        /// 结束游戏
        /// </summary>
		public void Terminate()
		{
            // BattleEngine.Instance.EndBattle();
			// EventManager.Instance.SendEvent(EventDef.OnGameEnd);
		}

        public bool EnablePause()
        {
            return false;
        }
        public bool EnableRevive()
        {
            return false;
        }

        public void OnPlayerDie(int playerId)
		{
            Debuger.Log("PVEGame", "OnPlayerDie {0}, _mainPlayerId = {1}", playerId, _mainPlayerId);
			if (_mainPlayerId == playerId)
			{
				GameInput.DisableInput();

                // 可以观战
                EventManager.Instance.SendEvent("OpenGodOfView");
			}
		}
    }
}