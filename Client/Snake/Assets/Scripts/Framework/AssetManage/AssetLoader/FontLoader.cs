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

        protected override void Init(string url, LoadMode loadMode, params object[] args)
        {
            base.Init(url, loadMode, args);

            _assetLoader = AssetLoader.Load<AssetLoader>(Url, loadMode, (isOk, asset) => 
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
