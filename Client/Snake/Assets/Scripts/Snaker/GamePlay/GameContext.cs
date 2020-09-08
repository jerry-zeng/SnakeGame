using System.Collections;
using System.Collections.Generic;
using Random = Framework.Random;

namespace GamePlay
{
    public class GameContext 
    {
        private GameParam _param;
        public GameParam Param
        {
            get{ return _param; }
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

        private bool m_pause = false;
        public bool isPaused {get {return m_pause;}}


        public GameContext(GameParam param)
        {
            _param = param;

            _random = new Random();
            _random.Seed = _param.randSeed;

            _currentFrame = 0;
        }

        public void EnterFrame(int frame)
        {
            _currentFrame = frame;
        }


        public void Pause()
		{
			m_pause = true;
		}
        public void Resume()
		{
			m_pause = false;
		}
    }
}
