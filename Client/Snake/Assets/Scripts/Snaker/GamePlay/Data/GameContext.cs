using System.Collections;
using System.Collections.Generic;
using Random = Framework.Random;
using GameProtocol;

namespace GamePlay
{
    public sealed class GameContext 
    {
        private GameParam _param;
        public GameParam Param
        {
            get{ return _param; }
        }

        private IGame _game;
        public IGame Game
        {
            get { return _game; }
        }

        private Random _random;
        public Random Random
        {
            get{ return _random; }
        }

        private int _currentFrame;
        public int CurrentFrame
        {
            get{ return _currentFrame; }
        }


        public GameContext(GameParam param, IGame game)
        {
            _param = param;
            _game = game;

            _random = new Random();
            _random.Seed = _param.randSeed;

            _currentFrame = 0;
        }

        public void EnterFrame(int frame)
        {
            _currentFrame = frame;
        }

        public void Clear()
        {
            _param = null;
            _game = null;

            _currentFrame = 0;
        }
        //----------------------------------游戏逻辑---------------------------------------
        /// <summary>
        /// 是否为限时模式
        /// </summary>
		public bool IsTimelimited
		{
			get
			{
				return Param.mode == GameMode.TimelimitPVE || Param.mode == GameMode.TimelimitPVP;
			}
		}

        /// <summary>
        /// 如果是限时模式，还剩下多少时间
        /// </summary>
        /// <returns></returns>
		public int GetRemainTime()
		{
			if (IsTimelimited)
			{
				return (int)(Param.limitTime - CurrentFrame * BattleEngine.TICK_INTERVAL);
			}
			return 0;
		}

        /// <summary>
        /// 游戏经过了多少时间
        /// </summary>
        /// <returns></returns>
		public int GetElapsedTime()
		{
			return (int)(CurrentFrame * BattleEngine.TICK_INTERVAL);
		}



        public bool isPaused 
        {
            get { return _game.isPaused; }
        }
        public void Pause()
		{
			_game.Pause();
		}
        public void Resume()
		{
			_game.Resume();
		}
    }
}
