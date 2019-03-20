using System;
using System.Collections.Generic;

namespace Framework.Module
{
    // TODO: 通过ModuleManager给不同Module发送消息；缓存未创建的Module收到的消息//

    public class ModuleManager : ServiceModule<ModuleManager>
    {
        private string _domain = "";
        private Dictionary<string, BusinessModule> _moduleTable = new Dictionary<string, BusinessModule>();


        protected override void Init()
        {
            base.Init();

            _domain = "";
            _moduleTable.Clear();
        }

        public void SetDomain(string domain)
        {
            _domain = domain;
        }

        public override void Release()
        {
            foreach( var pair in _moduleTable )
            {
                pair.Value.Release();
            }
            _moduleTable.Clear();

            base.Release();
        }




        public T CreateModule<T>(object arg = null) where T : BusinessModule
        {
            return (T)CreateModule(typeof(T).Name, arg);
        }

        public BusinessModule CreateModule(string moduleName, object arg = null)
        {
            if( _moduleTable.ContainsKey(moduleName) )
            {
                this.LogWarning("CreateModule(): Module {0} has been already created, do not create again.", moduleName);
                return _moduleTable[moduleName];
            }
            else
            {
                //this.Log("CreateModule(): to create module {0}.", moduleName);

                Type type = null;
                if(string.IsNullOrEmpty(_domain))
                    type = Type.GetType(moduleName);
                else
                    type = Type.GetType(_domain + "." + moduleName);

                BusinessModule module = null;
                if( type != null ){
                    module = Activator.CreateInstance(type) as BusinessModule;
                }
                else{
                    module = new LuaModule(moduleName);
                }

                module.Create(arg);

                _moduleTable.Add(moduleName, module);

                return module;
            }
        }

        public T GetModule<T>() where T : BusinessModule
        {
            return (T)GetModule(typeof(T).Name);
        }

        public BusinessModule GetModule(string moduleName)
        {
            if( _moduleTable.ContainsKey(moduleName) )
                return _moduleTable[moduleName];
            return null;
        }

        public void ReleaseModule(string moduleName)
        {
            if( _moduleTable.ContainsKey(moduleName) )
            {
                BusinessModule module = _moduleTable[moduleName];
                module.Release();

                _moduleTable.Remove(moduleName);
            }
        }

        public void ReleaseModule(BusinessModule module)
        {
            if( _moduleTable.ContainsKey(module.Name) )
            {
                _moduleTable.Remove(module.Name);
            }
            module.Release();
        }

        public void OpenModule(string moduleName, object arg = null)
        {
            if( _moduleTable.ContainsKey(moduleName) )
            {
                BusinessModule module = _moduleTable[moduleName];
                module.Open(arg);
            }
        }

        public void CloseModule(string moduleName)
        {
            if( _moduleTable.ContainsKey(moduleName) )
            {
                BusinessModule module = _moduleTable[moduleName];
                module.Close();
            }
        }

        /// <summary>
        /// Ensures the module has exsited, if it doesn't exsit, create it.
        /// </summary>
        /// <param name="moduleName">Module name.</param>
        /// <param name="arg">Argument.</param>
        public BusinessModule EnsureModule(string moduleName, object arg = null)
        {
            BusinessModule module = GetModule(moduleName);
            if( module == null )
                module = CreateModule(moduleName, arg);
            return module;
        }
    }
}
