using System;
using System.Collections.Generic;
using Framework.Module;

namespace Framework
{
    /// <summary>
    /// Adds a generic event system. The event system allows objects to register, unregister, and execute events on a particular object.
    /// </summary>
    public sealed class EventManager : ServiceModule<EventManager>
    {
        private const string LOG_TAG = "EventManager";

        public bool ShowLog { get; set; }

        // Internal variables
        private Dictionary<string, Delegate> _eventTable;

        protected override void Init()
        {
            base.Init();

            ShowLog = true;
            _eventTable = new Dictionary<string, Delegate>();
        }

        #region Internel Implements
        private void RegisterEvent(string eventName, Delegate handler)
        {
            Delegate prevHandlers;
            if (_eventTable.TryGetValue(eventName, out prevHandlers)) {
                _eventTable[eventName] = Delegate.Combine(prevHandlers, handler);
            }
            else {
                _eventTable.Add(eventName, handler);
            }
        }

        private Delegate GetDelegate(string eventName)
        {
            Delegate handler;
            if(_eventTable.TryGetValue(eventName, out handler)) 
            {
                return handler;
            }
            return null;
        }

        private void UnregisterEvent(string eventName, Delegate handler)
        {
            Delegate prevHandlers;
            if( _eventTable.TryGetValue(eventName, out prevHandlers) )
            {
                _eventTable[eventName] = Delegate.Remove(prevHandlers, handler);
            }
            else
                LogWarning( "Not found event by {0}", eventName );
        }

        private void LogWarning( string format, params object[] args )
        {
            if( ShowLog ){
                Debuger.LogWarning(LOG_TAG, format, args);
            }
        }
        #endregion

        public void ClearAll()
        {
            _eventTable.Clear();
        }

        #region Register Methods
        public void RegisterEvent(string eventName, Action handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T>(string eventName, Action<T> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U>(string eventName, Action<T, U> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent(string eventName, Action<object[]> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }
        #endregion

        #region Unregister Methods
        public void UnregisterEvent(string eventName, Action handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T>(string eventName, Action<T> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U>(string eventName, Action<T, U> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent(string eventName, Action<object[]> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }
        #endregion

        #region Execute Methods
        public void SendEvent(string eventName)
        {
            var handler = GetDelegate(eventName) as Action;
            if (handler != null) {
                handler();
            }
            else
                LogWarning( "Not found any event handler (Action) by {0}", eventName );
        }

        public void SendEvent<T>(string eventName, T arg1)
        {
            var handler = GetDelegate(eventName) as Action<T>;
            if (handler != null) {
                handler(arg1);
            }
            else
                LogWarning( "Not found any event handler (Action<T>) by {0}", eventName );
        }

        public void SendEvent<T, U>(string eventName, T arg1, U arg2)
        {
            var handler = GetDelegate(eventName) as Action<T, U>;
            if (handler != null) {
                handler(arg1, arg2);
            }
            else
                LogWarning( "Not found any event handler (Action<T, U>) by {0}", eventName );
        }

        public void SendEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V>;
            if (handler != null) {
                handler(arg1, arg2, arg3);
            }
            else
                LogWarning( "Not found any event handler (Action<T, U, V>) by {0}", eventName );
        }

        public void SendEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V, W>;
            if (handler != null) {
                handler(arg1, arg2, arg3, arg4);
            }
            else
                LogWarning( "Not found any event handler (Action<T, U, V, W>) by {0}", eventName );
        }

        public void SendEvent(string eventName, object[] args)
        {
            var handler = GetDelegate(eventName) as Action<object[]>;
            if (handler != null) {
                handler(args);
            }
            else
                LogWarning( "Not found any event handler (Action<object[]>) by {0}", eventName );
        }
        #endregion

    }
}