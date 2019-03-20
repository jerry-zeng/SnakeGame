using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class RankingModule : BusinessModule
{
    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UIRankingPanel );
    }

    public override void Close()
    {
        UIManager.Instance.HideUI( UIDef.UIRankingPanel );

        base.Close();
    }
}
