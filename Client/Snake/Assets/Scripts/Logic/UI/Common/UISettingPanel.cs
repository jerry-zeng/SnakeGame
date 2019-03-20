using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;
using GameProtocol;

public class UISettingPanel : UIBasePanel 
{
    public Button btn_Close;
    public Button btn_SwitchBGM;
    public Button btn_SwitchSE;

    public Text tag_SwitchBGM;
    public Text tag_SwitchSE;


    public override void Setup()
    {
        base.Setup();

        btn_Close.onClick.AddListener(OnClickClose);
        btn_SwitchBGM.onClick.AddListener(OnClickSwitchBGM);
        btn_SwitchSE.onClick.AddListener(OnClickSwitchSE);
    }

    void OnClickClose()
    {
        UIManager.Instance.HideUI(CachedGameObject.name);
    }

    void OnClickSwitchBGM()
    {
        
    }

    void OnClickSwitchSE()
    {

    }

    public override void Show(object args)
    {
        base.Show(args);


    }


}
