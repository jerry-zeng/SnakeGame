using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class MailModule : BusinessModule 
{

    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UIMailPanel );
        UIManager.Instance.SetFullScreenMaskClickListener( Close );
    }

    public override void Close()
    {
        UIManager.Instance.HideUI( UIDef.UIMailPanel );

        base.Close();
    }
}
