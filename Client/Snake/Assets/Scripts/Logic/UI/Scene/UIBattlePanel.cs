using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;
using EpochProtocol;

public class UIBattlePanel : UIBasePanel 
{
    public Text lab_UserInfo;
    public Text lab_Time;
    public Button btn_Pause;
    public Button btn_Ready;

    PVEModule pveModule;
    uint timerId = 0;

    public override void Setup()
    {
        base.Setup();

        btn_Pause.onClick.AddListener(OnClickPause);
        btn_Ready.onClick.AddListener(OnClickReady);
    }

    public override void Release()
    {
        btn_Pause.onClick.RemoveAllListeners();
        btn_Ready.onClick.RemoveAllListeners();

        base.Release();
    }


    void OnClickPause()
    {
        //this.Log("OnClickPause()");

        PVEGame pveGame = pveModule.currentGame;
        if (pveGame.isPaused)
            return;

        pveGame.Pause();

        MessageBox.Show("Resume or Quit the game?",
                        "Quit", ()=>{
                                    MessageBox.Hide();
                                    pveGame.Terminate();
                                },
                        "Resume", ()=>{
                                    MessageBox.Hide();
                                    pveGame.Resume();
                                }
                       );
    }

    void OnClickReady()
    {
        //this.Log("OnClickReady()");
        btn_Ready.gameObject.SetActive(false);

        PVEGame pveGame = pveModule.currentGame;
        pveGame.OnPlayerReady();
    }


    public override void Show(object args)
    {
        base.Show(args);

        EventManager.Instance.RegisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        EventManager.Instance.RegisterEvent(EventDef.OnGameEnd, OnGameEnd);

        pveModule = ModuleManager.Instance.EnsureModule<PVEModule>();

        UserData data = UserManager.Instance.UserData;
        lab_UserInfo.text = string.Format("{0}(Lv.{1})", data.userName, 0);

        timerId = Scheduler.Schedule(0.5f, 0, this.UpdateTime);
        UpdateTime();
    }

    public override void Hide()
    {
        pveModule = null;

        EventManager.Instance.UnregisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        EventManager.Instance.UnregisterEvent(EventDef.OnGameEnd, OnGameEnd);

        if (timerId > 0)
        {
            Scheduler.Unschedule(timerId);
            timerId = 0;
        }

        base.Hide();
    }

    void OnGameEnd()
    {
        MessageBox.Show("Show scores...",
                    "OK", ()=>{
                            MessageBox.Hide();
                            EventManager.Instance.SendEvent(EventDef.OnLeaveBattle);
                        }
                    );
    }

    void OnSnakerDead(Snaker snaker, object killer)
    {
        PVEGame pveGame = pveModule.currentGame;
        pveGame.OnPlayerDie(snaker.PlayerID);

        if (pveGame.isPaused)
        {
            MessageBox.Show("Revive or Quit this game?",
                        "Quit", ()=>{
                            MessageBox.Hide();
                            pveGame.Terminate();
                        },
                        "Revive", ()=>{
                            MessageBox.Hide();
                            pveGame.Resume();
                            pveGame.RebornPlayer();
                        });
        }
    }


    void UpdateTime()
    {
        if (pveModule == null)
        {
            lab_Time.text = "";
            return;
        }

        PVEGame pveGame = pveModule.currentGame;
        int time;
        if (pveGame.IsTimelimited) 
        {
            time = pveGame.GetRemainTime(); //second
        } 
        else 
        {
            time = pveGame.GetElapsedTime();
        }
        lab_Time.text = TimeUtils.GetTimeString("%hh:%mm:%ss", time);
    }
}
