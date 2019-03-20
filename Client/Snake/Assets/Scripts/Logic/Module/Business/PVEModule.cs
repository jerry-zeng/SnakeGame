using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;

public class PVEModule : BusinessModule 
{
    public override void Open(object arg)
    {
        base.Open(arg);

        //Toast.Show( "This function wasn't implemented yet." );

        GameParam param = new GameParam();
        param.gameID = 1;
        param.randSeed = System.DateTime.Now.Millisecond;
        param.mode = GameProtocol.GameMode.TimelimitPVE;

        OnEnterBattleReply(param);
    }

    void OnEnterBattleReply(GameParam param)
    {
        int stageID = (int)param.mode;
        var data = CSVTableLoader.GetTableContainer("Stage").GetRow(stageID.ToString());
        param.mapID = data["Map ID"].IntValue;
        param.limitTime = data["Limit Time"].FloatValue;
        param.limitPlayer = data["Limit Player"].IntValue;
        BattleEngine.Instance.EnterBattle(param);

        EventManager.Instance.SendEvent(EventDef.OnEnterBattle);
    }
}
