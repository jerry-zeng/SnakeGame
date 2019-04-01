//#define USE_ASSET_BUNDLE_EDITOR  // use asset bundle in editor.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework
{
    /// <summary>
    /// Asset config. 配置一些资源规范，获取资源路径等方法.
    /// </summary>
    public static class AssetConfig 
    {
        public static readonly string AssetBundleExtension = ".assetBundle";

        private static readonly string ManifestFileName = "ArtResources";  //without extension

        //所有AssetBundle的依赖都在这个AssetBundle文件里面(Unity5.x)
        public static string GetManifestFilePath()
        {
            return GetAssetFullPath(ManifestFileName);
        }

        public static string GetManifestFileName()
        {
            return GetPlatformName();// ManifestFileName;
        }

        public static string GetPlatformName()
        {
            #if UNITY_EDITOR
                return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
            #else
            if( Application.platform == RuntimePlatform.Android )
            {
                return "Android";
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                return "iOS";
            }
            else
            {
                return "StandAlone";
            }
            #endif
        }

        /// <summary>
        /// Gets the writable path, ends with '/'.
        /// </summary>
        /// <returns>The writable path.</returns>
        public static string GetWritablePath()
        {
            return Application.persistentDataPath + "/";
        }

        /// <summary>
        /// Gets the streaming assets path, ends with '/'.
        /// </summary>
        /// <returns>The streaming assets path.</returns>
        public static string GetStreamingAssetsPath()
        {
            return Application.streamingAssetsPath + "/";
        }

        /// <summary>
        /// Gets the update data path. Used for AssetBundle.LoadFromFile(path) or AssetBundle.LoadFromFileAsync(path)
        /// </summary>
        /// <returns>The update data path.</returns>
        /// <param name="relativePath">Relative path.</param>
        public static string GetUpdateDataPath(string relativePath)
        {
            return Path.Combine( GetWritablePath(), relativePath );
        }

        /// <summary>
        /// Gets the local data path. Used for new WWW(url) or new UnityWebRequest(url) or UnityWebRequestAssetBundle.GetAssetBundle(url).
        /// </summary>
        /// <returns>The local data path.</returns>
        /// <param name="relativePath">Relative path.</param>
        public static string GetLocalDataPath(string relativePath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Path.Combine( GetStreamingAssetsPath(), relativePath );
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Path.Combine( GetStreamingAssetsPath(), relativePath );
            }

            else if( Application.platform == RuntimePlatform.WindowsPlayer )  //win standalone, enable to place assets out of default path
            {
                return Path.Combine( GetStreamingAssetsPath(), relativePath );
            }
            else if( Application.platform == RuntimePlatform.OSXPlayer )    //mac standalone, enable to place assets out of default path
            {
                return Path.Combine( GetStreamingAssetsPath(), relativePath );
            }
            else
            {
                return Path.Combine( GetStreamingAssetsPath(), relativePath );
            }
        }


        /// <summary>
        /// Gets the local asset full path. The search order is: UpdateDataPath -> LocalDataPath (-> EditorLocalDataPath if in editor)
        /// #If ( UNITY_EDITOR && !USE_ASSET_BUNDLE_EDITOR ), return a non-AssetBundle object path.
        /// </summary>
        /// <returns>The asset full path.</returns>
        /// <param name="relativePath">Relative path.</param>
        public static string GetAssetFullPath( string relativePath)
        {
            string fullPath;

            fullPath = GetUpdateDataPath(relativePath);
            if( File.Exists(fullPath) )
                return fullPath;

            fullPath = GetLocalDataPath(relativePath);
            if( File.Exists(fullPath) )
                return fullPath;

#if UNITY_EDITOR
            fullPath = GetEditorLocalDataPath(relativePath);
            if( File.Exists(fullPath) )
                return fullPath;
#endif

            return "";
        }



#if UNITY_EDITOR
        /// <summary>
        /// Gets the editor local data path, only used in editor.
        /// </summary>
        /// <returns>The editor local data path.</returns>
        /// <param name="relativePath">Relative path.</param>
        public static string GetEditorLocalDataPath(string relativePath)
        {
            #if USE_ASSET_BUNDLE_EDITOR
            return GetEditorAssetBundlePathRoot() + relativePath;
            #else
            return GetEditorAssetPathRoot() + relativePath;
            #endif
        }

        /// <summary>
        /// Gets the client asset bundle path root, ends with '/'.
        /// We needn't place asset bundles under Application.dataPath or Application.streamingAssetsPath.
        /// Placing them out of Application.dataPath is more convienient to compile project.
        /// </summary>
        /// <returns>The client asset bundle path root.</returns>
        public static string GetEditorAssetBundlePathRoot()
        {
            return Application.dataPath + "/../Exports/" + GetPlatformName() + "/";
        }

        /// <summary>
        /// Gets the client assets path root, ends with '/'. 
        /// It's neither Resources/ nor StreamingAssets/.
        /// </summary>
        /// <returns>The client data path root.</returns>
        public static string GetEditorAssetPathRoot()
        {
            return Application.dataPath + "/GameAssets/";
        }
#endif

    }
}
