
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exception = System.Exception;

namespace Framework
{
    public class Scheduler : MonoSingleton<Scheduler>
    {
        protected override void InitSingleton()
        {
            base.InitSingleton();

            _pool = new ObjectPool<Timer>( onRelease: (timer)=>{
                timer.Reset();
            } );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // when singleton is being destoryed, it's no need to return timers to pool.
            _timersToAdd.Clear();
            _workingTimers.Clear();
            _workingTimersCount = 0;
            _waitingTimers.Clear();
            _waitingTimersCount = 0;
            _nextID = TIMER_START_ID;
            _cancelAll = false;
            _hasPrepared = false;
        }

        //===========================================================

        #region Scheduler
        public const float MIN_INTERVAL = 1f / 60f;
        public const uint TIMER_START_ID = 1;

        private ObjectPool<Timer> _pool;
        private int MaxPoolSize = 50;

        private List<Timer> _workingTimers = new List<Timer>(50);
        private int _workingTimersCount = 0;
        private List<Timer> _waitingTimers = new List<Timer>(50);
        private int _waitingTimersCount = 0;

        [System.NonSerialized]
        public float TimeThrehold = 1f;
        private float _minExecTimeOnWait = float.MaxValue;
        private bool _hasPrepared = false;

        private List<Timer> _timersToAdd = new List<Timer>();
        private uint _nextID = TIMER_START_ID;
        private bool _isRunning = false;
        private bool _cancelAll = false;


        public static uint Schedule(float interval, uint repeatTimes, Action callback)
        {
            if(Instance != null) 
            {
                return Instance.AddEventInternal(interval, repeatTimes, callback);
            }
            return 0;
        }

        public static uint ScheduleNextFrame(Action callback)
        {
            return Schedule(0f, 1, callback);
        }

        public static uint ScheduleOnce(float delay, Action callback)
        {
            return Schedule(delay, 1, callback);
        }

        /// <summary>
        /// Internal method to add a new event to be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="callback">The delegate to execute after the specified delay.</param>
        /// <returns>The ScheduledEvent instance, useful if the event should be cancelled.</returns>
        private uint AddEventInternal(float interval, uint repeatTimes, Action callback)
        {
            // Don't add the event if the game hasn't started.
            if( isActiveAndEnabled == false || callback == null )
                return 0;

            if( interval <= 0f ) interval = MIN_INTERVAL;

            Timer timer = GetTimer();
            float nextTime = GetCurrentTime() + interval;
            timer.Init( _nextID, interval, repeatTimes, callback, nextTime );

            _nextID++;

            if( _isRunning ){
                _timersToAdd.Add( timer );
            }
            else
            {
                if( !_hasPrepared ){
                    PushToWaitingQueue(timer);
                }
                else
                {
                    if( nextTime < _minExecTimeOnWait + TimeThrehold ){
                        PushToWorkingQueue(timer);
                    }
                    else{
                        PushToWaitingQueue(timer);
                    }
                }
            }

            return _nextID;
        }

        /// <summary>
        /// Cancels an event.
        /// </summary>
        /// <param name="id">The timer id to cancel.</param>
        public static bool Cancel(uint id)
        {
            if( Instance != null ) 
            {
                return Instance.CancelEventInternal(id);
            }
            return false;
        }

        /// <summary>
        /// Internal method to cancel an event.
        /// </summary>
        /// <param name="id">The timer id to cancel.</param>
        private bool CancelEventInternal(uint id)
        {
            if( id == 0 )
                return false;

            for( int i = 0; i < _workingTimersCount; i++ )
            {
                if( _workingTimers[i].ID == id ){
                    _workingTimers[i].Abort();
                    return true;
                }
            }

            for( int i = 0; i < _waitingTimersCount; i++ )
            {
                if( _waitingTimers[i].ID == id ){
                    _waitingTimers[i].Abort();
                    return true;
                }
            }

            for( int i = 0; i < _timersToAdd.Count; i++ )
            {
                if( _timersToAdd[i].ID == id ){
                    _timersToAdd[i].Abort();
                    return true;
                }
            }

            return false;
        }


        public void CancelAll()
        {
            if( _isRunning ){
                _cancelAll = true;
                return;
            }

            _cancelAll = false;

            _timersToAdd.Clear();
            _workingTimers.Clear();
            _waitingTimers.Clear();

            _pool.Clear();
        }


        private Timer GetTimer()
        {
            return _pool.Get();
        }
        private bool ReleaseTimer(Timer timer)
        {
            if( _pool.Count >= MaxPoolSize )
                return false;
            
            _pool.Release(timer);
            return true;
        }

        private float GetCurrentTime()
        {
            return Time.time;
        }

        void RemoveWorkingQueueAt(int index)
        {
            if( index < 0 || index >= _workingTimersCount )
                return;

            _workingTimers[index] = _workingTimers[_workingTimersCount-1];
            _workingTimers[_workingTimersCount - 1] = null;
            _workingTimersCount--;
        }
        void PushToWorkingQueue(Timer timer)
        {
            if( timer == null ) return;

            if( _workingTimersCount >= _workingTimers.Count ){
                _workingTimers.Add(timer);
            }
            else{
                _workingTimers[_workingTimersCount] = timer;
            }
            _workingTimersCount++;
        }
        void RemoveWaitingQueueAt(int index)
        {
            if( index < 0 || index >= _waitingTimersCount )
                return;

            _waitingTimers[index] = _waitingTimers[_waitingTimersCount-1];
            _waitingTimers[_waitingTimersCount - 1] = null;
            _waitingTimersCount--;
        }
        void PushToWaitingQueue(Timer timer)
        {
            if( timer == null ) return;

            _hasPrepared = true;

            if( _waitingTimersCount >= _waitingTimers.Count ){
                _waitingTimers.Add(timer);
            }
            else{
                _waitingTimers[_waitingTimersCount] = timer;
            }
            _waitingTimersCount++;

            float nextTime = timer.NextExecuteTime;
            if( nextTime < _minExecTimeOnWait )
                _minExecTimeOnWait = nextTime;
        }

        private void AddWaitingTimers()
        {
            foreach( Timer timer in _timersToAdd )
            {
                if( timer.IsAborted ){
                    ReleaseTimer(timer);
                    continue;
                }

                float nextTime = timer.NextExecuteTime;

                if( nextTime < _minExecTimeOnWait && _hasPrepared ){
                    PushToWorkingQueue(timer);
                }
                else{
                    PushToWaitingQueue(timer);
                }
            }
            _timersToAdd.Clear();
        }

        private void UpdateWaitingTimers(float curTime)
        {
            if( curTime >= _minExecTimeOnWait )
            {
                float newMinExecTimeOnWait = float.MaxValue;

                for( int i = _waitingTimersCount-1; i >= 0; i-- )
                {
                    Timer timer = _waitingTimers[i];
                    if( timer.IsAborted )
                    {
                        ReleaseTimer( timer );
                        RemoveWaitingQueueAt(i);
                        continue;
                    }

                    float nextTime = timer.NextExecuteTime;
                    if( nextTime <= _minExecTimeOnWait + TimeThrehold ){
                        //move to working queue
                        RemoveWaitingQueueAt(i);
                        PushToWorkingQueue(timer);
                    }
                    else
                    {
                        // keep it
                        // update newMinExecTimeOnWait
                        if( nextTime < newMinExecTimeOnWait )
                            newMinExecTimeOnWait = nextTime;
                    }
                }

                _minExecTimeOnWait = newMinExecTimeOnWait;
            }

        }

        private void UpdateWorkingTimers(float curTime)
        {
            for( int i = _workingTimersCount-1; i >= 0; i-- )
            {
                Timer timer = _workingTimers[i];
                if( timer.IsAborted )
                {
                    ReleaseTimer( timer );
                    RemoveWorkingQueueAt(i);
                    continue;
                }

                float nextTime = timer.NextExecuteTime;
                if( curTime >= nextTime )
                {
                    timer.Trigger();

                    if( timer.IsAborted )
                    {
                        ReleaseTimer( timer );
                        RemoveWorkingQueueAt(i);
                    }
                    else
                    {
                        nextTime = curTime + timer.Interval;
                        timer.NextExecuteTime = nextTime;

                        if( nextTime < _minExecTimeOnWait ){
                            // keep it
                        }
                        else{
                            //move to waiting queue
                            RemoveWorkingQueueAt(i);
                            PushToWaitingQueue(timer);
                        }
                    }
                }
            }

        }

        private void UpdateTimers()
        {
            AddWaitingTimers();

            _isRunning = true;

            float curTime = GetCurrentTime();

            // sweep waiting timers
            UpdateWaitingTimers(curTime);

            // update working timers
            UpdateWorkingTimers(curTime);

            _isRunning = false;

            //async
            if( _cancelAll )
                CancelAll();
        }
        #endregion

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

        void Update()
        {
            UpdateTimers();

            if (UpdateEvent != null)
            {
                try
                {
                    UpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("Scheduler", "Update() Error:{0}\n{1}", e.Message, e.StackTrace);
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
                    Debuger.LogError("Scheduler", "LateUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
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
                    Debuger.LogError("Scheduler", "FixedUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
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
