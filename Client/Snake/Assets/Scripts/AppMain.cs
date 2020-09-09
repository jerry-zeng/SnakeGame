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
        Debuger.Init();

        Debug.Log(Debuger.LogFileDir);

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

    // TODO：监听键盘事件，应该再弄个不销毁的脚本来做，懒得搞，先放这边 
    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Framework.UI.UIManager.Instance.PopUI();
        }
    }
}
