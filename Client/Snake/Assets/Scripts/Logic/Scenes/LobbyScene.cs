using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;
using Framework.Module;

public class LobbyScene : MonoBehaviour 
{

    void Awake()
    {
        ModuleManager.Instance.EnsureModule(ModuleDef.LobbyModule);
        ModuleManager.Instance.EnsureModule(ModuleDef.QuestModule);
        ModuleManager.Instance.EnsureModule(ModuleDef.RankingModule);
        ModuleManager.Instance.EnsureModule(ModuleDef.MailModule);
        ModuleManager.Instance.EnsureModule(ModuleDef.PVPModule);
        ModuleManager.Instance.EnsureModule(ModuleDef.PVEModule);
    }

    void Start ()
    {
        ModuleManager.Instance.GetModule(ModuleDef.LobbyModule).Open();

        UIManager.Instance.ShowUI( UIDef.UILobbyPanel );
        EventManager.Instance.RegisterEvent(EventDef.OnEnterBattle, OnEnterBattle);
    }

    void OnEnterBattle()
    {
        UIManager.Instance.ReleaseAllUIOfCurrentUnityScene();
        SceneManager.Instance.ShowLoading();
        SceneManager.Instance.LoadScene( SceneManager.Scene_Battle );
    }

    void OnDestroy()
    {
        EventManager.Instance.UnregisterEvent(EventDef.OnEnterBattle, OnEnterBattle);
    }

}
