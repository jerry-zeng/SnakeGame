using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{
    public sealed class LoaderCache 
    {
        private const string LOG_TAG = "LoaderCache";

        /// <summary>
        /// Loader延迟Dispose
        /// </summary>
        private const float LoaderDisposeTime = 0f;

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
        private static float _lastGcTime = 0f;

        private static readonly Dictionary<Type, Dictionary<string, BaseLoader>> _loadersCaches =
            new Dictionary<Type, Dictionary<string, BaseLoader>>();

        /// <summary>
        /// 缓存起来要删掉的，供DoGarbageCollect函数用, 避免重复的new List
        /// </summary>
        private static readonly List<BaseLoader> CacheLoaderToRemoveFromUnUsed =
            new List<BaseLoader>();

        /// <summary>
        /// 进行垃圾回收
        /// </summary>
        internal static readonly Dictionary<BaseLoader, float> UnuseLoaders =
            new Dictionary<BaseLoader, float>();

        

        private static float GetCurrentTime()
        {
            return Time.time;
        }

        private static int GetCount<T>()
        {
            return GetTypeDict(typeof(T)).Count;
        }

        private static Dictionary<string, BaseLoader> GetTypeDict(Type type)
        {
            Dictionary<string, BaseLoader> typesDict;
            if (!_loadersCaches.TryGetValue(type, out typesDict))
            {
                typesDict = new Dictionary<string, BaseLoader>();
                _loadersCaches.Add( type, typesDict );
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

        public static void AddUnusedLoader(BaseLoader loader)
        {
            if( loader == null ) return;

            UnuseLoaders[loader] = GetCurrentTime();
        }

        public static bool RemoveLoader(BaseLoader loader)
        {
            if( loader == null ) return false;

            var typeDict = GetTypeDict( loader.GetType() );
            return typeDict.Remove(loader.Url);
        }

        /// <summary>
        /// 统一的对象工厂
        /// </summary>

        public static T AutoNew<T>(string url, LoadMode loadMode, LoaderCallback callback, bool forceCreateNew = false,
                                   params object[] initArgs) where T : BaseLoader, new()
        {
            if (string.IsNullOrEmpty(url))
            {
                Debuger.LogError(LOG_TAG, "[{0}:AutoNew] url为空", typeof(T));
            }

            Dictionary<string, BaseLoader> typesDict = GetTypeDict(typeof(T));
            BaseLoader loader;

            if ( forceCreateNew || !typesDict.TryGetValue(url, out loader) )
            {
                loader = new T();

                if (!forceCreateNew)
                    typesDict[url] = loader;

                loader.IsForceNew = forceCreateNew;
                loader.Init(url, loadMode, initArgs);

                if (Application.isEditor)
                {
                    BaseLoaderDebugger.Create(typeof(T).Name, url, loader);
                }
            }
            else
            {
                if (loader.RefCount < 0)
                {
                    //loader.IsDisposed = false;  // 转死回生的可能
                    Debuger.LogError(LOG_TAG, "Error RefCount!");
                }
            }

            loader.AddRef();

            // RefCount++了，重新激活，在队列中准备清理的Loader
            if (UnuseLoaders.ContainsKey(loader))
            {
                UnuseLoaders.Remove(loader);
                loader.Revive();
            }

            loader.AddCallback(callback);

            return loader as T;
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
                BaseLoader loader = CacheLoaderToRemoveFromUnUsed[i];
                CacheLoaderToRemoveFromUnUsed.RemoveAt(i);

                UnuseLoaders.Remove(loader);

                loader.Dispose();
            }

            if (CacheLoaderToRemoveFromUnUsed.Count > 0)
            {
                Debuger.LogError(LOG_TAG, "[DoGarbageCollect] CacheLoaderToRemoveFromUnUsed muse be empty!!");
            }
        }

        /// <summary>
        /// Clears the loader caches.
        /// </summary>
        public static void ClearCaches()
        {
            _loadersCaches.Clear();
        }

    }
}
