using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;

public class UIMailPanel : UIBasePanel 
{
    public Button btn_Close;

    public UIListView listView;


    public override void Setup()
    {
        base.Setup();

        btn_Close.onClick.AddListener(OnClickClose);
    }

    public override void Release()
    {
        btn_Close.onClick.RemoveAllListeners();

        base.Release();
    }


    void OnClickClose()
    {
        UIManager.Instance.PopUI();
    }


    public override void Show(object args)
    {
        base.Show(args);


    }

    public override void Hide()
    {
        listView.Clear();

        base.Hide();
    }
}
