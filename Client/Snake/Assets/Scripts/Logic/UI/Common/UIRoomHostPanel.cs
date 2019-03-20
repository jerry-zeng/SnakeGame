using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UIRoomHostPanel : UIBasePanel 
{
    public Text lab_roomIP;
    public Button btn_Close;
    public Button btn_StartServer;
    public Button btn_StopServer;


    public override void Setup()
    {
        base.Setup();

        btn_Close.onClick.AddListener( OnClickClose );
        btn_StartServer.onClick.AddListener(OnClickStartServer);
        btn_StopServer.onClick.AddListener(OnClickStopServer);
    }

    void OnClickClose()
    {
        UIManager.Instance.HideUI(CachedGameObject.name);
    }

    void OnClickStartServer()
    {
        this.Log("OnClickStartServer()");
    }

    void OnClickStopServer()
    {
        this.Log("OnClickStopServer()");
    }

    public override void Show(object args)
    {
        base.Show(args);

        lab_roomIP.text = "";
    }

    public override void Hide()
    {
        lab_roomIP.text = "";

        base.Hide();
    }
}

