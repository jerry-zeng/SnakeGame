using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;

public class PVEModule : BusinessModule 
{
    private PVEGame pveGame;
    public PVEGame currentGame 
    {
        get{ return pveGame; }
    }

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
        if (pveGame != null)
        {
            pveGame.Stop();
            pveGame = null;
        }

        ClearBattleView();

        ModuleManager.Instance.OpenModule(ModuleDef.LobbyModule);
    }


    void RequestEnterBattle()
    {
        // 服务器下推
        GameParam param = new GameParam();
        param.gameID = 1;
        param.randSeed = System.DateTime.Now.Millisecond;
        param.mode = GameProtocol.GameMode.TimelimitPVE;

        int stageID = (int)param.mode;
        var data = CSVTableLoader.GetTableContainer("Stage").GetRow(stageID.ToString());
        param.mapID = data["Map ID"].IntValue;
        param.limitTime = data["Limit Time"].FloatValue;
        param.limitPlayer = data["Limit Player"].IntValue;

        OnEnterBattleReply(param);
    }

    void OnEnterBattleReply(GameParam param)
    {
        if (pveGame != null)
        {
            Debuger.LogError("PVEModule", "The pveGame is already created!");
            return;
        }

        pveGame = new PVEGame();
        pveGame.Start(param);

        EventManager.Instance.SendEvent(EventDef.OnEnterBattle);

        CreateBattleView();
    }

    void CreateBattleView()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        UIManager.Instance.ShowUI( UIDef.UIBattlePanel );

        GameObject prefab = Resources.Load<GameObject>("GameInput");
        GameObject go = GameObject.Instantiate(prefab);
        go.name = "GameInput";
        GameInput.DisableInput();

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
