using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class PVPModule : BusinessModule 
{
    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UIRoomLobbyPanel );
    }

    public override void Close()
    {
        StopHost();

        UIManager.Instance.PopUI();

        base.Close();
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

    public void JoinRoom()
    {
        this.Log("OnClickJoinRoom()");
        UIManager.Instance.ShowUI( UIDef.UIRoomLobbyPanel );
    }

    public void GetReady()
    {
        this.Log("OnClickReady()");
    }
}
