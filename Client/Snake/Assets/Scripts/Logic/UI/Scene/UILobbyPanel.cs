using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;
using GameProtocol;

public class UILobbyPanel : UIBasePanel 
{
    public Text lab_UserInfo;
    public Button btn_UserIcon;
    public Button btn_Setting;
    public Button btn_Quest;
    public Button btn_Mail;
    public Button btn_Ranking;

    public Button btn_PVE;
    public Button btn_PVP;

    LobbyModule lobbyModule;

    public override void Setup()
    {
        base.Setup();

        lobbyModule = ModuleManager.Instance.EnsureModule<LobbyModule>();

        btn_UserIcon.onClick.AddListener(OnClickUseIcon);

        btn_PVE.onClick.AddListener(OnClickPVE);
        btn_PVP.onClick.AddListener(OnClickPVP);

        btn_Setting.onClick.AddListener(OnClickSetting);
        btn_Quest.onClick.AddListener(OnClickQuest);
        btn_Mail.onClick.AddListener(OnClickMail);
        btn_Ranking.onClick.AddListener(OnClickRanking);
    }

    void OnClickUseIcon()
    {
        /*
        MessageBox.Show("This function wasn't implemented yet.",
                       "OK",
                        MessageBox.Hide);
                        */
        Toast.Show( "This function wasn't implemented yet." );
    }

    void OnClickPVE()
    {
        lobbyModule.OpenModule(ModuleDef.PVEModule, GameMode.EndlessPVE);
    }

    void OnClickPVP()
    {
        lobbyModule.OpenModule(ModuleDef.PVPModule, GameMode.EndlessPVP);
    }

    void OnClickSetting()
    {
        lobbyModule.OpenModule(ModuleDef.SettingModule);
    }

    void OnClickQuest()
    {
        lobbyModule.OpenModule(ModuleDef.QuestModule);
    }

    void OnClickMail()
    {
        lobbyModule.OpenModule(ModuleDef.MailModule);
    }

    void OnClickRanking()
    {
        lobbyModule.OpenModule(ModuleDef.RankingModule);
    }

    public override void Show(object args)
    {
        base.Show(args);

        UserData data = UserManager.Instance.UserData;
        lab_UserInfo.text = string.Format("{0}(Lv.{1})", data.userName, 0);
    }
}
