using Framework;
using Framework.UI;
using Framework.Module;

public class SettingModule : BusinessModule
{
    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UISettingPanel );
        UIManager.Instance.SetFullScreenMaskClickListener( Close );
    }

    public override void Close()
    {
        UIManager.Instance.HideUI( UIDef.UISettingPanel );

        base.Close();
    }

    
}