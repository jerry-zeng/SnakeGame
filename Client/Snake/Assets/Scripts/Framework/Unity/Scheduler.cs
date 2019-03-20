////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exception = System.Exception;


namespace Framework
{
    public class ScheduledEvent
    {
        public Action Callback { get { return _callback; } set { _callback = value; } }
        public Action<object> CallbackWithArg { get { return _callbackArg; } set { _callbackArg = value; } }
        public object Argument { get { return _argument; } set { _argument = value; } }
        public float RemainTime { get { return _remainTime; } set { _remainTime = value; } }

        // Internal variables.
        private Action _callback = null;
        private Action<object> _callbackArg = null;
        private object _argument;
        private float _remainTime;

        public void Reset()
        {
            _callback = null;
            _callbackArg = null;
            _argument = null;
        }
    }


    public class Scheduler : MonoSingleton<Scheduler>
    {

        protected override void InitSingleton()
        {
            base.InitSingleton();

            _activeSchedulers.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _activeSchedulers.Clear();
        }

        //===========================================================
        #region MonoUpdateEvent
        protected event MonoUpdateEvent UpdateEvent;
        protected event MonoUpdateEvent LateUpdateEvent;
        protected event MonoUpdateEvent FixedUpdateEvent;


        public static void AddUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.UpdateEvent += listener;
            }
        }

        public static void RemoveUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.UpdateEvent -= listener;
            }
        }

        public static void AddLateUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.LateUpdateEvent += listener;
            }
        }

        public static void RemoveLateUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.LateUpdateEvent -= listener;
            }
        }

        public static void AddFixedUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.FixedUpdateEvent += listener;
            }
        }

        public static void RemoveFixedUpdateListener(MonoUpdateEvent listener)
        {
            if (Instance != null)
            {
                Instance.FixedUpdateEvent -= listener;
            }
        }
        #endregion

        #region Scheduler
        private List<ScheduledEvent> _activeSchedulers = new List<ScheduledEvent>();


        public static ScheduledEvent Schedule(float delay, Action callback)
        {
            if(Instance != null) 
            {
                return Instance.AddEventInternal(delay, callback);
            }
            return null;
        }

        /// <summary>
        /// Internal method to add a new event to be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="callback">The delegate to execute after the specified delay.</param>
        /// <returns>The ScheduledEvent instance, useful if the event should be cancelled.</returns>
        private ScheduledEvent AddEventInternal(float delay, Action callback)
        {
            // Don't add the event if the game hasn't started.
            if( isActiveAndEnabled == false || callback == null )
                return null;

            if( delay <= 0 )
            {
                callback();
                return null;
            }
            else
            {
                ScheduledEvent scheduledEvent = new ScheduledEvent();// ObjectPool.Get<ScheduledEvent>();
                scheduledEvent.Reset();
                scheduledEvent.RemainTime = delay;
                scheduledEvent.Callback = callback;
                _activeSchedulers.Add(scheduledEvent);

                return scheduledEvent;
            }
        }

        /// <summary>
        /// Add a new event with an argumentto be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="callback">The delegate to execute after the specified delay.</param>
        /// <param name="arg">The argument of the delegate.</param>
        /// <returns>The ScheduledEvent instance, useful if the event should be cancelled.</returns>
        public static ScheduledEvent Schedule(float delay, Action<object> callback, object arg)
        {
            if(Instance != null) 
            {
                return Instance.AddEventInternal(delay, callback, arg);
            }
            return null;
        }

        /// <summary>
        /// Internal event to add a new event with an argumentto be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="callback">The delegate to execute after the specified delay.</param>
        /// <param name="arg">The argument of the delegate.</param>
        /// <returns>The ScheduledEvent instance, useful if the event should be cancelled.</returns>
        private ScheduledEvent AddEventInternal(float delay, Action<object> callbackArg, object arg)
        {
            if( isActiveAndEnabled == false || callbackArg == null )
                return null;
            
            if(delay <= 0)
            {
                callbackArg(arg);
                return null;
            } 
            else
            {
                ScheduledEvent scheduledEvent = new ScheduledEvent();// ObjectPool.Get<ScheduledEvent>();
                scheduledEvent.Reset();
                scheduledEvent.RemainTime = delay;
                scheduledEvent.CallbackWithArg = callbackArg;
                scheduledEvent.Argument = arg;
                _activeSchedulers.Add(scheduledEvent);

                return scheduledEvent;
            }
        }

        /// <summary>
        /// Cancels an event.
        /// </summary>
        /// <param name="scheduledEvent">The event to cancel.</param>
        public static void Cancel(ref ScheduledEvent scheduledEvent)
        {
            if( Instance != null ) 
            {
                Instance.CancelEventInternal(ref scheduledEvent);
            }
        }

        /// <summary>
        /// Internal method to cancel an event.
        /// </summary>
        /// <param name="scheduledEvent">The event to cancel.</param>
        private void CancelEventInternal(ref ScheduledEvent scheduledEvent)
        {
            if(scheduledEvent != null && _activeSchedulers.Contains(scheduledEvent))
            {
                _activeSchedulers.Remove(scheduledEvent);
                //ObjectPool.Return(scheduledEvent);
                scheduledEvent = null;
            }
        }

        /// <summary>
        /// Executes an event with the specified index.
        /// </summary>
        /// <param name="index">The index of the event to execute.</param>
        private void Execute(int index)
        {
            var activeEvent = _activeSchedulers[index];
            // Remove the event from the list before the callback to prevent the callback from adding a new event and changing the order.
            _activeSchedulers.RemoveAt(index);

            if (activeEvent.Callback != null) {
                activeEvent.Callback();
            }
            else {
                activeEvent.CallbackWithArg(activeEvent.Argument);
            }
            //ObjectPool.Return(activeEvent);
        }
        #endregion

        void Update()
        {
            // update schedulers
            for( int i = _activeSchedulers.Count-1; i >= 0; --i )
            {
                _activeSchedulers[i].RemainTime -= Time.deltaTime;

                if( _activeSchedulers[i].RemainTime <= 0f )
                    Execute(i);
            }

            if (UpdateEvent != null)
            {
                try
                {
                    UpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "Update() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        void LateUpdate()
        {
            if (LateUpdateEvent != null)
            {
                try
                {
                    LateUpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "LateUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        void FixedUpdate()
        {
            if (FixedUpdateEvent != null)
            {
                try
                {
                    FixedUpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "FixedUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        //===========================================================
        public static void DoStartCoroutine(IEnumerator routine)
        {
            if(Instance != null)
                Instance.StartCoroutine(routine);
        }
    }
}
