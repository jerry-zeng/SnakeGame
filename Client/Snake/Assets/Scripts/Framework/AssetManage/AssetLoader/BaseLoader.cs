using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

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
    public delegate void LoadAssetCallback(Object resultObject);


    /// <summary>
    /// 所有资源Loader的基类
    /// </summary>
    public abstract class BaseLoader
    {
        protected abstract string LOG_TAG
        {
            get;
        }

        public enum ErrorType : uint
        {
            None = 0,
            Success,
            Canceled,
            Timeout,
            GC,
        }

        public string AssetBundlePath { get; protected set; }
        public string AssetPath { get; protected set; }

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
        public Action DisposeEvent;

        #endregion

        protected readonly List<LoaderCallback> _callbacks = new List<LoaderCallback>();
        protected float _initTime = -1f;
        protected float _finishTime = -1f;

        protected string _uniqueKey = null;

        /// <summary>
        /// Load the specified assetBundlePath, assetPath, loadMode, callback, forceCreateNew and initArgs.
        /// 加载 assetBundle 时: assetPath 为null或者""
        /// 加载 assetBundle中的asset 时: assetBundlePath和assetPath都要填
        /// 加载 非assetBundle中的asset 时: assetBundlePath 为null或者""
        /// </summary>
        public static T Load<T>(string assetBundlePath, string assetPath, LoadMode loadMode = LoadMode.Async, LoaderCallback callback = null, 
                                bool forceCreateNew = false, params object[] initArgs) where T : BaseLoader, new()
        {
            return LoaderCache.AutoNew<T>(assetBundlePath, assetPath, loadMode, callback, forceCreateNew, initArgs);
        }

        public BaseLoader()
        {
            RefCount = 0;
        }

        public virtual void Init(string assetBundlePath, string assetPath, LoadMode loadMode, params object[] args)
        {
            this.AssetBundlePath = assetBundlePath ?? "";
            this.AssetPath = assetPath ?? "";
            this.loadMode = loadMode;
            Progress = 0f;
            _initTime = Time.realtimeSinceStartup;
            _uniqueKey = null;

            ResultObject = null;
            Error = ErrorType.None;
            IsCompleted = false;
            IsReadyDisposed = false;

            if(string.IsNullOrEmpty(this.AssetBundlePath) && string.IsNullOrEmpty(this.AssetPath))
            {
            }
            else
            {
                _uniqueKey = GenerateKey(this.AssetBundlePath, this.AssetPath);
            }
            Debuger.Log(LOG_TAG, "_uniqueKey={0}", _uniqueKey);
        }
        protected virtual void Reset()
        {
            this.AssetBundlePath = "";
            this.AssetPath = "";
            Progress = 0f;
            _uniqueKey = null;

            ResultObject = null;
            Error = ErrorType.None;
            IsCompleted = false;
            IsReadyDisposed = false;
        }

        /// <summary>
        /// Gets the unique key. Combined with assetBundlePath and assetPath
        /// </summary>
        public string GetUniqueKey()
        {
            return _uniqueKey;
        }

        /// <summary>
        /// Generates the key. Combined with assetBundlePath and assetPath
        /// </summary>
        public static string GenerateKey(string assetBundlePath, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) && string.IsNullOrEmpty(assetBundlePath))
                return null;
            else if (string.IsNullOrEmpty(assetPath))
                return assetBundlePath;
            else if (string.IsNullOrEmpty(assetBundlePath))
                return assetPath;
            else
                return string.Format("{0}|{1}", assetBundlePath, assetPath);;
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

                this.Log("[BaseLoader:OnFinish]时，准备Disposed {0}", AssetPath);

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
                // TODO: 这里如果原先是Async加载方式，这样调用就会变成Sync，会有问题.
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
                this.LogWarning("[{0}]Too many dipose! {1}, Count: {2}", GetType().Name, this.AssetPath, RefCount);
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
                    Debuger.LogWarning("[{0}:Dispose], No Url: {1}, Cur RefCount: {2}", GetType().Name, AssetPath, RefCount);
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

            //NOTE: 单纯把 ResultObject = null 不一定能释放加载出来的资源，或者释放不及时，比如AssetBundle，具体情况具体分析.
            Reset();
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
