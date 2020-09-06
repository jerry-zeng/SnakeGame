﻿using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class HostModule : BusinessModule 
{
    public string ip {get; private set;}
    public int port {get; private set;}


    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI(UIDef.UIRoomHostPanel);
    }

    public override void Close()
    {
        StopHost();

        UIManager.Instance.HideUI(UIDef.UIRoomHostPanel);

        base.Close();
    }


    public bool HasRoom()
    {
        return !string.IsNullOrEmpty(ip) && port > 0;
    }

    public void StartHost()
    {
        if (!HasRoom())
        {
            ip = "127.0.0.1";
            port = 1001;
            Debuger.Log("HostModule", string.Format("start host: {0}-{1}", ip, port));
        }
        else
        {
            Debuger.Log("HostModule", "The host is alreay started");
        }
    }

    public void StopHost()
    {
        if (HasRoom())
        {
            ip = "";
            port = 0;
            Debuger.Log("HostModule", "The host is stoped");
        }
    }
}
