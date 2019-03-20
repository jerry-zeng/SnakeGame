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


    }

    public void OpenModule(string moduleName, object arg = null)
    {
        switch(moduleName)
        {
        case ModuleDef.SettingModule:
            UIManager.Instance.ShowUI( UIDef.UISettingPanel );
            UIManager.Instance.SetFullScreenMaskClickListener( ()=>
            {
                UIManager.Instance.HideUI( UIDef.UISettingPanel );
            } );
        break;

        case ModuleDef.QuestModule:
        case ModuleDef.RankingModule:
        case ModuleDef.MailModule:
        case ModuleDef.PVPModule:
        case ModuleDef.PVEModule:
            ModuleManager.Instance.OpenModule(moduleName, arg);
        break;

        default:
        break;
        }
    }
}
