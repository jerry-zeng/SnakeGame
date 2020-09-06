using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;

public class UpdateModule : BusinessModule 
{

    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();

        UIManager.Instance.ShowUI( UIDef.UIUpdatePanel );
        EventManager.Instance.RegisterEvent(EventDef.OnUpdateEnd, OnUpdateEnd);

        PatchManager.Instance.Start();
    }

    public override void Close()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnUpdateEnd, OnUpdateEnd);
    }

    void OnUpdateEnd()
    {
        ModuleManager.Instance.OpenModule(ModuleDef.LoginModule);
    }


}
