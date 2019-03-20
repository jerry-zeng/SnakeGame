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
        base.Close();

        UIManager.Instance.PopUI();
    }

    public void ShowHost()
    {
        this.Log("OnClickShowHost()");
        MessageBox.Show("This function wasn't implemented yet.",
                        "OK",
                        MessageBox.Hide);
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

    public void Test()
    {
        this.Log("OnClickTest()");
    }
}
