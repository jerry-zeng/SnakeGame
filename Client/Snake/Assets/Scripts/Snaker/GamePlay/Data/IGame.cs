using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameProtocol;

namespace GamePlay
{
    public interface IGame
    {
        GameMode gameMode { get; }
        bool isPaused { get; }
        bool isStoped { get; }

        void Start(object param);
        void Stop();

        void Pause();
        void Resume();

        // 单局的开始和结束
        void OnPlayerReady();
        void Terminate();

        // logic
        bool EnableRevive();
        bool EnablePause();

        void RebornPlayer();
        void OnPlayerDie(int playerId);
    }
}