using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Module;

public class PatchManager : ServiceModule<PatchManager> 
{
    public enum PatchState
    {
        CheckUpdate = 0,
        Update,
        UpdateEnd
    }

    public PatchState CurrentState{ get; private set;}
    public float PatchProgress{ get; private set;}


    const float DelayTime = 1f;
    const float UpdateTotalTime = 3f;
    float elapseTime = 0f;


    protected override void Init()
    {
        base.Init();

        Start();
    }

    public void Start()
    {
        CurrentState = PatchState.CheckUpdate;
        PatchProgress = 0f;
        elapseTime = 0f;
    }

    public void Update()
    {
        if( CurrentState == PatchState.UpdateEnd ) 
            return;

        elapseTime += Time.deltaTime;

        if( elapseTime < DelayTime ){
            CurrentState = PatchState.CheckUpdate;
        }
        else if( elapseTime <= UpdateTotalTime + DelayTime ){
            CurrentState = PatchState.Update;
            PatchProgress = (elapseTime - DelayTime) / UpdateTotalTime;

            if( elapseTime >= UpdateTotalTime + DelayTime ){
                PatchProgress = 1f;
                CurrentState = PatchState.UpdateEnd;
            }
        }
        else{
            PatchProgress = 1f;
            CurrentState = PatchState.UpdateEnd;
        }
    }
}
