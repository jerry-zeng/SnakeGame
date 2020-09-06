using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UIUpdatePanel : UIBasePanel 
{
    public Text lab_des;
    public Transform progressBar;

    PatchManager.PatchState curState;
    float progress;
    bool hasUpdateEnd = false;
    float endDelay = 0.5f;

    public override void Show(object args)
    {
        base.Show(args);

        SetDescription("checking update...");
        SetProgress(0f);
        hasUpdateEnd = false;
    }

    void SetDescription(string des)
    {
        lab_des.text = des;
    }

    void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);

        Vector3 scale = progressBar.localScale;
        scale.x = value;
        progressBar.localScale = scale;
    }

    void Update()
    {
        PatchManager.Instance.Update();

        curState = PatchManager.Instance.CurrentState;
        progress = PatchManager.Instance.PatchProgress;

        if( curState == PatchManager.PatchState.CheckUpdate ){
            SetDescription("checking update...");
        }
        else if( curState == PatchManager.PatchState.Update ){
            SetDescription(string.Format("doing update...({0}%)", (int)(progress*100)));
            SetProgress(progress);
        }
        else if( curState == PatchManager.PatchState.UpdateEnd ){
            SetDescription(string.Format("doing update...({0}%)", (int)(progress*100)));
            SetProgress(progress);

            if( !hasUpdateEnd ){
                Invoke("SendUpdateEndEvent", endDelay);
                hasUpdateEnd = true;
            }
        }
    }

    void SendUpdateEndEvent()
    {
        EventManager.Instance.SendEvent(EventDef.OnUpdateEnd);
    }
}
