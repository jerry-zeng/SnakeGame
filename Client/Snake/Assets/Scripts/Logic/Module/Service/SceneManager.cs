using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.SceneManagement;
using Framework;
using Framework.UI;
using Framework.Module;

public class SceneManager : ServiceModule<SceneManager>
{
    public const string Scene_Splash = "Splash";
    public const string Scene_Update = "Update";
    public const string Scene_Login  = "Login";
    public const string Scene_Lobby  = "Lobby";
    public const string Scene_Battle = "Battle";


    private bool isInLoading = false;
    public bool IsInLoading{ get{ return isInLoading; } }


    protected override void Init()
    {
        base.Init();

        isInLoading = false;
        UnitySceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isInLoading = false;
        HideLoading();
        
        switch(scene.name)
        {
        case Scene_Splash:
            
        break;
        case Scene_Update:

        break;
        case Scene_Login:

        break;
        case Scene_Lobby:

        break;
        case Scene_Battle:

        break;
        default:
        break;
        }
    }

    public override void Release()
    {
        base.Release();

        UnitySceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void ShowLoading(Action<UIBasePanel> onUIReady = null)
    {
        UIManager.Instance.ShowUI(UIDef.UILoadingPanel, null, onUIReady);
    }

    public void HideLoading()
    {
        UIManager.Instance.HideUI(UIDef.UILoadingPanel);
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        isInLoading = true;
        UnitySceneManager.LoadScene(sceneName, mode);
    }

    public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        isInLoading = true;
        AsyncOperation operation = UnitySceneManager.LoadSceneAsync(sceneName, mode);
        return operation;
    }
}
