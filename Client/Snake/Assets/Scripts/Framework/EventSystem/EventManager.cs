using System;
using System.Collections.Generic;
using Framework.Module;

namespace Framework
{
    /// <summary>
    /// Adds a generic event system. The event system allows objects to register, unregister, and execute events on a particular object.
    /// </summary>
    public class EventManager : ServiceModule<EventManager>
    {
        // Internal variables
        private Dictionary<object, Dictionary<string, Delegate>> _eventTable = new Dictionary<object, Dictionary<string, Delegate>>();
        private Dictionary<string, Delegate> _globalEventTable = new Dictionary<string, Delegate>();

        protected override void Init()
        {
            base.Init();

            _eventTable.Clear();
            _globalEventTable.Clear();
        }

        #region Internel Implements
        private void RegisterEvent(string eventName, Delegate handler)
        {
            Delegate prevHandlers;
            if (_globalEventTable.TryGetValue(eventName, out prevHandlers)) {
                _globalEventTable[eventName] = Delegate.Combine(prevHandlers, handler);
            }
            else {
                _globalEventTable.Add(eventName, handler);
            }
        }

        private void RegisterEvent(object obj, string eventName, Delegate handler)
        {
            if (obj == null) {
                Debuger.LogError("EventManager.RegisterEvent error: target object cannot be null.");
                return;
            }

            Dictionary<string, Delegate> handlers;
            if(!_eventTable.TryGetValue(obj, out handlers)) {
                handlers = new Dictionary<string, Delegate>();
                _eventTable.Add(obj, handlers);
            }

            Delegate prevHandlers;
            if(handlers.TryGetValue(eventName, out prevHandlers)) {
                handlers[eventName] = Delegate.Combine(prevHandlers, handler);
            }
            else {
                handlers.Add(eventName, handler);
            }
        }

        private Delegate GetDelegate(string eventName)
        {
            Delegate handler;
            if(_globalEventTable.TryGetValue(eventName, out handler)) 
            {
                return handler;
            }
            return null;
        }

        private Delegate GetDelegate(object obj, string eventName)
        {
            Dictionary<string, Delegate> handlers;
            if (_eventTable.TryGetValue(obj, out handlers)) 
            {
                Delegate handler;
                if (handlers.TryGetValue(eventName, out handler)) {
                    return handler;
                }
            }
            return null;
        }

        private void UnregisterEvent(string eventName, Delegate handler)
        {
            Delegate prevHandlers;
            if(_globalEventTable.TryGetValue(eventName, out prevHandlers))
            {
                _globalEventTable[eventName] = Delegate.Remove(prevHandlers, handler);
            }
        }

        private void UnregisterEvent(object obj, string eventName, Delegate handler)
        {
            if(obj == null) {
                Debuger.LogError("EventManager.UnregisterEvent error: target object cannot be null.");
                return;
            }

            Dictionary<string, Delegate> handlers;
            if (_eventTable.TryGetValue(obj, out handlers))
            {
                Delegate prevHandlers;
                if (handlers.TryGetValue(eventName, out prevHandlers)) {
                    handlers[eventName] = Delegate.Remove(prevHandlers, handler);
                }
            }
        }
        #endregion


        public void ClearEventTable()
        {
            _eventTable.Clear();
            SendEvent("OnEventTableClear");
        }

        public void ClearGlobalEventTable()
        {
            _globalEventTable.Clear();
        }

        public void ClearAll()
        {
            _eventTable.Clear();
            _globalEventTable.Clear();
        }

        #region Register Methods
        public void RegisterEvent(string eventName, Action handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent(object obj, string eventName, Action handler)
        {
            RegisterEvent(obj, eventName, (Delegate)handler);
        }

        public void RegisterEvent<T>(string eventName, Action<T> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T>(object obj, string eventName, Action<T> handler)
        {
            RegisterEvent(obj, eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U>(string eventName, Action<T, U> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U>(object obj, string eventName, Action<T, U> handler)
        {
            RegisterEvent(obj, eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V>(object obj, string eventName, Action<T, U, V> handler)
        {
            RegisterEvent(obj, eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            RegisterEvent(eventName, (Delegate)handler);
        }

        public void RegisterEvent<T, U, V, W>(object obj, string eventName, Action<T, U, V, W> handler)
        {
            RegisterEvent(obj, eventName, (Delegate)handler);
        }
        #endregion

        #region Unregister Methods
        public void UnregisterEvent(string eventName, Action handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent(object obj, string eventName, Action handler)
        {
            UnregisterEvent(obj, eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T>(string eventName, Action<T> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T>(object obj, string eventName, Action<T> handler)
        {
            UnregisterEvent(obj, eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U>(string eventName, Action<T, U> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U>(object obj, string eventName, Action<T, U> handler)
        {
            UnregisterEvent(obj, eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V>(object obj, string eventName, Action<T, U, V> handler)
        {
            UnregisterEvent(obj, eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            UnregisterEvent(eventName, (Delegate)handler);
        }

        public void UnregisterEvent<T, U, V, W>(object obj, string eventName, Action<T, U, V, W> handler)
        {
            UnregisterEvent(obj, eventName, (Delegate)handler);
        }
        #endregion

        #region Execute Methods
        public void SendEvent(string eventName)
        {
            var handler = GetDelegate(eventName) as Action;
            if (handler != null) {
                handler();
            }
        }

        public void SendEvent(object obj, string eventName)
        {
            var handler = GetDelegate(obj, eventName) as Action;
            if (handler != null) {
                handler();
            }
        }

        public void SendEvent<T>(string eventName, T arg1)
        {
            var handler = GetDelegate(eventName) as Action<T>;
            if (handler != null) {
                handler(arg1);
            }
        }

        public void SendEvent<T>(object obj, string eventName, T arg1)
        {
            var handler = GetDelegate(obj, eventName) as Action<T>;
            if (handler != null) {
                handler(arg1);
            }
        }

        public void SendEvent<T, U>(string eventName, T arg1, U arg2)
        {
            var handler = GetDelegate(eventName) as Action<T, U>;
            if (handler != null) {
                handler(arg1, arg2);
            }
        }

        public void SendEvent<T, U>(object obj, string eventName, T arg1, U arg2)
        {
            var handler = GetDelegate(obj, eventName) as Action<T, U>;
            if (handler != null) {
                handler(arg1, arg2);
            }
        }

        public void SendEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V>;
            if (handler != null) {
                handler(arg1, arg2, arg3);
            }
        }

        public void SendEvent<T, U, V>(object obj, string eventName, T arg1, U arg2, V arg3)
        {
            var handler = GetDelegate(obj, eventName) as Action<T, U, V>;
            if (handler != null) {
                handler(arg1, arg2, arg3);
            }
        }

        public void SendEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V, W>;
            if (handler != null) {
                handler(arg1, arg2, arg3, arg4);
            }
        }

        public void SendEvent<T, U, V, W>(object obj, string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            var handler = GetDelegate(obj, eventName) as Action<T, U, V, W>;
            if (handler != null) {
                handler(arg1, arg2, arg3, arg4);
            }
        }
        #endregion

    }
}