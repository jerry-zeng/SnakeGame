using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;

public class UIRoomLobbyPanel : UIBasePanel 
{
    public Button btn_ShowHost;

    public Button btn_JoinRoom;
    public Text lab_JoinRoom;
    public Button btn_Ready;
    public Text lab_Ready;

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

        EventManager.Instance.RegisterEvent("OnJoinRoom", UpdateRoom);
        EventManager.Instance.RegisterEvent("OnExitRoom", UpdateRoom);
        EventManager.Instance.RegisterEvent("OnRoomUpdate", UpdateRoom);

        pvpModule = ModuleManager.Instance.EnsureModule<PVPModule>();

        UpdateRoom();
    }

    public override void Hide()
    {
        EventManager.Instance.UnregisterEvent("OnJoinRoom", UpdateRoom);
        EventManager.Instance.UnregisterEvent("OnExitRoom", UpdateRoom);
        EventManager.Instance.UnregisterEvent("OnRoomUpdate", UpdateRoom);

        pvpModule = null;

        base.Hide();
    }

    void UpdateRoom()
    {
        PVPRoom room = pvpModule.CurrentRoom;

        if (room.IsInRoom)
        {
            lab_JoinRoom.text = "退出房间";
        }
        else
        {
            lab_JoinRoom.text = "加入房间";
        }

        if (room.IsReady)
        {
            lab_Ready.text = "取消准备";
        }
        else
        {
            lab_Ready.text = "开始准备";
        }

        btn_JoinRoom.gameObject.SetActive(!room.IsReady);
        btn_Ready.gameObject.SetActive(room.IsInRoom);

        listView.SetData(room.players);
    }
}
