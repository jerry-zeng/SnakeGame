using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;

public class UIRoomLobbyPanel : UIBasePanel 
{
    public Button btn_ShowHost;

    public Button btn_JoinRoom;
    public Button btn_Ready;

    public Button btn_Back;

    public UIListView listView;

    PVPModule pvpModule = null;


    public override void Setup()
    {
        base.Setup();

        btn_ShowHost.onClick.AddListener(OnClickShowHost);
        btn_JoinRoom.onClick.AddListener(OnClickJoinRoom);
        btn_Ready.onClick.AddListener(OnClickReady);
        btn_Back.onClick.AddListener(OnClickBack);
    }

    public override void Release()
    {
        btn_ShowHost.onClick.RemoveAllListeners();
        btn_JoinRoom.onClick.RemoveAllListeners();
        btn_Ready.onClick.RemoveAllListeners();
        btn_Back.onClick.RemoveAllListeners();

        base.Release();
    }


    void OnClickShowHost()
    {
        pvpModule.ShowHost();
    }

    void OnClickJoinRoom()
    {
        pvpModule.JoinRoom();
    }

    void OnClickReady()
    {
        pvpModule.GetReady();
    }

    void OnClickBack()
    {
        pvpModule.Close();
    }


    public override void Show(object args)
    {
        base.Show(args);

        pvpModule = ModuleManager.Instance.EnsureModule<PVPModule>();

        UpdatePlayerList();
    }

    public override void Hide()
    {
        pvpModule = null;

        base.Hide();
    }

    void UpdatePlayerList()
    {

    }
}
