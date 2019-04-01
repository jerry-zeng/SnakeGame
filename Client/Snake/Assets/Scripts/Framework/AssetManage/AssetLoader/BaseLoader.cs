using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{
    public enum LoadMode
    {
        Async = 0,
        Sync,
    }

    /// <summary>
    /// Base loader callback.
    /// 'isOk': the load result flag
    /// 'resultObject': the loaded result object, most time is UnityEngine.Object
    /// </summary>
    public delegate void LoaderCallback(bool isOk, object resultObject);


    /// <summary>
    /// 所有资源Loader的基类
    /// </summary>
    public abstract class BaseLoader
    {
        protected const string LOG_TAG = "BaseLoader";

        public enum ErrorType : uint
        {
            None = 0,
            Success,
            Canceled,
            Timeout,
            GC,
        }


        public string Url { get; protected set; }

        public LoadMode loadMode { get; protected set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; protected set; }

        /// <summary>
        /// 进度百分比~ 0-1浮点
        /// </summary>
        public virtual float Progress { get; protected set; }

        /// <summary>
        /// 最终加载结果的资源
        /// </summary>
        public object ResultObject { get; protected set; }

        /// <summary>
        /// 是否已经完成，它的存在令Loader可以用于协程StartCoroutine
        /// </summary>
        public bool IsCompleted { get; protected set; }

        /// <summary>
        /// 错误类型.
        /// </summary>
        public ErrorType Error { get; protected set; }

        /// <summary>
        /// 类似WWW, IsFinished再判断是否有错误对吧
        /// </summary>
        public bool IsError 
        { 
            get { return Error > ErrorType.Success; } 
        }

        /// <summary>
        /// RefCount 为 0，进入预备Disposed状态
        /// </summary>
        public bool IsReadyDisposed { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsSuccess
        {
            get { return !IsError && ResultObject != null && !IsReadyDisposed; }
        }

        public bool IsForceNew;

        /// <summary>
        /// 用时
        /// </summary>
        public float CostTime
        {
            get
            {
                if (!IsCompleted) 
                    return -1f;
                else
                    return _finishTime - _initTime;
            }
        }

        #region Debug
        public Action<string> SetDescEvent;

        private string _desc = "";

        /// <summary>
        /// Gets or sets the desc. Used for debugger.
        /// </summary>
        /// <value>The desc.</value>
        public virtual string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                if (SetDescEvent != null)
                    SetDescEvent(_desc);
            }
        }

        public Action DisposeEvent;
        #endregion

        protected readonly List<LoaderCallback> _callbacks = new List<LoaderCallback>();
        protected float _initTime = -1f;
        protected float _finishTime = -1f;


        public static T Load<T>(string url, LoadMode loadMode = LoadMode.Async, LoaderCallback callback = null, 
                                bool forceCreateNew = false, params object[] initArgs) where T : BaseLoader, new()
        {
            return LoaderCache.AutoNew<T>(url, loadMode, callback, forceCreateNew, initArgs);
        }

        public BaseLoader()
        {
            RefCount = 0;
        }

        public virtual void Init(string url, LoadMode loadMode, params object[] args)
        {
            Url = url;
            this.loadMode = loadMode;
            Progress = 0f;
            _initTime = Time.realtimeSinceStartup;

            ResultObject = null;
            Error = ErrorType.None;
            IsCompleted = false;
            IsReadyDisposed = false;
        }

        public virtual void AddRef()
        {
            RefCount++;
        }

        /// <summary>
        /// 复活
        /// </summary>
        public virtual void Revive()
        {
            IsReadyDisposed = false; // 复活！
        }

        protected virtual void OnFinish(object resultObj)
        {
            // 如果ReadyDispose，无效！不用传入最终结果！
            ResultObject = resultObj;

            _finishTime = Time.realtimeSinceStartup;
            Progress = 1f;
            IsCompleted = true;

            // 如果 IsReadyDisposed, 依然会保存 ResultObject, 但在回调时会失败~无回调对象
            if( IsReadyDisposed )
            {
                Error = ErrorType.GC;

                DoCallback(IsSuccess, null);

                this.Log("[BaseLoader:OnFinish]时，准备Disposed {0}", Url);

                DoDispose();
            }
            else
            {
                Error = ErrorType.Success;

                DoCallback(IsSuccess, ResultObject);
            }
        }

        /// <summary>
        /// Abort this loader.
        /// </summary>
        public virtual void Abort()
        {
            Error = ErrorType.Canceled;

            //_callbacks.Clear();
        }

        /// <summary>
        /// 在IsFinisehd后执行的回调
        /// </summary>
        /// <param name="callback"></param>
        public void AddCallback(LoaderCallback callback)
        {
            if( callback == null ) return;

            if (IsCompleted)
            {
                callback(IsSuccess, ResultObject);
            }
            else
                _callbacks.Add(callback);
        }

        protected void DoCallback(bool isOk, object resultObj)
        {
            foreach (var callback in _callbacks)
                callback(isOk, resultObj);
            
            _callbacks.Clear();
        }

        /// <summary>
        /// 释放资源，减少引用计数
        /// </summary>
        public virtual void Release()
        {
            if (IsReadyDisposed && Debug.isDebugBuild)
            {
                this.LogWarning("[{0}]Too many dipose! {1}, Count: {2}", GetType().Name, this.Url, RefCount);
            }

            RefCount--;
            if (RefCount <= 0)
            {
                // 加入队列，准备Dispose
                LoaderCache.AddUnusedLoader(this);

                IsReadyDisposed = true;
                OnReadyDisposed();
            }
        }

        /// <summary>
        /// 引用为0时，进入准备Disposed状态时触发
        /// </summary>
        protected virtual void OnReadyDisposed()
        {
            
        }

        /// <summary>
        /// Dispose是有引用检查的， DoDispose一般用于继承重写
        /// </summary>
        public void Dispose()
        {
            if (DisposeEvent != null)
                DisposeEvent();
            DisposeEvent = null;

            if (!IsForceNew)
            {
                bool bRemove = LoaderCache.RemoveLoader(this);
                if (!bRemove)
                {
                    Debuger.LogWarning("[{0}:Dispose], No Url: {1}, Cur RefCount: {2}", GetType().Name, Url, RefCount);
                }
            }

            if( IsCompleted ){
                DoDispose();
            }
            else{
                // 未完成，在OnFinish时会执行DoDispose
            }
        }

        protected virtual void DoDispose()
        {
            RefCount = 0;
            Init(null, loadMode);

            SetDescEvent = null;
        }

        /// <summary>
        /// 强制进行Dispose，无视Ref引用数，建议用在RefCount为1的Loader上
        /// </summary>
        public virtual void ForceDispose()
        {
            if (RefCount != 1)
            {
                Debuger.LogWarning(LOG_TAG, "[ForceDisose]Use force dispose to dispose loader, recommend this loader RefCount == 1");
            }
            Dispose();
        }

    }

    // Unity潜规则: 等待帧最后再执行，避免一些(DestroyImmediatly)在Phycis函数内
}
