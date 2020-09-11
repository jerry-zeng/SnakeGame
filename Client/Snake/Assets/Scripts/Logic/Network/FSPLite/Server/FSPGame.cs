using System;
using System.Collections.Generic;
using GameProtocol;

namespace Framework.Network.FSP.Server
{
    public class FSPGame
    {
        
        //有一个玩家退出游戏
		public event Action<uint> onGameExit;
		//游戏真正结束
		public event Action<int> onGameEnd;


        

        public void Start(FSPParam param)
        {
            
        }

        public void Stop()
        {
            onGameExit = null;
            onGameEnd = null;
        }

        public void EnterFrame()
        {

        }
    }
}