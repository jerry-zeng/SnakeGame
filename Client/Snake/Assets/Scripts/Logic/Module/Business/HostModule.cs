using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using Framework.Network.FSP.Server;

public class HostModule : BusinessModule 
{
    public string ip { get{ return HasHost()? FSPServer.Instance.RoomIP : "";} }
    public int port { get{ return HasHost()? FSPServer.Instance.RoomPort : 0;} }


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
        return FSPServer.Instance.IsRunning;
    }

    public void StartHost()
    {
        if (!HasHost())
        {
            FSPServer.Instance.Start(0);
            FSPServer.Instance.SetServerTimeout(0);
            //将自定义游戏参数传给房间
            //以便于由房间通知玩家游戏开始时，能够将该参数转发给所有玩家
            byte[] customGameParam = PBSerializer.Serialize(new GamePlay.GameParam());
            FSPServer.Instance.Room.SetCustomGameParam(customGameParam);

            Debuger.Log("HostModule", string.Format("start host: {0}-{1}", ip, port));

            EventManager.Instance.SendEvent<string, int>("OnStartHost", ip, port);
            EventManager.Instance.SendEvent("OnHostChanged");
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
            FSPServer.Instance.Close();

            Debuger.Log("HostModule", "The host is stoped");

            EventManager.Instance.SendEvent("OnStopHost");
            EventManager.Instance.SendEvent("OnHostChanged");
        }
    }

}
