using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Audio loader. 借助AssetLoader加载音效。
    /// </summary>
    public class AudioLoader : BaseLoader
    {
        private AssetLoader _assetLoader;


        public AudioClip audioClip
        {
            get { return ResultObject as AudioClip; }
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

