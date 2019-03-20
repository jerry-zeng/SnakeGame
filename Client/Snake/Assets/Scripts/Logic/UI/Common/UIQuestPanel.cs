using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;

public class UIQuestPanel : UIBasePanel 
{
    public Button btn_Close;
    public UIListView listView;

    QuestModule questModule = null;


    public override void Setup()
    {
        base.Setup();

        btn_Close.onClick.AddListener(OnClickClose);
    }

    void OnClickClose()
    {
        questModule.Close();
    }

    public override void Show(object args)
    {
        base.Show(args);

        // load quest list
        questModule = ModuleManager.Instance.GetModule<QuestModule>();

        listView.SetData( questModule.GetQuestList() );
    }

    public override void Hide()
    {
        // clear all items
        questModule = null;
        listView.Clear();

        base.Hide();
    }

}
