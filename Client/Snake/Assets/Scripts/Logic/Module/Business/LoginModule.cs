using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;

public class LoginModule : BusinessModule 
{
    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();

        UIManager.Instance.ShowUI( UIDef.UILoginPanel );
        EventManager.Instance.RegisterEvent(EventDef.OnLogin, OnLogin);
    }

    public override void Close()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnLogin, OnLogin);
    }

    void OnLogin()
    {
        ModuleManager.Instance.OpenModule(ModuleDef.LobbyModule);
    }


    public void Login(uint id, string userName, string password)
    {
        UserData data = new UserData();
        data.id = id;
        data.userName = userName;
        data.password = password;

        OnHandlerLoginResult(ResultCode.RC_SUCCESS, data );
    }

    private void OnHandlerLoginResult(ResultCode result, UserData data)
    {
        UserManager.Instance.UpdateLoginUser(data);
        EventManager.Instance.SendEvent(EventDef.OnLogin);
    }
}
