using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework
{
    public delegate void OnLoadAssetBundle(AssetBundle assetBundle);
    public delegate void OnLoadAsset(Object asset);
    public delegate void OnLoadInstanceAsset(Object asset);

    public sealed class AssetManager : MonoBehaviour
    {
        private static AssetManager _instance;
        public static AssetManager instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            _instance = this;
        }

        bool hasLoadedShaders = false;
        AssetBundleLoader shaderBundleLoader;
        Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        private void Update()
        {
            // loader gc
            LoaderCache.CheckGcCollect();
        }

        #region Shader
        public void LoadShaders()
        {
            if (hasLoadedShaders)
                return;

            shaderBundleLoader = LoadAssetBundleAsync("shaders/shaders", OnLoadedShaders);
        }
        void OnLoadedShaders(AssetBundle assetBundle)
        {
            if(assetBundle == null)
            {
                this.LogError("Can't load shader assetBundle!!!");
                return;
            }

            foreach(Shader shader in assetBundle.LoadAllAssets<Shader>())
            {
                if (_shaders.ContainsKey(shader.name))
                    this.LogError("Duplicate shader {0}", shader.name);
                _shaders[shader.name] = shader;
            }

            hasLoadedShaders = true;
        }

        public Shader GetShader(string name)
        {
            Shader shader = null;
            if(_shaders.TryGetValue(name, out shader))
            {
                return shader;
            }

            shader = Shader.Find(name);

            return shader;
        }
        #endregion

        /// <summary>
        /// Loads the asset bundle async.
        /// 'assetBundlePath'，只需要填相对路径，不用填后缀，如 UI/Panel/MenuPanel，除非ab不是按AssetConfig里面配置的后缀来设置的.
        /// </summary>

        public AssetBundleLoader LoadAssetBundleAsync(string assetBundleRelativePath, OnLoadAssetBundle callback)
        {
            string fullRelativePath = AssetConfig.GetAssetBundleFullRelativePath(assetBundleRelativePath);
            string assetBundleFullPath = AssetConfig.GetGameAssetFullPath(fullRelativePath);

            LoaderCallback internelHandler = (bool isOk, object resultObject) =>
            {
                if (isOk)
                {
                    callback.Invoke(resultObject as AssetBundle);
                }
                else
                {
                    callback.Invoke(null);
                }
            };
            return BaseLoader.Load<AssetBundleLoader>(assetBundleFullPath, "", callback: internelHandler);
        }

        /// <summary>
        /// Loads the asset bundle async.
        /// 'assetBundlePath'，只需要填相对路径，不用填后缀，如 UI/Panel/MenuPanel，除非ab不是按AssetConfig里面配置的后缀来设置的.
        /// 'assetName'，只需要填相对路径，需要加后缀，如 UI/Panel/MenuPanel.prefab
        /// </summary>
        public AssetLoader LoadAssetAsync(string assetBundleRelativePath, string assetName, OnLoadAsset callback)
        {
            // 如果是加载ab中的资源，需要填完成的路径，这跟 BuildAssetBundleOptions 有关，参考BundleBuilder.BuildAssetBundle()
            if (!string.IsNullOrEmpty(assetBundleRelativePath))
            {
                assetName = AssetConfig.GetAssetFullPathInAB(assetName);
            }

            string fullRelativePath = AssetConfig.GetAssetBundleFullRelativePath(assetBundleRelativePath);
            string assetBundleFullPath = AssetConfig.GetGameAssetFullPath(fullRelativePath);

            LoaderCallback internelHandler = (bool isOk, object resultObject) =>
            {
                if (isOk)
                {
                    callback.Invoke(resultObject as Object);
                }
                else
                {
                    callback.Invoke(null);
                }
            };
            return BaseLoader.Load<AssetLoader>(assetBundleFullPath, assetName, callback: internelHandler);
        }
        public AssetLoader LoadAssetAsync(string assetName, OnLoadAsset callback)
        {
            return LoadAssetAsync("", assetName, callback);
        }

        /// <summary>
        /// Loads the instance asset async.
        /// 'assetBundlePath'，只需要填相对路径，不用加后缀，如 UI/Panel/MenuPanel，除非ab不是按AssetConfig里面配置的后缀来设置的.
        /// 'assetName'，只需要填相对路径，需要加后缀，如 UI/Panel/MenuPanel.prefab
        /// </summary>
        public InstanceAssetLoader LoadInstanceAssetAsync(string assetBundleRelativePath, string assetName, OnLoadInstanceAsset callback)
        {
            // 如果是加载ab中的资源，需要填完成的路径，这跟 BuildAssetBundleOptions 有关，参考BundleBuilder.BuildAssetBundle()
            if (!string.IsNullOrEmpty(assetBundleRelativePath))
            {
                assetName = AssetConfig.GetAssetFullPathInAB(assetName);
            }

            string fullRelativePath = AssetConfig.GetAssetBundleFullRelativePath(assetBundleRelativePath);
            string assetBundleFullPath = AssetConfig.GetGameAssetFullPath(fullRelativePath);

            LoaderCallback internelHandler = (bool isOk, object resultObject) =>
            {
                if (isOk)
                {
                    callback.Invoke(resultObject as Object);
                }
                else
                {
                    callback.Invoke(null);
                }
            };
            return BaseLoader.Load<InstanceAssetLoader>(assetBundleFullPath, assetName, callback: internelHandler);
        }
        public InstanceAssetLoader LoadInstanceAssetAsync(string assetName, OnLoadInstanceAsset callback)
        {
            return LoadInstanceAssetAsync("", assetName, callback);
        }

        // 资源不用时，释放BaseLoader的引用计数，有加载资源就必须调用该方法.
        public void ReleaseLoader(BaseLoader loader)
        {
            if (loader != null)
                loader.Release();
        }

        // 清除所有加载器，慎用！
        public void Clear()
        {
            LoaderCache.ClearCaches();
        }

    }
}
