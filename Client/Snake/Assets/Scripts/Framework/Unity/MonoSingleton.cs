
using UnityEngine;

namespace Framework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = default(T);
        public static T Instance
        {
            get{ return _instance; }
        }

        protected static T Instantiate()
        {
            if (_instance == null)
            {
                GameObject singleton = new GameObject("[Singleton]" + typeof(T).Name);
                _instance = singleton.AddComponent<T>();
                _instance.InitSingleton();
            }

            return _instance;
        }

        protected virtual void InitSingleton()
        {

        }

        public static T CreateInstance()
        {
            if( _instance == null )
                _instance = Instantiate();
            return _instance;
        }

        public static void DestroyInstance()
        {
            if( _instance != null )
                Destroy(_instance.gameObject);
            _instance = null;
        }

        public void MakePersistence()
        {
            DontDestroyOnLoad(gameObject);
        }


        protected virtual void Awake()
        {
            if( _instance == null )
            {
                _instance = this as T;
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if(_instance == this)
                _instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            if(_instance == this)
                _instance = null;
        }
    }
}
