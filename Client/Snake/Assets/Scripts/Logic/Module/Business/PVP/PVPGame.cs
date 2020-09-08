using GameProtocol;
using Framework;

namespace GamePlay
{
    public class PVPGame
    {
        private int m_frameIndex = 0;
        private int _mainPlayerId = 0;

        public void Start(PVPStartParam startParam)
        {
            // RegisterPlayer();

            // GameInput.onVKey = OnVKey;
            // GameInput.DisableInput();

            // Scheduler.AddFixedUpdateListener(FixedUpdate);

            // BattleEngine.Instance.EnterBattle(param);

        }

        public void Stop()
        {
            // GameInput.DisableInput();

            // m_context = null;

            // Scheduler.RemoveFixedUpdateListener(FixedUpdate);

            // BattleEngine.Instance.EndBattle();
        }
    }
}