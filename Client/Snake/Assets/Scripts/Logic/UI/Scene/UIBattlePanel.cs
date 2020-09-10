using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework;
using Framework.UI;
using GamePlay;
using EpochProtocol;

public class UIBattlePanel : UIBasePanel 
{
    public Text lab_UserInfo;
    public Text lab_Time;
    public Button btn_Pause;
    public Button btn_Ready;
    public Text lab_Tips;

    GameContext context;
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

        if (!context.Game.EnablePause())
        {
            return;
        }

        if (context.Game.isPaused)
            return;

        context.Game.Pause();

        MessageBox.Show("Resume or Quit the game?",
                        "Quit", ()=>{
                                    MessageBox.Hide();
                                    context.Game.Terminate();
                                },
                        "Resume", ()=>{
                                    MessageBox.Hide();
                                    context.Game.Resume();
                                }
                       );
    }

    void OnClickReady()
    {
        //this.Log("OnClickReady()");
        btn_Ready.gameObject.SetActive(false);

        context.Game.OnPlayerReady();
    }


    public override void Show(object args)
    {
        base.Show(args);

        EventManager.Instance.RegisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        EventManager.Instance.RegisterEvent(EventDef.OnGameEnd, OnGameEnd);
        EventManager.Instance.RegisterEvent("OpenGodOfView", OpenGodOfView);

        context = BattleEngine.Instance.Context;

        UserData data = UserManager.Instance.UserData;
        lab_UserInfo.text = string.Format("{0}(Lv.{1})", data.userName, 0);

        timerId = Scheduler.Schedule(0.5f, 0, this.UpdateTime);
        UpdateTime();
    }

    public override void Hide()
    {
        context = null;

        EventManager.Instance.UnregisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        EventManager.Instance.UnregisterEvent(EventDef.OnGameEnd, OnGameEnd);
        EventManager.Instance.UnregisterEvent("OpenGodOfView", OpenGodOfView);

        if (timerId > 0)
        {
            Scheduler.Unschedule(timerId);
            timerId = 0;
        }

        base.Hide();
    }


    void OpenGodOfView()
    {
        lab_Tips.gameObject.SetActive(true);
        BattleView.Instance.FocusOnAnotherPlayer();
    }

    void OnGameEnd()
    {
        lab_Tips.gameObject.SetActive(false);
        MessageBox.Show("Show scores...",
                    "OK", ()=>{
                            MessageBox.Hide();
                            EventManager.Instance.SendEvent(EventDef.OnLeaveBattle);
                        }
                    );
    }

    void OnSnakerDead(Snaker snaker, object killer)
    {
        if (context.Game.EnableRevive())
        {
            MessageBox.Show("Revive or Quit this game?",
                        "Quit", ()=>{
                            MessageBox.Hide();
                            context.Game.Terminate();
                        },
                        "Revive", ()=>{
                            MessageBox.Hide();
                            context.Game.Resume();
                            context.Game.RebornPlayer();
                        });
        }
    }


    void UpdateTime()
    {
        if (context == null)
        {
            lab_Time.text = "";
            return;
        }

        int time;
        if (context.IsTimelimited) 
        {
            time = context.GetRemainTime(); //second
        } 
        else 
        {
            time = context.GetElapsedTime();
        }
        lab_Time.text = TimeUtils.GetTimeString("%hh:%mm:%ss", time);
    }
}
