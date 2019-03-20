using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;

public class UIMessageBox : UIBasePanel 
{
    public Button btn_Left;
    public Text tag_Left;
    public Button btn_Middle;
    public Text tag_Middle;
    public Button btn_Right;
    public Text tag_Right;
    public Text lab_title;
    public Text lab_msg;


    public override void Show(object args)
    {
        base.Show(args);

        Dictionary<string, object> showParamDict = args as Dictionary<string, object>;
        if( showParamDict != null )
        {
            if( showParamDict.ContainsKey(MessageBox.KEY_TITLE) )
                lab_title.text = (string)showParamDict[MessageBox.KEY_TITLE];
            else
                lab_title.text = "";

            if( showParamDict.ContainsKey(MessageBox.KEY_MESSAGE) )
                lab_msg.text = (string)showParamDict[MessageBox.KEY_MESSAGE];
            else
                lab_msg.text = "";

            btn_Left.gameObject.SetActive(false);
            btn_Middle.gameObject.SetActive(false);
            btn_Right.gameObject.SetActive(false);

            if( showParamDict.ContainsKey(MessageBox.KEY_MIDDLE_HANDLER) ){
                btn_Middle.gameObject.SetActive(true);

                tag_Middle.text = (string)showParamDict[MessageBox.KEY_MIDDLE_BUTTON];
                btn_Middle.onClick.AddListener((UnityAction)showParamDict[MessageBox.KEY_MIDDLE_HANDLER]);
            }
            else{
                btn_Left.gameObject.SetActive(true);
                btn_Right.gameObject.SetActive(true);

                tag_Left.text = (string)showParamDict[MessageBox.KEY_LEFT_BUTTON];
                btn_Left.onClick.AddListener((UnityAction)showParamDict[MessageBox.KEY_LEFT_HANDLER]);
                tag_Right.text = (string)showParamDict[MessageBox.KEY_RIGHT_BUTTON];
                btn_Right.onClick.AddListener((UnityAction)showParamDict[MessageBox.KEY_RIGHT_HANDLER]);
            }
        }
        else
        {
            this.LogError("Null show parameters!!!");
        }
    }

    public override void Hide()
    {
        btn_Left.onClick.RemoveAllListeners();
        btn_Middle.onClick.RemoveAllListeners();
        btn_Right.onClick.RemoveAllListeners();

        base.Hide();
    }

}
