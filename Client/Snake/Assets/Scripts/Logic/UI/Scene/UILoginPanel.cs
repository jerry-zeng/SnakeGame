using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework;
using Framework.UI;
using Framework.Module;
using EpochProtocol;

public class UILoginPanel : UIBasePanel 
{
    public InputField input_userName;
    public InputField input_password;
    public Button btn_Login;

    bool isUseNameValid = false;
    bool isPasswordValid = false;
    Color btnNormalColor;

    public override void Setup()
    {
        base.Setup();

        btn_Login.onClick.AddListener( OnClickLogin );
        btnNormalColor = btn_Login.GetComponent<Image>().color;

        input_userName.onEndEdit.AddListener( ValidateInputUserName );
        input_password.onEndEdit.AddListener( ValidateInputPassword );

        OnValidationChanged();
    }

    void ValidateInputUserName(string value)
    {
        if( string.IsNullOrEmpty(value) ){
            isUseNameValid = false;
            this.LogWarning("UserName can't be empty.");
        }
        else if( value.IndexOf(" ") >= 0 ){
            isUseNameValid = false;
            this.LogWarning("UserName can't contain space.");
        }
        else{
            isUseNameValid = true;
        }

        OnValidationChanged();
    }

    void ValidateInputPassword(string value)
    {
        if( string.IsNullOrEmpty(value) ){
            isPasswordValid = false;
            this.LogWarning("Password can't be empty.");
        }
        else if( value.IndexOf(" ") >= 0 ){
            isPasswordValid = false;
            this.LogWarning("Password can't contain space.");
        }
        else{
            isPasswordValid = true;
        }

        OnValidationChanged();
    }

    void OnValidationChanged()
    {
        if( isUseNameValid && isPasswordValid )
            btn_Login.GetComponent<Image>().color = btnNormalColor;
        else
            btn_Login.GetComponent<Image>().color = Color.gray;
    }

    public void SetUserData(uint id, string userName, string password)
    {
        input_userName.text = userName;
        input_password.text = password;

        ValidateInputUserName(input_userName.text);
        ValidateInputPassword(input_password.text);
    }

    void OnClickLogin()
    {
        if( isUseNameValid && isPasswordValid )
        {
            LoginModule loginModule = ModuleManager.Instance.EnsureModule<LoginModule>();
            loginModule.Login( (uint)0, input_userName.text, input_password.text );
        }
    }

    public override void Show(object args)
    {
        base.Show(args);

        UserData data = UserManager.Instance.UserData;
        SetUserData(data.id, data.userName, data.password);
    }
}
