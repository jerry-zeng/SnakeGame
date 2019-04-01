using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Texture loader. 借助AssetLoader加载贴图。
    /// </summary>
    public class TextureLoader : BaseLoader
    {
        private AssetLoader _assetLoader;


        public Texture texture
        {
            get { return ResultObject as Texture; }
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

            _assetLoader = AssetLoader.Load<AssetLoader>(Url, loadMode, (isOk, asset) => 
            { 
                OnFinish(asset);

                if (isOk)
                {
                    Texture2D tex = asset as Texture2D;

                    string format = tex != null ? tex.format.ToString() : "";
                    Desc = string.Format("{0}*{1}-{2}-{3}", tex.width, tex.height, tex.width * tex.height, format);
                }
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

