using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UIMainDetailPanel : UIBasePanel 
{
    public Text lab_title;
    public Text lab_msg;
    public Button btn_Receive;


    public override void Setup()
    {
        base.Setup();

        btn_Receive.onClick.AddListener(OnClickReceive);
    }

    public override void Release()
    {
        btn_Receive.onClick.RemoveAllListeners();

        base.Release();
    }


    void OnClickReceive()
    {
        UIManager.Instance.PopUI();
    }

    public override void Show(object args)
    {
        base.Show(args);


    }

    public override void Hide()
    {


        base.Hide();
    }
}
