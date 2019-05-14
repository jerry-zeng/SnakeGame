using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///  借助AssetLoader加载资源并创建一个实例
    /// </summary>
    public class InstanceAssetLoader : BaseLoader
    {
        protected override string LOG_TAG
        {
            get { return "InstanceAssetLoader"; }
        }

        private AssetLoader _assetLoader;


        public Object instanceAsset
        {
            get { return ResultObject as Object; }
            protected set { ResultObject = value; }
        }

        public override float Progress
        {
            get 
            { 
                return _assetLoader != null? _assetLoader.Progress : 0f; 
            }
        }

        public override void Init(string assetBundlePath, string assetPath, LoadMode loadMode, params object[] args)
        {
            base.Init(assetBundlePath, assetPath, loadMode, args);

            LoaderCallback callback = (isOk, asset) =>
            {
                if( IsReadyDisposed ) // 中途释放
                {
                    OnFinish(null);
                    return;
                }

                if( !isOk )
                {
                    OnFinish(null);
                    Debuger.LogError(LOG_TAG, "Error on AssetLoader loaded... {0}|{1}", assetBundlePath, assetPath);
                    return;
                }

                Object prefab = asset as Object;
                Object instance = Object.Instantiate(prefab);
                instance.name = prefab.name;

                if (Application.isEditor)
                {
                    LoadedAssetDebugger.Create("InstanceAsset", GetUniqueKey(), instance);
                }

                OnFinish(instance);
            };

            _assetLoader = BaseLoader.Load<AssetLoader>(assetBundlePath, assetPath, loadMode, callback, true);
        }


        protected override void DoDispose()
        {
            if (instanceAsset != null)
            {
                Object.Destroy(instanceAsset);
                instanceAsset = null;
            }

            base.DoDispose();

            _assetLoader.Release();
            _assetLoader = null;
        }

    }
}
