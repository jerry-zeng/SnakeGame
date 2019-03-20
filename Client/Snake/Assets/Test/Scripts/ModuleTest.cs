using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.Module;

namespace Test
{
    public class ModuleTest : MonoBehaviour 
    {
        void Start()
        {
            Debuger.EnableLog = true;
            this.Log("Start Test");

            ModuleManager.Instance.SetDomain("Test");

            ModuleManager.Instance.CreateModule("ModuleA");
            ModuleManager.Instance.CreateModule<ModuleB>();

            ModuleC.Instance.DoSomething(false);
            ModuleC.Instance.ReleaseSomeModule("ModuleB");
            ModuleC.Instance.DoSomething(true);

            BusinessModule module = ModuleManager.Instance.GetModule("ModuleA");
            if( module != null ){
                module.Open("Hello");
                module.Close();
            }

            ModuleManager.Instance.Release();
            ModuleC.Instance.DoSomething(true);
        }
    }

    class ModuleA : BusinessModule
    {
        public override void Create(object arg)
        {
            base.Create(arg);
            EventManager.Instance.RegisterEvent<bool>(EventDef.OnLogin, OnLogin);
        }

        public override void Open(object arg)
        {
            base.Open(arg);
            this.LogWarning("Open() arg = {0}", arg);
        }

        public override void Close()
        {
            base.Close();
            this.LogWarning("Close()");
        }

        public override void Release()
        {
            base.Release();
            EventManager.Instance.UnregisterEvent<bool>(EventDef.OnLogin, OnLogin);
        }

        void OnLogin(bool value)
        {
            this.LogWarning("OnLogin: {0}", value);
        }
    }

    class ModuleB : BusinessModule
    {
        public override void Create(object arg)
        {
            base.Create(arg);
            EventManager.Instance.RegisterEvent<bool>(EventDef.OnLogin, OnLogin);
        }

        public override void Release()
        {
            base.Release();
            EventManager.Instance.UnregisterEvent<bool>(EventDef.OnLogin, OnLogin);
            this.Log("Release()");
        }

        void OnLogin(bool value)
        {
            this.LogWarning("OnLogin: {0}", value);
        }
    }

    class ModuleC : ServiceModule<ModuleC>
    {
        protected override void Init()
        {
            base.Init();

            this.Log("ModuleC::Init()");
        }

        public void DoSomething(bool value)
        {
            EventManager.Instance.SendEvent<bool>("OnLogin", value);
        }

        public void ReleaseSomeModule(string moduleName)
        {
            this.Log("ReleaseSomeModule() {0}", moduleName);
            ModuleManager.Instance.ReleaseModule(moduleName);
        }
    }
}
