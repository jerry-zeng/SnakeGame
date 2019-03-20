using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.UI;

public class UILuaPanel : UIBasePanel 
{
    public string luaPath;


    public override void Setup()
    {
        base.Setup();

        LoadLua();
        CallLuaFunction_Void("Setup", CachedGameObject);
    }

    public override void Show(object args)
    {
        base.Show(args);

        CallLuaFunction_Void("Show", args);
    }

    public override void Hide()
    {
        CallLuaFunction_Void("Hide");

        base.Hide();
    }

    // lua must release the reference of unity objects
    public override void Release()
    {
        CallLuaFunction_Void("Release");
        ReleaseLua();

        base.Release();
    }

    protected void LoadLua()
    {
        
    }

    protected void ReleaseLua()
    {
        
    }

    protected void CallLuaFunction_Void(string funcName, params object[] args)
    {
        
    }
}
