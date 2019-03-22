using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class Timer
    {
        public uint ID { get; private set;}
        public Action Callback { get; private set; }
        public float NextExecuteTime { get; set;}
        public uint RepeatTimes { get; private set;}
        public float Interval { get; private set; }
        public bool IsAborted { get; private set;}
        public bool RunForever { get; private set;}

        public void Init( uint id, float interval, uint repeatTimes, Action callback, float nextExecuteTime )
        {
            this.ID = id;
            this.Interval = interval;
            this.RepeatTimes = repeatTimes;
            this.Callback = callback;
            this.NextExecuteTime = nextExecuteTime;
            this.IsAborted = false;
            this.RunForever = repeatTimes == 0;
        }

        public void Reset()
        {
            ID = 0;
            Callback = null;
            NextExecuteTime = float.MaxValue;
            RepeatTimes = 0;
            Interval = 0f;
            IsAborted = false;
            RunForever = false;
        }

        public void Trigger()
        {
            if( Callback != null )
                Callback.Invoke();
            Callback = null;

            if( RepeatTimes > 0 )
                RepeatTimes--;

            if( RepeatTimes <= 0 && !RunForever )
                Abort();
        }

        public void Abort()
        {
            IsAborted = true;
        }
    }

}
