using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using GamePlay;

public class UIBattlePanel : UIBasePanel 
{
    public Text lab_UserInfo;
    public Text lab_Time;
    public Button btn_Pause;
    public Button btn_Ready;

    public override void Setup()
    {
        base.Setup();

        btn_Pause.onClick.AddListener(OnClickPause);
        btn_Ready.onClick.AddListener(OnClickReady);
    }

    void OnClickPause()
    {
        if( !BattleEngine.Instance.IsRunning )
            return;

        //this.Log("OnClickPause()");
        BattleView.Instance.PauseGame();
        MessageBox.Show("Resume or Quit the game?",
                        "Quit", ()=>{
                                    BattleEngine.Instance.EndBattle();
                                    MessageBox.Hide();
                                    EventManager.Instance.SendEvent(EventDef.OnLeaveBattle);
                                },
                        "Resume", ()=>{
                                    BattleView.Instance.ResumeGame();
                                    MessageBox.Hide();
                                    }
                       );
    }

    void OnClickReady()
    {
        //this.Log("OnClickReady()");
        BattleView.Instance.OnPlayerReady();
        btn_Ready.gameObject.SetActive(false);
    }

    public override void Show(object args)
    {
        base.Show(args);

        lab_UserInfo.text = "";
        lab_Time.text = "00:00";
    }


}
