using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.UI;

public class Toast 
{
    public const float DURATION_SHORT = 1f;
    public const float DURATION_NORMAL = 3f;
    public const float DURATION_LENGTH = 5f;

    public const string KEY_MESSAGE = "Message";
    public const string KEY_DURATION = "Duration";

    /// <summary>
    /// Show the specified message in duration(seconds), the duration range is [DURATION_SHORT, DURATION_LENGTH].
    /// </summary>
    /// <param name="msg">Message.</param>
    /// <param name="duration">Duration.</param>
    public static void Show(string msg, float duration = DURATION_NORMAL)
    {
        if( string.IsNullOrEmpty(msg) )
            return;

        duration = Mathf.Clamp(duration, DURATION_SHORT, DURATION_LENGTH);

        Dictionary<string, object> param = new Dictionary<string, object>();
        param[Toast.KEY_MESSAGE] = msg;
        param[Toast.KEY_DURATION] = duration;

        Show_Internel(param);
    }

    static void Show_Internel(Dictionary<string, object> paramDict)
    {
        UIManager.Instance.ShowUI( UIDef.UIToast, paramDict );
    }

    public static void Hide()
    {
        UIManager.Instance.HideUI( UIDef.UIToast );
    }
}
