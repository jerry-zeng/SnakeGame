using System.Collections;
using System.Collections.Generic;
using Framework.Module;


public class GameSettings : ServiceModule<GameSettings> 
{
    private Dictionary<string, object> _configDict = new Dictionary<string, object>();

#region Getter & Setter
    public static string GetString(string key, string defaultValue = "")
    {
        if( _instance != null && _instance._configDict.ContainsKey(key) )
        {
            return (string)_instance._configDict[key];
        }
        return defaultValue;
    }
    public static bool SetString(string key, string value)
    {
        if( _instance != null )
        {
            _instance._configDict[key] = value;
            return true;
        }
        return false;
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        if( _instance != null && _instance._configDict.ContainsKey(key) )
        {
            return (bool)_instance._configDict[key];
        }
        return defaultValue;
    }
    public static bool SetBool(string key, bool value)
    {
        if( _instance != null )
        {
            _instance._configDict[key] = value;
            return true;
        }
        return false;
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        if( _instance != null && _instance._configDict.ContainsKey(key) )
        {
            return (int)_instance._configDict[key];
        }
        return defaultValue;
    }
    public static bool SetInt(string key, int value)
    {
        if( _instance != null )
        {
            _instance._configDict[key] = value;
            return true;
        }
        return false;
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if( _instance != null && _instance._configDict.ContainsKey(key) )
        {
            return (float)_instance._configDict[key];
        }
        return defaultValue;
    }
    public static bool SetFloat(string key, float value)
    {
        if( _instance != null )
        {
            _instance._configDict[key] = value;
            return true;
        }
        return false;
    }
#endregion

    protected override void Init()
    {
        base.Init();

        _configDict.Clear();

        LoadGameSettings();
        LoadNetworkSettings();
    }


    void LoadGameSettings()
    {
        
    }

    void LoadNetworkSettings()
    {
        
    }
}
