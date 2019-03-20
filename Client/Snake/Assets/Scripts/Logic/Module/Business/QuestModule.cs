using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.UI;
using Framework.Module;

public class QuestModule : BusinessModule 
{
    private List<string> questDataList;


    public override void Create(object arg)
    {
        base.Create(arg);

        questDataList = new List<string>();
        for(int i = 0; i < 20; i++)
        {
            questDataList.Add( "Quest " + i.ToString() );
        }
    }

    public override void Open(object arg)
    {
        base.Open(arg);

        UIManager.Instance.ShowUI( UIDef.UIQuestPanel );
        UIManager.Instance.SetFullScreenMaskClickListener( Close );
    }

    public override void Close()
    {
        UIManager.Instance.HideUI( UIDef.UIQuestPanel );

        base.Close();
    }

    public override void Release()
    {
        questDataList.Clear();
        questDataList = null;

        base.Release();
    }

    public IList GetQuestList()
    {
        return questDataList;
    }
}
