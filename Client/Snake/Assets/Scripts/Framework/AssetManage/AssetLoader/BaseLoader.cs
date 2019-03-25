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

#region 垃圾回收 Garbage Collect
        /// <summary>
        /// Loader延迟Dispose
        /// </summary>
        protected const float LoaderDisposeTime = 0f;

        /// <summary>
        /// 间隔多少秒做一次GC(在AutoNew时)
        /// </summary>
        public static float GcTimeInterval
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor)
                    return 1f;

                return Debug.isDebugBuild ? 5f : 10f;
            }
        }

        /// <summary>
        /// 上次做GC的时间
        /// </summary>
        protected static float _lastGcTime = 0f;

        protected static readonly Dictionary<Type, Dictionary<string, BaseLoader>> _loadersPool =
            new Dictionary<Type, Dictionary<string, BaseLoader>>();

        /// <summary>
        /// 缓存起来要删掉的，供DoGarbageCollect函数用, 避免重复的new List
        /// </summary>
        protected static readonly List<BaseLoader> CacheLoaderToRemoveFromUnUsed =
            new List<BaseLoader>();

        /// <summary>
        /// 进行垃圾回收
        /// </summary>
        internal static readonly Dictionary<BaseLoader, float> UnuseLoaders =
            new Dictionary<BaseLoader, float>();

#endregion


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
        protected bool IsReadyDisposed { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsSuccess
        {
            get { return !IsError && ResultObject != null && !IsReadyDisposed; }
        }

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


        protected readonly List<LoaderCallback> _callbacks = new List<LoaderCallback>();
        protected float _initTime = -1f;
        protected float _finishTime = -1f;


#region Static Functions
        protected static float GetCurrentTime()
        {
            return Time.time;
        }

        protected static int GetCount<T>()
        {
            return GetTypeDict(typeof(T)).Count;
        }

        protected static Dictionary<string, BaseLoader> GetTypeDict(Type type)
        {
            Dictionary<string, BaseLoader> typesDict;
            if (!_loadersPool.TryGetValue(type, out typesDict))
            {
                typesDict = new Dictionary<string, BaseLoader>();
                _loadersPool.Add( type, typesDict );
            }
            return typesDict;
        }

        public static int GetRefCount<T>(string url)
        {
            Dictionary<string, BaseLoader> dict = GetTypeDict(typeof(T));
            BaseLoader loader;
            if (dict.TryGetValue(url, out loader))
            {
                return loader.RefCount;
            }
            return 0;
        }

        /// <summary>
        /// 统一的对象工厂
        /// </summary>

        protected static T AutoNew<T>(string url, LoadMode loadMode, LoaderCallback callback, params object[] initArgs) where T : BaseLoader, new()
        {
            if (string.IsNullOrEmpty(url))
            {
                Debuger.LogError(LOG_TAG, "[{0}:AutoNew] url为空", typeof(T));
            }

            Dictionary<string, BaseLoader> typesDict = GetTypeDict(typeof(T));
            BaseLoader loader;

            if ( !typesDict.TryGetValue(url, out loader) )
            {
                loader = new T();
                typesDict[url] = loader;

                loader.Init(url, loadMode, initArgs);
            }
            else
            {
                if (loader.RefCount < 0)
                {
                    //loader.IsDisposed = false;  // 转死回生的可能
                    Debuger.LogError(LOG_TAG, "Error RefCount!");
                }
            }

            loader.RefCount++;

            // RefCount++了，重新激活，在队列中准备清理的Loader
            if (UnuseLoaders.ContainsKey(loader))
            {
                UnuseLoaders.Remove(loader);
                loader.Revive();
            }

            loader.AddCallback(callback);

            return loader as T;
        }

        protected static void ReturnToPool<T>(T loader) where T : BaseLoader
        {
            if( loader == null ) return;

            Dictionary<string, BaseLoader> typesDict = GetTypeDict(typeof(T));
            typesDict[loader.Url] = loader;
        }

        public static T Load<T>(string url, LoadMode loadMode = LoadMode.Async, LoaderCallback callback = null) where T : BaseLoader, new()
        {
            return AutoNew<T>(url, loadMode, callback);
        }

        /// <summary>
        /// 是否进行垃圾收集
        /// </summary>
        public static void CheckGcCollect()
        {
            float curTime = GetCurrentTime();
            if( curTime - _lastGcTime >= GcTimeInterval)
            {
                DoGarbageCollect(curTime);
                _lastGcTime = curTime;
            }
        }

        /// <summary>
        /// 进行垃圾回收
        /// </summary>
        internal static void DoGarbageCollect(float curTime)
        {
            foreach (var kv in UnuseLoaders)
            {
                if (curTime - kv.Value >= LoaderDisposeTime)
                {
                    CacheLoaderToRemoveFromUnUsed.Add(kv.Key);
                }
            }

            for (var i = CacheLoaderToRemoveFromUnUsed.Count - 1; i >= 0; i--)
            {
                try
                {
                    BaseLoader loader = CacheLoaderToRemoveFromUnUsed[i];
                    CacheLoaderToRemoveFromUnUsed.RemoveAt(i);

                    UnuseLoaders.Remove(loader);

                    loader.Dispose();
                    ReturnToPool(loader);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, e.Message);
                }
            }

            if (CacheLoaderToRemoveFromUnUsed.Count > 0)
            {
                Debuger.LogError(LOG_TAG, "[DoGarbageCollect] CacheLoaderToRemoveFromUnUsed muse be empty!!");
            }
        }

        /// <summary>
        /// Clears the loader pool.
        /// </summary>
        public static void ClearPool()
        {
            _loadersPool.Clear();
        }
#endregion


        protected BaseLoader()
        {
            RefCount = 0;
        }

        protected virtual void Init(string url, LoadMode loadMode, params object[] args)
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

        /// <summary>
        /// 复活
        /// </summary>
        protected virtual void Revive()
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
        protected void AddCallback(LoaderCallback callback)
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
                UnuseLoaders[this] = GetCurrentTime();

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
        protected void Dispose()
        {
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
