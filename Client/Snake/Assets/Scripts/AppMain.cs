using UnityEngine;
using Framework;
using Framework.Module;


public class AppMain : MonoBehaviour
{
    public bool enableLog = true;
    public bool enableLogSave = true;


	void Start () 
    {
        Debuger.EnableLog = enableLog;
        Debuger.EnableSave = enableLogSave;
		Debuger.Log(Debuger.LogFileDir);

        AppConfig.Init();

	    InitServices();

        // 这一行崩溃？
        ModuleManager.Instance.OpenModule(ModuleDef.SplashModule);
    }

    void InitServices()
    {
        ModuleManager.CreateInstance();
        GameSettings.CreateInstance();

        Scheduler.CreateInstance().MakePersistence();
    }

}
