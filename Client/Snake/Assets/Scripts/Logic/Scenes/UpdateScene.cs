using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;

public class UpdateScene : MonoBehaviour 
{

    void Start ()
    {
        UIManager.Instance.ShowUI( UIDef.UIUpdatePanel );
        EventManager.Instance.RegisterEvent(EventDef.OnUpdateEnd, OnUpdateEnd);
        PatchManager.Instance.Start();
    }

    void OnUpdateEnd()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        SceneManager.Instance.ShowLoading();
        SceneManager.Instance.LoadScene( SceneManager.Scene_Login );
    }

    void OnDestroy()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnUpdateEnd, OnUpdateEnd);
    }

    void Update()
    {
        if(PatchManager.Instance != null)
            PatchManager.Instance.Update();
    }
}
