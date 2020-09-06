using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;

public class PVEModule : BusinessModule 
{
    public override void Open(object arg)
    {
        base.Open(arg);

        EventManager.Instance.RegisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);

        RequestEnterBattle();
    }

    public override void Close()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);
    }

    void OnLeaveBattle()
    {
        ClearBattleView();

        ModuleManager.Instance.OpenModule(ModuleDef.LobbyModule);
    }


    void RequestEnterBattle()
    {
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

        CreateBattleView();
    }

    void CreateBattleView()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        UIManager.Instance.ShowUI( UIDef.UIBattlePanel );

        GameObject prefab = Resources.Load<GameObject>("GameInput");
        GameObject go = GameObject.Instantiate(prefab);
        go.name = "GameInput";

        go = new GameObject("BattleView");
        go.AddComponent<BattleView>();
    }

    void ClearBattleView()
    {
        DestroyObject("GameInput");
        DestroyObject("BattleView");
    }
    void DestroyObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
            Object.Destroy(go);
    }
}
