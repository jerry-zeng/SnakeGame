using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class to cache assetBundle and inner assets.
    /// </summary>
    public class AssetBundleCache
    {
        //asset bundle
        public AssetBundle assetBundle = null;
        //asset bundle里面的资源
        public Dictionary<string, Object> assetMap = new Dictionary<string, Object>();

        public AssetBundleCache( AssetBundle ab )
        {
            assetBundle = ab;
        }

        /// <summary>
        /// Clear all assets loaded from asset bundle and set asset bundle null.
        /// </summary>
        /// <param name="unloadAllLoadedObjects">If set to <c>true</c> unload all loaded objects.</param>
        public void Clear( bool unloadAllLoadedObjects )
        {
            assetMap.Clear();

            if( assetBundle != null )
            {
                assetBundle.Unload( unloadAllLoadedObjects );
                assetBundle = null;
            }
        }

        public void AddAsset( string name, Object asset )
        {
            if( assetMap.ContainsKey( name ) )
                assetMap[name] = asset;
            else
                assetMap.Add( name, asset );
        }
        public Object GetAsset( string assetName )
        {
            Object asset;
            assetMap.TryGetValue( assetName, out asset );
            return asset;
        }
    }
}
