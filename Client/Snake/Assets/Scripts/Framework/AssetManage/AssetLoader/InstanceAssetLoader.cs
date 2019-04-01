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

        public override void Init(string url, LoadMode loadMode, params object[] args)
        {
            base.Init(url, loadMode, args);

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
                    Debuger.LogError(LOG_TAG, "[InstanceAssetLoader]Error on assetfilebridge loaded... {0}", url);
                    return;
                }

                Object instance = Object.Instantiate((Object)asset);

                if (Application.isEditor)
                {
                    XXLoadedAssetDebugger.Create("AssetCopy", url, instance);
                }

                OnFinish(instance);
            };

            _assetLoader = AssetLoader.Load<AssetLoader>(url, loadMode, callback, true);
        }


        protected override void DoDispose()
        {
            base.DoDispose();

            _assetLoader.Release();
            _assetLoader = null;

            if (ResultObject != null)
            {
                Object.Destroy((Object)ResultObject);
                ResultObject = null;
            }
        }

    }
}
