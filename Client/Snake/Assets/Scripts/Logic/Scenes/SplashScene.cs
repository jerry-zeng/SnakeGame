using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;
using Framework.Module;
using GamePlay;

public class SplashScene : MonoBehaviour 
{
    public bool enableLog = true;

    public static bool HasSetup = false;

    void Awake()
    {
        if( !HasSetup ){
            InitServiceModules();
            InitBusinessModules();
            InitOthers();
            HasSetup = true;
        }
    }

    void InitServiceModules()
    {
        ModuleManager.CreateInstance();
        SceneManager.CreateInstance();
        UIManager.CreateInstance();
        EventManager.CreateInstance();
        PatchManager.CreateInstance();
        NetworkManager.CreateInstance();
        UserManager.CreateInstance();
        GameSettings.CreateInstance();

        BattleEngine.CreateInstance();
    }

    void InitBusinessModules()
    {

    }

    void InitOthers()
    {
        if( Scheduler.Instance == null )
            Scheduler.CreateInstance().MakePersistence();
    }

    void Start ()
    {
        Debuger.EnableLog = enableLog;

        UIManager.Instance.ShowUI( UIDef.UISplashPanel );
        EventManager.Instance.RegisterEvent(EventDef.OnSplashEnd, OnSplashEnd);
    }

    void OnSplashEnd()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        SceneManager.Instance.LoadScene( SceneManager.Scene_Update );
    }

    void OnDestroy()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnSplashEnd, OnSplashEnd);
    }
}
