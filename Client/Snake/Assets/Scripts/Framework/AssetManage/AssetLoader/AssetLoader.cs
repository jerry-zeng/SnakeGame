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
                return _bundleLoader != null? _bundleLoader.Progress : 0f;
            }
        }

        public static bool IsEditorLoadAsset
        {
            get
            {
                return Application.isEditor;
            }
        }
        public bool IsLoadAssetBundle
        {
            get
            {
                #if UNITY_EDITOR && !USE_ASSET_BUNDLE_EDITOR
                return false;
                #else
                return true;
                #endif
            }
        }


        public override void Init(string url, LoadMode loadMode, params object[] args)
        {
            // TODO: 添加扩展名
            //if (!IsEditorLoadAsset)
            //    url = url + AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleExt);

            base.Init(url, loadMode, args);

            //TODO
            //AssetManager.Instance.StartCoroutine( Start() );
        }

        IEnumerator Start()
        {
            string path = Url;
            Object getAsset = null;

            if (IsEditorLoadAsset) 
            {
                #if UNITY_EDITOR
                getAsset = UnityEditor.AssetDatabase.LoadAssetAtPath( AssetConfig.GetEditorAssetPathRoot() + "/" + path, typeof(UnityEngine.Object));
                if (getAsset == null)
                {
                    Debuger.LogError("Asset is NULL(from {0} Folder): {1}", AssetConfig.GetEditorAssetPathRoot(), path);
                }
                #else
                Debuger.LogError("`IsEditorLoadAsset` is used in Unity Editor only");
                #endif

                OnFinish(getAsset);
            }
            else if (!IsLoadAssetBundle)
            {
                string extension = Path.GetExtension(path);
                path = path.Substring(0, path.Length - extension.Length); // remove extensions

                getAsset = Resources.Load<Object>(path);
                if (getAsset == null)
                {
                    Debuger.LogError("Asset is NULL(from Resources Folder): {0}", path);
                }
                OnFinish(getAsset);
            }
            else
            {
                _bundleLoader = AssetBundleLoader.Load<AssetBundleLoader>(path, loadMode, null);

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
                    Debuger.LogError("[AssetFileLoader]Load BundleLoader Failed(Error) when Finished: {0}", path);
                    _bundleLoader.Release();
                    OnFinish(null);
                    yield break;
                }

                var assetBundle = _bundleLoader.assetBundle;

                var assetName = Path.GetFileNameWithoutExtension(Url).ToLower();
                if (!assetBundle.isStreamedSceneAssetBundle)
                {
                    if (loadMode == LoadMode.Sync)
                    {
                        getAsset = assetBundle.LoadAsset(assetName);
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
                        _bundleLoader.PushLoadedAsset(assetName, getAsset);
                    }
                }
                else
                {
                    // if it's a scene in asset bundle, did nothing
                    // but set a fault Object the result

                    //TODO:
                    getAsset = null;
                }

                if (getAsset == null)
                {
                    Debuger.LogError("Asset is NULL: {0}", path);
                }
            }

            if (Application.isEditor)
            {
                if (getAsset != null)
                    XXLoadedAssetDebugger.Create(getAsset.GetType().Name, Url, getAsset as Object);

                // 编辑器环境下，如果遇到GameObject，对Shader进行Fix
                if (getAsset is GameObject)
                {
                    var go = getAsset as GameObject;
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    {
                        RefreshMaterialsShaders(r);
                    }
                }
            }

            if (getAsset != null)
            {
                // 更名~ 注明来源asset bundle 带有类型
                getAsset.name = string.Format("{0}~{1}", getAsset, Url);
            }

            OnFinish(getAsset);
        }

        /// <summary>
        /// 编辑器模式下，对指定GameObject刷新一下Material
        /// </summary>
        public static void RefreshMaterialsShaders(Renderer renderer)
        {
            if (renderer.sharedMaterials != null)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.shader != null)
                    {
                        mat.shader = Shader.Find(mat.shader.name);
                    }
                }
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            _bundleLoader.Release(); // 释放Bundle(WebStream)
            _bundleLoader = null;

            //TODO: 这里有坑.
            //if (IsFinished)
            {
                if (!IsLoadAssetBundle)
                {
                    Resources.UnloadAsset(ResultObject as Object);
                }
                else
                {
                    //Object.DestroyObject(ResultObject as UnityEngine.Object);
                }
            }
            //else
            //{
            //    // 交给加载后，进行检查并卸载资源
            //    // 可能情况TIPS：两个未完成的！会触发上面两次！
            //}
        }
    }

}
