using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.UI;
using Framework;

public class UIToast : UIBasePanel 
{
    public Image bg_msg;
    public Text lab_msg;

    const float fadeDuration = 0.5f;
    float duration = 1f;

    bool isInFadeAnim = false;
    bool isTerminated = false;
    float elapsedTime = 0f;


    void ResetValues()
    {
        isTerminated = false;
        isInFadeAnim = false;
        elapsedTime = 0f;
    }


    public override void Show(object args)
    {
        base.Show(args);

        ResetValues();

        Dictionary<string, object> showParamDict = args as Dictionary<string, object>;
        if( showParamDict != null )
        {
            if( showParamDict.ContainsKey(Toast.KEY_MESSAGE) )
                lab_msg.text = (string)showParamDict[Toast.KEY_MESSAGE];
            else
                lab_msg.text = "";

            if( showParamDict.ContainsKey(Toast.KEY_DURATION) )
                duration = (float)showParamDict[Toast.KEY_DURATION];
            else
                duration = 1f;
        }
        else
        {
            this.LogError("No parameters on showing toast!!!");
            lab_msg.text = "";
            duration = 1f;
        }
    }

    public override void Hide()
    {
        lab_msg.text = "";
        isTerminated = true;

        base.Hide();
    }


    void Update()
    {
        if( isTerminated ) return;

        elapsedTime += Time.deltaTime;

        if( elapsedTime >= duration )
        {
            if( !isInFadeAnim ){
                isInFadeAnim = true;
                bg_msg.CrossFadeAlpha(0f, fadeDuration, false);
                elapsedTime = 0f;
            }
        }

        if( isInFadeAnim && elapsedTime >= fadeDuration ){
            isTerminated = true;
            Toast.Hide();
        }
    }
}
