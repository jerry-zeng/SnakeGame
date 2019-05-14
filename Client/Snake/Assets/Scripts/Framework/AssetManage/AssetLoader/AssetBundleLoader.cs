using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{
    /// <summary>
    /// Asset bundle loader. 加载AssetBundle.
    /// </summary>
    public class AssetBundleLoader : BaseLoader
    {
        protected override string LOG_TAG
        {
            get{ return "AssetBundleLoader"; }
        }

        public AssetBundle assetBundle
        {
            get { return ResultObject as AssetBundle; }
            protected set { ResultObject = value; }
        }

        private AssetBundleLoader[] _depLoaders;
        private Dictionary<string, Object> _loadedAssets;


        #region Manifest
        private static bool _hasPreloadAssetBundleManifest = false;
        private static AssetBundle _mainAssetBundle;
        private static AssetBundleManifest _assetBundleManifest;

        public static void PreLoadManifest()
        {
            if (_hasPreloadAssetBundleManifest)
                return;

            _hasPreloadAssetBundleManifest = true;

            _mainAssetBundle = AssetBundle.LoadFromFile( AssetConfig.GetManifestFilePath() );  //好处是不管ab放在哪里，都可以统一用一个接口.
            _assetBundleManifest = _mainAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        }

        public static void ClearManifest()
        {
            _assetBundleManifest = null;
            _mainAssetBundle.Unload(true);
            _mainAssetBundle = null;
            _hasPreloadAssetBundleManifest = false;
        }
        #endregion


        public override void Init(string assetBundlePath, string assetPath, LoadMode loadMode, params object[] args)
        {
            PreLoadManifest();

            base.Init(assetBundlePath, assetPath, loadMode, args);

            // 这个MonoBehaviour类需要自己提供.
            AssetManager.instance.StartCoroutine( Start() );
        }

        IEnumerator Start()
        {
            string abPath = AssetBundlePath;

            //load dependence
            var deps = _assetBundleManifest.GetAllDependencies(abPath);

            _depLoaders = new AssetBundleLoader[deps.Length];
            for (int i = 0; i < deps.Length; i++)
            {
                _depLoaders[i] = BaseLoader.Load<AssetBundleLoader>(deps[i], "", loadMode, null);
            }

            for (var l = 0; l < _depLoaders.Length; l++)
            {
                while (!_depLoaders[l].IsCompleted)
                {
                    yield return null;
                }
            }

            // load self
            AssetBundle ab = null;
            if( loadMode == LoadMode.Async )
            {
                var request = AssetBundle.LoadFromFileAsync(abPath);
                while (!request.isDone)
                {
                    yield return null;
                }

                ab = request.assetBundle;
            }
            else
            {
                ab = AssetBundle.LoadFromFile(abPath);
            }

            if(ab == null)
            {
                Debuger.LogError(LOG_TAG, "Load assetBundle from {0} failed", abPath);
            }

            OnFinish(ab);
        }

        /// 原以为，每次都通过getter取一次assetBundle会有序列化解压问题，会慢一点，后用AddWatch调试过，发现如果把.assetBundle放到Dictionary里缓存，查询会更慢
        /// 因为，估计.assetBundle是一个纯Getter，没有做序列化问题。（不保证.mainAsset）
        public void PushLoadedAsset(string assetName, Object asset)
        {
            if (_loadedAssets == null)
                _loadedAssets = new Dictionary<string, Object>();
            
            _loadedAssets[assetName] = asset;
        }
        public void PushLoadedAssets(Object[] assets)
        {
            foreach( Object obj in assets )
                PushLoadedAsset(obj.name, obj);
        }

        public Object GetAsset(string assetName)
        {
            if (_loadedAssets == null)
                _loadedAssets = new Dictionary<string, Object>();

            Object obj;
            if( _loadedAssets.TryGetValue(assetName, out obj) ){
                return obj;
            }
            else{
                if( assetBundle != null ){
                    obj = assetBundle.LoadAsset(assetName);
                    PushLoadedAsset(assetName, obj);
                }
                return obj;
            }
        }

        public void GetAssetAsync(string assetName, LoadAssetCallback callback)
        {
            if (_loadedAssets == null)
                _loadedAssets = new Dictionary<string, Object>();

            // 这个MonoBehaviour类需要自己提供.
            AssetManager.instance.StartCoroutine(DoGetAssetAsync(assetName, callback));
        }
        protected IEnumerator DoGetAssetAsync(string assetName, LoadAssetCallback callback)
        {
            Object obj;
            if (_loadedAssets.TryGetValue(assetName, out obj))
            {
                yield return null;  //隔一帧.
            }
            else
            {
                if (assetBundle != null)
                {
                    var request = assetBundle.LoadAssetAsync(assetName);
                    while (!request.isDone)
                    {
                        yield return null;
                    }

                    obj = request.asset;

                    if(obj != null)
                        PushLoadedAsset(assetName, obj);
                }
            }

            if (callback != null)
                callback.Invoke(obj);
        }

        protected override void DoDispose()
        {
            // 释放ab中加载出来的资源
            if (_loadedAssets != null)
            {
                foreach (var kvs in _loadedAssets)
                {
                    Object.DestroyImmediate(kvs.Value, true);
                }
                _loadedAssets.Clear();
            }
            _loadedAssets = null;

            // 释放 assetBundle.
            if (assetBundle != null)
                assetBundle.Unload(true);

            base.DoDispose();

            if( _depLoaders != null )
            {
                foreach( var depLoader in _depLoaders )
                {
                    depLoader.Release();
                }
            }
            _depLoaders = null;
        }

    }
}
