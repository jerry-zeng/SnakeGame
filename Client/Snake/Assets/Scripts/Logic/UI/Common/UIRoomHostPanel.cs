using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;

public class UIRoomHostPanel : UIBasePanel 
{
    public Button btn_Close;
    public Text lab_roomIP;
    public Button btn_StartServer;
    public Button btn_StopServer;

    private HostModule hostModule;

    public override void Setup()
    {
        base.Setup();

        btn_Close.onClick.AddListener( OnClickClose );
        btn_StartServer.onClick.AddListener(OnClickStartServer);
        btn_StopServer.onClick.AddListener(OnClickStopServer);
    }

    public override void Release()
    {
        btn_Close.onClick.RemoveAllListeners();
        btn_StartServer.onClick.RemoveAllListeners();
        btn_StopServer.onClick.RemoveAllListeners();

        base.Release();
    }

    void OnClickClose()
    {
        UIManager.Instance.HideUI(CachedGameObject.name);
    }


    void OnClickStartServer()
    {
        this.Log("OnClickStartServer()");
        hostModule.StartHost();
        UpdateHost();
    }

    void OnClickStopServer()
    {
        this.Log("OnClickStopServer()");
        hostModule.StopHost();
        UpdateHost();
    }


    public override void Show(object args)
    {
        base.Show(args);

        EventManager.Instance.RegisterEvent("OnHostChanged", UpdateHost);

        hostModule = ModuleManager.Instance.EnsureModule<HostModule>();

        UpdateHost();
    }

    public override void Hide()
    {
        EventManager.Instance.UnregisterEvent("OnHostChanged", UpdateHost);

        hostModule = null;

        lab_roomIP.text = "";

        base.Hide();
    }


    void UpdateHost()
    {
        if (hostModule.HasHost())
        {
            lab_roomIP.text = string.Format("{0}:{1}", hostModule.ip, hostModule.port);
        }
        else
        {
            lab_roomIP.text = "";
        }
    }
}

