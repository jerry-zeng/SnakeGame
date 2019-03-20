using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.Module;
using EpochProtocol;

public class UserManager : ServiceModule<UserManager> 
{
    private UserData _userData;
    public UserData UserData{ get{ return _userData; } }

    private int _snakerID = 101;
    public int CurrentSnakerID
    {
        get{ return _snakerID; }
    }

    protected override void Init()
    {
        base.Init();

        LoadSavedUserData();
    }

    public void UpdateLoginUser(UserData data)
    {
        if( data == null )
        {
            this.LogError("A null user logined?");
        }
        else
        {
            _userData = data;

            SaveUserData();
        }
    }

    private void LoadSavedUserData()
    {
        if( PlayerPrefs.HasKey("UserData.id") )
        {
            _userData = new UserData();
            _userData.id = (uint)PlayerPrefs.GetInt("UserData.id");
            _userData.userName = PlayerPrefs.GetString("UserData.userName");
            _userData.password = PlayerPrefs.GetString("UserData.password");
        }
    }

    private void SaveUserData()
    {
        PlayerPrefs.SetInt("UserData.id", (int)_userData.id);
        PlayerPrefs.SetString("UserData.userName", _userData.userName);
        PlayerPrefs.SetString("UserData.password", _userData.password);
        PlayerPrefs.Save();
    }
}
