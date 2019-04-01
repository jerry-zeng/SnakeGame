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
        public AssetBundle assetBundle
        {
            get { return ResultObject as AssetBundle; }
            protected set { ResultObject = value; }
        }

        private string RelativePath;
        //private AssetBundleManifest _manifest;
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

            _mainAssetBundle = AssetBundle.LoadFromFile( AssetConfig.GetManifestFilePath() );
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


        public override void Init(string url, LoadMode loadMode, params object[] args)
        {
            PreLoadManifest();

            RelativePath = url;
            url = url.ToString();

            base.Init(url, loadMode, args);

            //AssetManager.Instance.StartCoroutine( Start() );
        }

        IEnumerator Start()
        {
            string abPath = Url;

            //load dependence
            var deps = _assetBundleManifest.GetAllDependencies(abPath);

            _depLoaders = new AssetBundleLoader[deps.Length];
            for (int i = 0; i < deps.Length; i++)
            {
                _depLoaders[i] = AssetBundleLoader.Load<AssetBundleLoader>(deps[i], loadMode, null);
            }

            for (var l = 0; l < _depLoaders.Length; l++)
            {
                while (!_depLoaders[l].IsCompleted)
                {
                    yield return null;
                }
            }

            // load self
            AssetBundle assetBundle = null;
            if( loadMode == LoadMode.Async )
            {
                var request = AssetBundle.LoadFromFileAsync(abPath);
                yield return request;

                assetBundle = request.assetBundle;
            }
            else
            {
                assetBundle = AssetBundle.LoadFromFile(abPath);
            }

            //_manifest = assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

            OnFinish(assetBundle);
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

        protected override void DoDispose()
        {
            base.DoDispose();

            //_manifest = null;

            if( _depLoaders != null )
            {
                foreach( var depLoader in _depLoaders )
                {
                    depLoader.Release();
                }
            }
            _depLoaders = null;

            if( _loadedAssets != null )
            {
                foreach( var kvs in _loadedAssets )
                {
                    Object.DestroyImmediate(kvs.Value, true);
                }
            }
            _loadedAssets.Clear();
        }

    }
}
