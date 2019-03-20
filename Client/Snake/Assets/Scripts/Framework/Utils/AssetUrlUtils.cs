using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public sealed class AssetUrlUtils 
    {
        public static string GetPlatformName()
        {
            #if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
            #else
            RuntimePlatform platform = Application.platform;
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    return "Android";
            }
            #endif
        }


        /// <summary>
        /// Gets the streaming asset URL. Used for new WWW(url)
        /// 只读、异步的路径
        /// </summary>
        /// <returns>The streaming asset UR.</returns>
        /// <param name="platformName">Platform name.</param>
        /// <param name="assetURL">Asset URL.</param>
        public static string GetStreamingAssetURL(string assetURL)
        {
            return GetStreamingAssetURL( GetPlatformName(), assetURL );
        }
        public static string GetStreamingAssetURL(string platformName, string assetURL)
        {
            #if UNITY_EDITOR
            return "file:///" + Application.dataPath + "/../Exports/" + platformName + "/" + assetURL;
            #else
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath + "/" + assetURL;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "file:///" + Application.streamingAssetsPath + "/" + assetURL;
            }
            return assetURL;
            #endif
        }


        /// <summary>
        /// Gets the streaming asset path. Used for AssetBundle.LoadFromFile(url)
        /// 只读、同步的路径
        /// </summary>
        /// <returns>The streaming asset path.</returns>
        /// <param name="platformName">Platform name.</param>
        /// <param name="assetURL">Asset URL.</param>
        public static string GetStreamingAssetPath(string assetURL)
        {
            return GetStreamingAssetPath(GetPlatformName(), assetURL);
        }
        public static string GetStreamingAssetPath(string platformName, string assetURL)
        {
            #if UNITY_EDITOR
            return Application.dataPath + "/../Exports/" + platformName + "/" + assetURL;
            #else
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath + "/" + assetURL;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Application.streamingAssetsPath + "/" + assetURL;
            }
            return assetURL;
            #endif
        }


        /// <summary>
        /// Gets the persistent data path where all the patch files exsit.
        /// 可读写的路径，放补丁包.
        /// </summary>
        /// <returns>The persistent data path.</returns>
        /// <param name="assetURL">Asset UR.</param>
        public static string GetPersistentDataUrl(string assetURL)
        {
            return "file:///" + Application.persistentDataPath + "/" + assetURL;
        }

        public static string GetPersistentDataPath(string assetURL)
        {
            return Application.persistentDataPath + "/" + assetURL;
        }
    }
}
