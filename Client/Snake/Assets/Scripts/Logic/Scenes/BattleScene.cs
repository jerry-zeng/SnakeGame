using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;

public class BattleScene : MonoBehaviour 
{
    
    void Awake()
    {
        
    }

    void Start ()
    {
        UIManager.Instance.ShowUI( UIDef.UIBattlePanel );
        EventManager.Instance.RegisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);
    }

    void OnLeaveBattle()
    {
        // TODO: clear battle objects...

        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        SceneManager.Instance.ShowLoading();
        SceneManager.Instance.LoadScene( SceneManager.Scene_Lobby );
    }

    void OnDestroy()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnLeaveBattle, OnLeaveBattle);
    }

}
