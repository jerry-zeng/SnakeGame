using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class LobbyModule : BusinessModule 
{
    
    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();

        UIManager.Instance.ShowUI( UIDef.UILobbyPanel );
    }


    public void OpenModule(string moduleName, object arg = null)
    {
        ModuleManager.Instance.OpenModule(moduleName, arg);
    }
}
