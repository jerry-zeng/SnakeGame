using GameProtocol;

namespace Framework.Network.FSP.Client
{
    public interface IGameStateListener
    {
        void OnGameBegin(FSPVKey cmd);
        void OnRoundBegin(FSPVKey cmd);
        void OnControlStart(FSPVKey cmd);
        void OnRoundEnd(FSPVKey cmd);
        void OnGameEnd(FSPVKey cmd);
        void OnPlayerExit(FSPVKey cmd);
    }
}