using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UILoadingPanel : UIBasePanel 
{
    public Text lab_description;
    float progress = 0f;
    public float Progress
    {
        get{ return progress; }
    }

    public override void Show(object args)
    {
        base.Show(args);

        lab_description.text = "loading...";
    }

    public void SetProgress(int cur, int total)
    {
        progress = cur*1f / total;
        lab_description.text = string.Format("loading...({0}/{1})", cur, total);
    }

    public override void Hide()
    {
        progress = 0f;
        lab_description.text = "";
        base.Hide();
    }
}
