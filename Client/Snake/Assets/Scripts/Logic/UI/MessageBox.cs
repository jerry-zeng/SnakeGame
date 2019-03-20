using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Framework.UI;


public class MessageBox 
{
    public const string KEY_LEFT_BUTTON = "LeftButton";
    public const string KEY_MIDDLE_BUTTON = "MiddleButton";
    public const string KEY_RIGHT_BUTTON = "RightButton";
    public const string KEY_LEFT_HANDLER = "LeftHandler";
    public const string KEY_MIDDLE_HANDLER = "MiddleHandler";
    public const string KEY_RIGHT_HANDLER = "RightHandler";
    public const string KEY_TITLE = "Title";
    public const string KEY_MESSAGE = "Message";


    public static void Show(string msg, string leftButton, UnityAction leftHandler, string rightButton, UnityAction rightHandler, string title = "")
    {
        Dictionary<string, object> paramDict = new Dictionary<string, object>();
        paramDict[MessageBox.KEY_MESSAGE] = msg;
        paramDict[MessageBox.KEY_LEFT_BUTTON] = leftButton;
        paramDict[MessageBox.KEY_LEFT_HANDLER] = leftHandler;
        paramDict[MessageBox.KEY_RIGHT_BUTTON] = rightButton;
        paramDict[MessageBox.KEY_RIGHT_HANDLER] = rightHandler;
        paramDict[MessageBox.KEY_TITLE] = title;
        Show_Internel( paramDict );
    }

    public static void Show(string msg, string middleButton, UnityAction middleHandler, string title = "")
    {
        Dictionary<string, object> paramDict = new Dictionary<string, object>();
        paramDict[MessageBox.KEY_MESSAGE] = msg;
        paramDict[MessageBox.KEY_MIDDLE_BUTTON] = middleButton;
        paramDict[MessageBox.KEY_MIDDLE_HANDLER] = middleHandler;
        paramDict[MessageBox.KEY_TITLE] = title;
        Show_Internel( paramDict );
    }

    static void Show_Internel(Dictionary<string, object> paramDict)
    {
        UIManager.Instance.ShowUI( UIDef.UIMessageBox, paramDict );
    }

    public static void Hide()
    {
        UIManager.Instance.HideUI( UIDef.UIMessageBox );
    }
}
