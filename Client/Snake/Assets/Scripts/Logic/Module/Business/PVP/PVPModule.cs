using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;
using GameProtocol;
using UnityEngine;

public class PVPModule : BusinessModule 
{
    PVPRoom pvpRoom;
    PVPGame pvpGame;

    public PVPRoom CurrentRoom { get { return pvpRoom; } }
    public PVPGame CurrentGame { get { return pvpGame; } }


    public override void Open(object arg)
    {
        base.Open(arg);

        EventManager.Instance.RegisterEvent<PVPStartParam>("OnGameStart", OnGameStart);
        EventManager.Instance.RegisterEvent<int>("OnGameResult", OnGameResult);
        EventManager.Instance.RegisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);
        EventManager.Instance.RegisterEvent<string, int>("OnStartHost", OnStartHost);
        EventManager.Instance.RegisterEvent("OnStopHost", OnStopHost);
        EventManager.Instance.RegisterEvent<string>("TryToJoinRoom", OnTryToJoinRoom);

        CreateRoom();

        UIManager.Instance.ShowUI( UIDef.UIRoomLobbyPanel );
    }

    public override void Close()
    {
        EventManager.Instance.UnregisterEvent<PVPStartParam>("OnGameStart", OnGameStart);
        EventManager.Instance.UnregisterEvent<int>("OnGameResult", OnGameResult);
        EventManager.Instance.UnregisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);
        EventManager.Instance.UnregisterEvent<string, int>("OnStartHost", OnStartHost);
        EventManager.Instance.UnregisterEvent("OnStopHost", OnStopHost);
        EventManager.Instance.UnregisterEvent<string>("TryToJoinRoom", OnTryToJoinRoom);

        CloseRoom();
        StopHost();

        UIManager.Instance.HideUI( UIDef.UIRoomLobbyPanel );

        base.Close();
    }

    void CreateRoom()
    {
        if (pvpRoom == null)
        {
            pvpRoom = new PVPRoom();
            pvpRoom.Start();
        }
    }
    void CloseRoom()
    {
        if (pvpRoom != null)
        {
            if (pvpRoom.IsInRoom)
                pvpRoom.ExitRoom();

            pvpRoom.Destroy();
            pvpRoom = null;
        }

        //返回上一个UI
        UIManager.Instance.PopUI();
    }



    void StopHost()
    {
        HostModule hostModule = ModuleManager.Instance.EnsureModule<HostModule>();
        hostModule.StopHost();
    }
    public void ShowHost()
    {
        this.Log("OnClickShowHost()");
        ModuleManager.Instance.OpenModule(ModuleDef.HostModule);
    }

    void OnStartHost(string ip, int port)
    {
        //自动加入房间
        if (pvpRoom != null)
        {
            pvpRoom.JoinRoom(ip, port);
        }
    }
    void OnStopHost()
    {
        if (pvpRoom != null)
        {
            if (pvpRoom.IsInRoom)
            {
                pvpRoom.ExitRoom();
            }
        }
    }

    public void JoinRoom()
    {
        this.Log("OnClickJoinRoom()");

        if (pvpRoom != null)
        {
            if (pvpRoom.IsInRoom)
            {
                pvpRoom.ExitRoom();
            }
            else
            {
                UIManager.Instance.ShowUI( UIDef.UIRoomSearchPanel );
            }
        }
    }
    void OnTryToJoinRoom(string ipStr)
    {
        if(string.IsNullOrEmpty(ipStr) || pvpRoom == null)
        {
            return;
        }
        string[] tmps = ipStr.Split(':');
        if(tmps.Length != 2)
        {
            this.LogWarning("OnBtnJoinRoom() RoomIPPort 格式错误！");
            return;
        }
        string ip = tmps[0];
        int port = int.Parse(tmps[1]);

        pvpRoom.JoinRoom(ip, port);
    }

    public void GetReady()
    {
        this.Log("OnClickReady()");

        if (pvpRoom != null)
        {
            if (pvpRoom.IsReady)
                pvpRoom.CancelReady();
            else
                pvpRoom.GetReady();
        }
    }

    //==============================================
    void OnGameStart(PVPStartParam startParam)
    {
        pvpGame = new PVPGame();
        pvpGame.Start(startParam);

        EventManager.Instance.SendEvent(EventDef.OnEnterBattle);

        CreateBattleView();
    }

    void OnGameResult(int reason)
    {
        if (pvpGame != null) 
        {
            pvpGame.Stop();
        }

        if (pvpRoom != null) 
        {
            pvpRoom.CancelReady();
        }

        // 重来？
    }

    // 本地测试
    public void StartLocalTest()
    {
        //战斗参数
        GameParam gameParam = new GameParam();
        gameParam.mapID = 0;
        gameParam.mode = GameMode.EndlessPVP;

        //帧同步参数
        FSPParam fspParam = new FSPParam();
        fspParam.useLocal = true;
        fspParam.sid = 1;

        //玩家参数
        PlayerData playerData = new PlayerData();
        playerData.userID = UserManager.Instance.UserData.id;
        playerData.playerID = 1;

        PVPStartParam param = new PVPStartParam();
        param.fspParam = fspParam;
        param.gameParam = gameParam;
        param.players.Add(playerData);

        OnGameStart(param);
    }

    //=========================================
    void OnLeaveBattle()
    {
        if (pvpGame != null)
        {
            pvpGame.Stop();
            pvpGame = null;
        }

        ClearBattleView();

        ModuleManager.Instance.OpenModule(ModuleDef.LobbyModule);
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
