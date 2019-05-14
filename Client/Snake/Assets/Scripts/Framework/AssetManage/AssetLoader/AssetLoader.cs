using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DateTime = System.DateTime;

namespace Framework
{
    /// <summary>
    /// 从AssetBundle中加载Asset，或者从Resources中加载, 也可以从编辑器下载，三种加载方式同时实现的桥接类
    /// </summary>
    public class AssetLoader : BaseLoader
    {
        protected override string LOG_TAG
        {
            get { return "AssetLoader"; }
        }

        private AssetBundleLoader _bundleLoader;


        public Object asset
        {
            get { return ResultObject as Object; }
            protected set { ResultObject = value; }
        }

        public override float Progress
        {
            get
            {
                return _bundleLoader != null ? _bundleLoader.Progress : 0f;
            }
        }

        public static bool IsEditorLoadAsset
        {
            get
            {
#if UNITY_EDITOR
    #if USE_ASSET_BUNDLE
                return false;
    #else
                return true;
    #endif
#else
                return false;
#endif
            }
        }

        public bool IsLoadAssetBundle
        {
            get
            {
#if USE_ASSET_BUNDLE
                return true;
#else
                return false;
#endif
            }
        }


        public override void Init(string assetBundlePath, string assetPath, LoadMode loadMode, params object[] args)
        {
            base.Init(assetBundlePath, assetPath, loadMode, args);

            // 这个MonoBehaviour类需要自己提供.
            AssetManager.instance.StartCoroutine(Start());
        }

        IEnumerator Start()
        {
            Object getAsset = null;

#if UNITY_EDITOR
            if (IsEditorLoadAsset) 
            {

                getAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>( AssetConfig.GetEditorAssetPathRoot() + AssetPath);
                if (getAsset == null)
                {
                    Debuger.LogError("Asset is NULL(from {0} Folder): {1}", AssetConfig.GetEditorAssetPathRoot(), AssetPath);
                }


                OnFinish(getAsset);
            }
            else
#endif
            if (!IsLoadAssetBundle)
            {
                string extension = Path.GetExtension(AssetPath);
                string path = AssetPath.Substring(0, AssetPath.Length - extension.Length); // remove extensions

                // 去掉 "GameAssets/"
                if (path.StartsWith(AssetConfig.GameAssetsFolder))
                    path = path.Replace(AssetConfig.GameAssetsFolder, "");

                getAsset = Resources.Load<Object>(path);
                if (getAsset == null)
                {
                    Debuger.LogError("Asset is NULL(from Resources Folder): {0}", path);
                }
                OnFinish(getAsset);
            }
            else
            {
                _bundleLoader = BaseLoader.Load<AssetBundleLoader>(AssetBundlePath, "", loadMode, null);

                while (!_bundleLoader.IsCompleted)
                {
                    if (IsReadyDisposed) // 中途释放
                    {
                        _bundleLoader.Release();
                        OnFinish(null);
                        yield break;
                    }
                    yield return null;
                }

                if (!_bundleLoader.IsSuccess)
                {
                    Debuger.LogError(LOG_TAG, "Load bundle Failed(Error) when Finished: {0}", AssetBundlePath);
                    _bundleLoader.Release();
                    OnFinish(null);
                    yield break;
                }

                var assetBundle = _bundleLoader.assetBundle;

                var assetName = AssetPath;
                if (!assetBundle.isStreamedSceneAssetBundle)
                {
                    if (loadMode == LoadMode.Sync)
                    {
                        getAsset = assetBundle.LoadAsset(assetName);

                        if(getAsset != null)
                            _bundleLoader.PushLoadedAsset(assetName, getAsset);
                    }
                    else
                    {
                        var request = assetBundle.LoadAssetAsync(assetName);
                        while (!request.isDone)
                        {
                            yield return null;
                        }

                        getAsset = request.asset;

                        if (getAsset != null)
                            _bundleLoader.PushLoadedAsset(assetName, getAsset);
                    }
                }
                else
                {
                    // if it's a scene in asset bundle, do nothing
                    // but set a default Object as the result

                    //TODO:
                    Debuger.LogWarning(LOG_TAG, "Can't load any assets from A scene asset bundle");
                    getAsset = null;
                }

                if (getAsset == null)
                {
                    Debuger.LogError(LOG_TAG, "Asset is NULL(From asset bundle): {0}", AssetPath);
                }
            }

            if (Application.isEditor)
            {
                if (getAsset != null)
                {
                    LoadedAssetDebugger.Create(getAsset.GetType().Name, GetUniqueKey(), getAsset);
                }
            }

            OnFinish(getAsset);
        }

        protected override void DoDispose()
        {
            //TODO: 这里有坑.
            //if (IsFinished)
            {
                if (!IsLoadAssetBundle)
                {
                    Resources.UnloadAsset(asset);
                }
                else
                {
                    //Object.DestroyObject(asset);
                }
            }
            //else
            //{
            //    // 交给加载后，进行检查并卸载资源
            //    // 可能情况TIPS：两个未完成的！会触发上面两次！
            //}

            base.DoDispose();

            if(_bundleLoader != null)
            {
                _bundleLoader.Release(); // 释放Bundle
            }
            _bundleLoader = null;
        }
    }

}
