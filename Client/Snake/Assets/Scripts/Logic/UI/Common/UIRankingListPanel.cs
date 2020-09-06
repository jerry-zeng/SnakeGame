using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;

public class UIRankingListPanel : UIBasePanel 
{
    public Button btn_Back;

    public UIListView listView;
    public UIRankingItem myRankingItem;


    public override void Setup()
    {
        base.Setup();

        btn_Back.onClick.AddListener(OnClickBack);
    }

    public override void Release()
    {
        btn_Back.onClick.RemoveAllListeners();

        base.Release();
    }


    void OnClickBack()
    {
        UIManager.Instance.PopUI();
    }

    public override void Show(object args)
    {
        base.Show(args);

        // get ranking data
        myRankingItem.Clear();
    }

    public override void Hide()
    {
        listView.Clear();

        base.Hide();
    }
}
