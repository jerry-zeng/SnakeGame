using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;

public class LoginScene : MonoBehaviour 
{
    static string[] loadDataTables = 
    { 
        "Config",
        "Map",
        "Food",
        "Snaker",
        "SnakerNode",
        "Stage"
    };

    UILoadingPanel loadingPanel;
    int curLoadIndex = 0;


    void Awake()
    {
        ModuleManager.Instance.EnsureModule(ModuleDef.LoginModule);
    }

    void Start ()
    {
        ModuleManager.Instance.GetModule(ModuleDef.LoginModule).Open();
        UIManager.Instance.ShowUI( UIDef.UILoginPanel );
        EventManager.Instance.RegisterEvent(EventDef.OnLogin, OnLogin);
    }

    void OnLogin()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        SceneManager.Instance.ShowLoading((ui)=>
        {
            loadingPanel = ui as UILoadingPanel;
            curLoadIndex = 0;
            StartCoroutine(LoadingDataTables());
        });

    }

    void OnDestroy()
    {
        loadingPanel = null;
        EventManager.Instance.UnregisterEvent(EventDef.OnLogin, OnLogin);
    }

    void GotoLobbyScene()
    {
        SceneManager.Instance.LoadScene( SceneManager.Scene_Lobby );
    }

    void Update()
    {
        if( loadingPanel != null && loadDataTables.Length > 0 )
        {
            loadingPanel.SetProgress( curLoadIndex, loadDataTables.Length );
        }
    }

    IEnumerator LoadingDataTables()
    {
        for( int i = 0; i < loadDataTables.Length; i++ )
        {
            CSVTableLoader.LoadDataTable( loadDataTables[i] );
            curLoadIndex = i;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        GotoLobbyScene();
    }
}
