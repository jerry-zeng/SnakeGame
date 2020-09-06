using Exception = System.Exception;

namespace Framework.Module
{
    public abstract class ServiceModule<T> : Module where T : ServiceModule<T>, new()
    {
        protected static readonly object _thread_locker = new object();

        protected static T _instance = default(T);
        public static T Instance
        {
            get
            { 
                if( _instance == null )
                {
                    lock( _thread_locker )
                    {
                        if( _instance == null ){
                            _instance = new T();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        protected string _name = null;
        public override string Name
        {
            get
            {
                if( string.IsNullOrEmpty(_name) )
                    _name = typeof(T).Name;
                return _name;
            }
        }

        public static T CreateInstance()
        {
            if( _instance == null ){
                _instance = new T();
            }
            else{
                Debuger.LogError("There exsits an instance of {0} already, don't create again!!!", _instance.Name);
            }
            _instance.Init();

            return _instance;
        }

        public static void DestroyInstance()
        {
            if( _instance != null )
                _instance.Release();
            _instance = null;
        }

        protected void CheckSingleton()
        {
            if (_instance == null)
            {
                throw new Exception("ServiceModule<" + typeof(T).Name + "> can't instance out of itself, as it's a singleton!");
            }
        }

        // pair with Release()
        protected virtual void Init()
        {
            CheckSingleton();
        }

    }
}
