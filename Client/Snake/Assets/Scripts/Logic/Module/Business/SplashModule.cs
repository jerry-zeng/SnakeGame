using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;

public class SplashModule : BusinessModule 
{

    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UISplashPanel );

        EventManager.Instance.RegisterEvent(EventDef.OnSplashEnd, OnSplashEnd);
    }

    public override void Close()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnSplashEnd, OnSplashEnd);
    }

    void OnSplashEnd()
    {
        ModuleManager.Instance.OpenModule(ModuleDef.UpdateModule);
    }
}
