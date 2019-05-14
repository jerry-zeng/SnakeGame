using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Font loader. 借助AssetLoader加载字体。
    /// </summary>
    public class FontLoader : BaseLoader
    {
        protected override string LOG_TAG
        {
            get { return "FontLoader"; }
        }

        private AssetLoader _assetLoader;


        public Font font
        {
            get { return ResultObject as Font; }
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

            _assetLoader = AssetLoader.Load<AssetLoader>(assetBundlePath, assetPath, loadMode, (isOk, asset) => 
            { 
                OnFinish(asset); 
            });
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            _assetLoader.Release();
            _assetLoader = null;
        }
    }
}
