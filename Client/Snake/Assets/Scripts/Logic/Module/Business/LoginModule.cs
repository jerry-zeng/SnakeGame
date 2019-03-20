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
