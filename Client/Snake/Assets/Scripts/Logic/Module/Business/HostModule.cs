using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using Framework.Network.FSP;

public class HostModule : BusinessModule 
{
    // TEST: 先在这边开房间，正式的要放到帧同步服务器
    FSPRoom fspRoom;

    public string ip { get{ return HasHost()? fspRoom.SelfIP : "";} }
    public int port { get{ return HasHost()? fspRoom.SelfPort : 0;} }


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


    public bool HasHost()
    {
        return fspRoom != null && fspRoom.IsRunning;
    }

    public void StartHost()
    {
        if (!HasHost())
        {
            fspRoom = new FSPRoom();
            fspRoom.Start();

            Debuger.Log("HostModule", string.Format("start host: {0}-{1}", ip, port));

            EventManager.Instance.SendEvent<string, int>("OnStartHost", ip, port);
            EventManager.Instance.SendEvent("OnHostChanged");

            Scheduler.AddUpdateListener(OnUpdate);
        }
        else
        {
            Debuger.Log("HostModule", "The host is alreay started");
        }
    }

    public void StopHost()
    {
        if (HasHost())
        {
            Scheduler.RemoveUpdateListener(OnUpdate);

            fspRoom.Destroy();
            fspRoom = null;

            Debuger.Log("HostModule", "The host is stoped");

            EventManager.Instance.SendEvent("OnStopHost");
            EventManager.Instance.SendEvent("OnHostChanged");
        }
    }

    void OnUpdate()
    {
        if (fspRoom != null)
            fspRoom.RPCTick();
    }

}
