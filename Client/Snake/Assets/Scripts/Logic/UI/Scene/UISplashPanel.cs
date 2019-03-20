using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UISplashPanel : UIBasePanel 
{
    public Text logo;
    public float AnimInterval = 3f;
    float elapseTime = 0f;


    public override void Show(object args)
    {
        base.Show(args);

        elapseTime = 0f;
    }

    void Update()
    {
        elapseTime += Time.deltaTime;

        if( elapseTime >= AnimInterval ){
            elapseTime = 0f;
            EventManager.Instance.SendEvent(EventDef.OnSplashEnd);
        }
    }
}
