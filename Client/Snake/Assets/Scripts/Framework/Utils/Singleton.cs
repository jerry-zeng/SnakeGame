
namespace Framework
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly object _locker = new object();

        private static T _instance = null;
        public static T Instance
        {
            get{ 
                if( _instance == null )
                {
                    lock( _locker ){
                        if( _instance == null ){
                            _instance = new T();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        public virtual void Init()
        {
            
        }

        public virtual void Destroy()
        {
            
        }

        public static void DestroyInstance()
        {
            if( _instance != null )
                _instance.Destroy();
            _instance = null;
        }
    }
}
