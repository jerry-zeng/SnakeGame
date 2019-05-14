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
        protected override string LOG_TAG
        {
            get { return "AudioLoader"; }
        }

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

        public override void Init(string assetBundlePath, string assetPath, LoadMode loadMode, params object[] args)
        {
            base.Init(assetBundlePath, assetPath, loadMode, args);

            _assetLoader = BaseLoader.Load<AssetLoader>(assetBundlePath, assetPath, loadMode, (isOk, asset) => 
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

