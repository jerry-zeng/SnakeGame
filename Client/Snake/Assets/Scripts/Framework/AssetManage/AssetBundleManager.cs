using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Object = UnityEngine.Object;

namespace Framework
{
    public delegate void LoadAssetCallback( Object loadObject );
    public delegate void LoadAllAssetCallback( Object[] loadObjects );

    public sealed class AssetBundleManager : MonoSingleton<AssetBundleManager> 
    {
/*
        private Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        private Dictionary<string, string[]> _dependences = new Dictionary<string, string[]>();

        private List<string> _downLoadHosts = new List<string>();

        private Dictionary<string, AssetBundleCache> _assetBundleCacheMap = new Dictionary<string, AssetBundleCache>();
        private Dictionary<string, FrameTask> _loadingTaskMap = new Dictionary<string, FrameTask>();
        private Dictionary<string, AssetBundleLoader> _loaderMap = new Dictionary<string, AssetBundleLoader>();


        protected override void InitSingleton()
        {
            base.InitSingleton();


        }

        public void Clear()
        {
            _loadingTaskMap.Clear();
            _assetBundleCacheMap.Clear();

            foreach( var a in _loaderMap )
            {
                Debug.Log( a.Key + ":" + a.Value.RefCount );
                a.Value.Clear();
            }
            _loaderMap.Clear();
        }


        private void InitShaders( Object obj )
        {
            _shaders.Clear();

            if( obj != null )
            {
                AssetBundle assetBundle = (AssetBundle)obj;

                Shader[] shaders = assetBundle.LoadAllAssets<Shader>();
                foreach( var shader in shaders )
                {
                    _shaders.Add( shader.name, shader );
                }
                assetBundle.Unload( false );
            }
        }

        public void PostBeginStart()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            LoadAssetBundleAsync( "shaders/shaders_mac", InitShaders );
#else
            LoadAssetBundleAsync("shaders/shaders", InitShaders);
#endif
            // 加载文件依赖信息
            LoadDependences();
        }

        private void LoadDependences()
        {
            string fileFullPath = AssetPathConfig.GetAssetFullPath( "ArtResources" );
            if( string.IsNullOrEmpty(fileFullPath) )
            {
                Debug.LogError( "not find ArtResources" );
                return;
            }

#if UNITY_EDITOR && !USE_ASSET_BUNDLE_EDITOR
            // init dependence in editor
#else
            AssetBundle bundle = AssetBundle.LoadFromFile( fullPath );

            AssetBundleManifest manifest = bundle.LoadAsset( "AssetBundleManifest" ) as AssetBundleManifest;
            List<string> dps = new List<string>();

            foreach( string abPath in manifest.GetAllAssetBundles() )
            {
                dps.Clear();

                foreach( string dp in manifest.GetDirectDependencies( abPath ) )
                {
                    if( !dp.Contains( "shaders/shaders" ) )
                        dps.Add( dp );
                }

                if( dps.Count > 0 )
                    _dependences[abPath] = dps.ToArray();
            }

            bundle.Unload( true );
#endif
        }

        //// 获取ab依赖
        private string[] s_dpsEmpty = new string[0];
        public string[] GetDependences( string assetPath )
        {
            string[] dps;
            if( _dependences.TryGetValue( assetPath, out dps ) )
            {
                return dps;
            }
            return s_dpsEmpty;
        }

        public Shader LoadShader( string name )
        {
            Shader shader = null;

#if UNITY_EDITOR
            shader = Shader.Find( name );
            if( shader )
            {
                return shader;
            }
#endif
            if( _shaders.TryGetValue( name, out shader ) )
            {
                return shader;
            }
            return null;
        }


        public void AddDownLoadHost( string url )
        {
            if( !string.IsNullOrEmpty(url) )
                _downLoadHosts.Add( url );
        }

        private string GetResourceUrl( string sourceName, int hostIndex )
        {
            if( _downLoadHosts.Count == 0 )
            {
                Debug.LogWarning( "no download host" );
                return "";
            }
            if( _downLoadHosts.Count <= hostIndex )
            {
                Debug.LogWarning( "logic error" );
                return "";
            }

            if( _downLoadHosts[hostIndex].EndsWith("/") )
                return string.Format( "{0}{1}", _downLoadHosts[hostIndex], sourceName );
            else
                return string.Format( "{0}/{1}", _downLoadHosts[hostIndex], sourceName );
        }

        //开启网络下载
        private HttpDownLoadHandler DoDownload( string sourceName, string md5, Action<Dictionary<string, object>> callback, int hostIndex = 0 )
        {
            string url = GetResourceUrl( sourceName, hostIndex );
            if( string.IsNullOrEmpty(url) )
                return null;

            string saveFileFullPath = AssetPathConfig.GetUpdateDataPath(sourceName);

            HttpDownLoadHandler downLoadHandle = HttpClient.Instance.DownLoadAssetBundle( url, saveFileFullPath, 0, ( dict ) =>
            {
                if( !dict.ContainsKey( "assetBundle" ) )
                {
                    if( hostIndex < _downLoadHosts.Count - 1 )
                    {
                        DoDownload( sourceName, md5, callback, hostIndex + 1 );
                    }
                    else
                    {
                        dict["status"] = -1;
                        dict["data"] = "Not find assetBundle on host.";
                        callback( dict );
                    }
                    return;
                }
                else{
                    callback( dict );
                }
            },
            true, md5 );

            return downLoadHandle;
        }

        //下载AssetBundle并缓存
        private FrameTask DownloadAndAddBundle( string assetBundlePath )
        {
            //如果正在加载，返回。
            if( _loadingTaskMap.ContainsKey( assetBundlePath ) )
            {
                return _loadingTaskMap[assetBundlePath];
            }

            //handle句柄
            if( _loaderMap.ContainsKey( assetBundlePath ) )
            {
                Debug.Log( "multi add bundle:" + assetBundlePath );
                ReleaseLoader( _loaderMap[assetBundlePath], false );

                _loaderMap.Remove( assetBundlePath );
            }

            //总的加载任务
            MultTask mTask = new MultTask( "add" );

            //主AssetBundle加载任务
            FrameTask addTask = new FrameTask();
            mTask.AddCondition( "add", addTask );

            //loader保存加载器和下载器
            AssetBundleLoader abLoader = new AssetBundleLoader( assetBundlePath );
            abLoader.task = mTask;

            //获取本地资源路径.
            string fileFullPath = AssetPathConfig.GetAssetFullPath( assetBundlePath );
            if( string.IsNullOrEmpty(fileFullPath) )
            {
                string md5 = "";  //TODO

                abLoader.downloadHandle = DoDownload( assetBundlePath, md5, ( dict ) =>
                {
                    abLoader.downloadHandle = null;  //下载结束要释放引用.

                    if( dict != null && dict.ContainsKey( "assetBundle" ) )
                    {
                        AssetBundle assetBundle = (AssetBundle)dict["assetBundle"];
                        if( !_assetBundleCacheMap.ContainsKey( assetBundlePath ) )
                        {
                            _assetBundleCacheMap.Add( assetBundlePath, new AssetBundleCache( assetBundle ) );
                        }
                        addTask.Callback( true, assetBundle );
                    }
                    else
                    {
                        addTask.Callback( false, "Download failed: " + assetBundlePath );
                    }
                } );
            }
            else
            {
                //本地文件异步加载
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( fileFullPath );
                request.completed += ( oper ) =>
                {
                    AssetBundle assetBundle = request.assetBundle;
                    if( assetBundle != null )
                    {
                        if( !_assetBundleCacheMap.ContainsKey( assetBundlePath ) )
                        {
                            _assetBundleCacheMap.Add( assetBundlePath, new AssetBundleCache( assetBundle ) );
                        }
                        addTask.Callback( true, assetBundle );
                    }
                    else
                    {
                        addTask.Callback( false, "LoadFromFileAsync" );
                    }
                };
            }

            //加载完成
            mTask.AddCallback( ( flag, obj ) =>
            {
                if( flag )
                {
                    abLoader.assetBundleCache = _assetBundleCacheMap[assetBundlePath];
                }
                abLoader.IsCompleted = true;
                return true;
            } );
            _loaderMap.Add( assetBundlePath, abLoader );

            // 加载依赖
            // 主AssetBundle加载完成之后再开始加载依赖，所有依赖加载通过MultTask dependTask控制
            FrameTask waitDependsTask = new FrameTask();

            //waitDependsTask完成后mTask才能完成
            mTask.AddCondition( "waitDepends", waitDependsTask );

            MultTask dependTask = new MultTask();

            //addTask完成后可以开始dependTask
            addTask.AddCallback( ( flag, obj ) =>
            {
                if( flag )
                {
                    string[] depends = AssetBundleManager.Instance.GetDependences( assetBundlePath );
                    abLoader.depLoaders = new AssetBundleLoader[depends.Length];
                    bool hasOtherTask = false;

                    for( int i = 0; i < depends.Length; i++ )
                    {
                        string item = depends[i];
                        if( !_assetBundleCacheMap.ContainsKey( item ) )
                        {
                            hasOtherTask = true;
                            var otherTask = DownloadAndAddBundle( item );
                            dependTask.AddCondition( item, otherTask );
                        }

                        var depItem = _loaderMap[item];
                        depItem.RefCount++;
                        abLoader.depLoaders[i] = depItem;
                    }

                    if( !hasOtherTask )
                    {
                        dependTask.Callback( true );
                    }

                }
                return true;
            } );

            //dependTask完成后waitDependsTask才能完成.
            dependTask.AddCallback( ( flag, obj ) =>
            {
                if( flag )
                {
                    var ab = _assetBundleCacheMap[assetBundlePath].assetBundle;
                    waitDependsTask.Callback( true, ab );
                }
                else
                {
                    waitDependsTask.Callback( false, obj );
                }
                return true;
            } );

            //全部完成后从加载队列移除
            mTask.AddCallback( ( flag, obj ) =>
            {
                if( _loadingTaskMap.ContainsKey( assetBundlePath ) )
                {
                    _loadingTaskMap.Remove( assetBundlePath );
                }
                return true;
            } );

            _loadingTaskMap[assetBundlePath] = mTask;

            return mTask;
        }

        //异步加载AssetBundle，然后加载指定资源。
        private FrameTask DownLoadAndLoadAsset( string assetBundlePath, string assetName, Type type, Action<bool, Object> callback )
        {
            string taskKey = assetBundlePath + assetName;

            if( _loadingTaskMap.ContainsKey( taskKey ) )  //重复的请求
            {
                FrameTask task = _loadingTaskMap[taskKey];
                task.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        callback( flag, (Object)obj );
                    }
                    else
                    {
                        callback( flag, null );
                    }
                    return true;
                } );
                return task;
            }

            //新的请求，结束时把从AssetBundle加载到的资源缓存起来，并从task队列删除。
            FrameTask loadAssetTask = new FrameTask();
            loadAssetTask.AddCallback( ( flag, obj ) =>
            {
                if( flag )
                {
                    Object asset = (Object)obj;
                    if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
                    {
                        _assetBundleCacheMap[assetBundlePath].AddAsset( assetName, asset );
                    }
                    callback( flag, asset );
                }
                else
                {
                    callback( flag, null );
                }
                return true;
            } );
            loadAssetTask.AddCallback( ( flag, obj ) =>
            {
                if( _loadingTaskMap.ContainsKey( taskKey ) )
                {
                    _loadingTaskMap.Remove( taskKey );
                }
                return true;
            } );

            // 如果已经加载过AssetBundle，则直接从AssetBundle中加载资源。
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                AssetBundle assetBundle = _assetBundleCacheMap[assetBundlePath].assetBundle;
                AssetBundleRequest request = assetBundle.LoadAssetAsync( assetName, type );

                request.completed += ( oper ) =>
                {
                    if( request.asset == null )
                    {
                        loadAssetTask.Callback( false, "LoadAssetAsync" );
                    }
                    else
                    {
                        loadAssetTask.Callback( true, request.asset );
                    }
                };
            }
            else
            {
                // 先从网络或者本地加载AssetBundle，再从AssetBundle中加载资源。
                FrameTask addTask = DownloadAndAddBundle( assetBundlePath );
                addTask.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        AssetBundle assetBundle = _assetBundleCacheMap[assetBundlePath].assetBundle;
                        AssetBundleRequest request = assetBundle.LoadAssetAsync( assetName, type );

                        request.completed += ( oper ) =>
                        {
                            if( request.asset == null )
                            {
                                loadAssetTask.Callback( false, "LoadAssetAsync" );
                            }
                            else
                            {
                                loadAssetTask.Callback( true, request.asset );
                            }
                        };
                    }
                    else
                    {
                        loadAssetTask.Callback( false, obj );
                    }
                    return true;
                } );
            }

            _loadingTaskMap[taskKey] = loadAssetTask;

            return loadAssetTask;
        }

        //异步加载AssetBundle，如果本地没有，会从网络下载到本地。
        public AssetBundleLoader LoadAssetBundleAsync( string assetBundlePath, LoadAssetCallback callback )
        {
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                var ab = _assetBundleCacheMap[assetBundlePath].assetBundle;
                callback( ab );
            }
            else
            {
                FrameTask task = DownloadAndAddBundle( assetBundlePath );
                task.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        callback( (AssetBundle)obj );
                    }
                    else
                    {
                        callback( null );
                    }
                    return true;
                } );
            }

            _loaderMap[assetBundlePath].RefCount++;
            return _loaderMap[assetBundlePath];
        }

        //同步加载AssetBundle，如果本地没有，返回null。
        public AssetBundle LoadAssetBundle( string assetBundlePath )
        {
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                return _assetBundleCacheMap[assetBundlePath].assetBundle;
            }
            else
            {
                string fileFullPath = AssetPathConfig.GetAssetFullPath( assetBundlePath );
                if( string.IsNullOrEmpty(fileFullPath) )
                {
                    Debug.LogWarning( "LoadAssetBundle failed for: " + assetBundlePath );
                    return null;
                }

                AssetBundle ab = AssetBundle.LoadFromFile( fileFullPath );
                var info = new AssetBundleCache( ab );
                _assetBundleCacheMap.Add( assetBundlePath, info );

                AssetBundleLoader abLoader = new AssetBundleLoader( assetBundlePath );
                abLoader.assetBundleCache = info;
                _loaderMap.Add( assetBundlePath, abLoader );

                string[] depends = GetDependences( assetBundlePath );
                abLoader.depLoaders = new AssetBundleLoader[depends.Length];

                for( int i = 0; i < depends.Length; i++ )
                {
                    string dep = depends[i];
                    if( _loaderMap.ContainsKey( dep ) )
                    {
                        abLoader.depLoaders[i] = _loaderMap[dep];
                    }
                    else
                    {
                        if( LoadAssetBundle( dep ) != null )
                        {
                            abLoader.depLoaders[i] = _loaderMap[dep];
                        }
                    }
                }
                return ab;
            }
        }

        //异步加载资源，返回资源。
        public AssetBundleLoader LoadAssetAsync( string assetBundlePath, string assetName, Type type, LoadAssetCallback callback )
        {
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                var abhandleInfo = _assetBundleCacheMap[assetBundlePath];
                if( abhandleInfo.assetMap.ContainsKey( assetName ) )
                {
                    callback( abhandleInfo.assetMap[assetName] );

                    _loaderMap[assetBundlePath].RefCount++;
                    return _loaderMap[assetBundlePath];
                }
            }

            DownLoadAndLoadAsset( assetBundlePath, assetName, type, ( flag, obj ) =>
            {
                if( flag )
                {
                    callback( obj );
                }
                else
                {
                    callback( null );
                }
            } );

            _loaderMap[assetBundlePath].RefCount++;

            return _loaderMap[assetBundlePath];
        }

        //同步加载资源，返回资源。
        public Object LoadAsset( string assetBundlePath, string assetName, Type type )
        {
            Object prefab = null;

            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                var abhandleInfo = _assetBundleCacheMap[assetBundlePath];
                if( abhandleInfo.assetMap.ContainsKey( assetName ) )
                {
                    prefab = abhandleInfo.assetMap[assetName];
                }
            }

            if( prefab == null )
            {
                AssetBundle assetBundle = LoadAssetBundle( assetBundlePath );
                if( assetBundle == null )
                {
                    Debug.LogWarning( "No assetBundle at " + assetBundlePath );
                }
                else
                {
                    prefab = assetBundle.LoadAsset( assetName, type );
                    //Debug.LogWarning( "LoadAsset " + assetBundlePath );
                }
            }

            return prefab;
        }

        //异步加载资源，并Instantiate出一个实例来返回.
        public AssetBundleLoader LoadInstanceAssetAsync( string assetBundlePath, string assetName, Type type, LoadAssetCallback callback, Transform parent = null )
        {
            return LoadAssetAsync( assetBundlePath, assetName, type, delegate ( Object obj )
            {
                if( obj == null )
                {
                    callback( null );
                }
                else
                {
                    Object go;
                    if( parent != null )
                        go = Object.Instantiate( obj, parent );
                    else
                        go = Object.Instantiate( obj );

                    callback( go );
                }
            } );
        }

        //同步加载资源，并Instantiate出一个实例来返回.
        public Object LoadInstanceAsset( string assetBundlePath, string assetName, Type type, Transform parent = null )
        {
            Object obj = LoadAsset( assetBundlePath, assetName, type );
            //Debug.LogWarning( "LoadInstanceAsset " + assetBundlePath );
            if( obj == null )
            {
                return null;
            }
            else
            {
                Object go;
                if( parent != null )
                    go = Object.Instantiate( obj, parent );
                else
                    go = Object.Instantiate( obj );

                return go;
            }
        }


        #region 一次把所有AssetBundle相关联的依赖，内部资源都加载出来，不再指定单独的资源名

        //异步加载AssetBundle，然后加载所有资源。区别于DownLoadAndLoadAsset。
        private FrameTask DownloadAndLoadAllAsset( string assetBundlePath, Action<bool, Object[]> callback )
        {
            string taskKey = assetBundlePath + "_load_all";

            if( _loadingTaskMap.ContainsKey( taskKey ) )  //重复的请求
            {
                var task = _loadingTaskMap[taskKey];
                task.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        callback( flag, (Object[])obj );
                    }
                    else
                    {
                        callback( flag, null );
                    }
                    return true;
                } );
                return task;
            }

            //新的请求，结束时把从AssetBundle加载到的资源缓存起来，并从task队列删除。
            FrameTask loadAssetTask = new FrameTask();
            loadAssetTask.AddCallback( ( flag, obj ) =>
            {
                if( flag )
                {
                    Object[] allAssets = (Object[])obj;

                    if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
                    {
                        if( allAssets != null )
                        {
                            foreach( Object o in allAssets )
                                _assetBundleCacheMap[assetBundlePath].AddAsset( o.name, o );
                        }
                        else
                        {
                            Debug.LogWarning( "DownloadAndLoadAllAsset failed for: " + assetBundlePath );
                        }
                    }
                    callback( flag, (Object[])obj );
                }
                else
                {
                    callback( flag, null );
                }
                return true;
            } );
            loadAssetTask.AddCallback( ( flag, obj ) =>
            {
                if( _loadingTaskMap.ContainsKey( taskKey ) )
                {
                    _loadingTaskMap.Remove( taskKey );
                }
                return true;
            } );


            // 如果已经加载过AssetBundle，则直接从AssetBundle中加载资源。
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                AssetBundle assetBundle = _assetBundleCacheMap[assetBundlePath].assetBundle;
                AssetBundleRequest request = assetBundle.LoadAllAssetsAsync();

                request.completed += ( oper ) =>
                {
                    if( request.asset == null )
                    {
                        loadAssetTask.Callback( false, "LoadAssetAsync" );
                    }
                    else
                    {
                        loadAssetTask.Callback( true, request.allAssets );
                    }
                };
            }
            else
            {
                // 先从网络或者本地加载AssetBundle，再从AssetBundle中加载资源。
                var addTask = DownloadAndAddBundle( assetBundlePath );
                addTask.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        AssetBundle assetBundle = _assetBundleCacheMap[assetBundlePath].assetBundle;
                        AssetBundleRequest request = assetBundle.LoadAllAssetsAsync();

                        request.completed += ( oper ) =>
                        {
                            if( request.asset == null )
                            {
                                loadAssetTask.Callback( false, "LoadAssetAsync" );
                            }
                            else
                            {
                                loadAssetTask.Callback( true, request.allAssets );
                            }
                        };
                    }
                    else
                    {
                        loadAssetTask.Callback( false, obj );
                    }
                    return true;
                } );
            }

            _loadingTaskMap[taskKey] = loadAssetTask;

            return loadAssetTask;
        }

        //同步加载AssetBundle与其依赖的AssetBundle，并把里面的内容全部读取出来。区别于LoadAssetBundle。
        public AssetBundle LoadAllAssetBundle( string assetBundlePath )
        {
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                return _assetBundleCacheMap[assetBundlePath].assetBundle;
            }
            else
            {
                string fileFullPath = AssetPathConfig.GetAssetFullPath( assetBundlePath );
                if( string.IsNullOrEmpty(fileFullPath) )
                {
                    Debug.LogWarning( "LoadAssetBundle failed for: " + assetBundlePath );
                    return null;
                }

                AssetBundle ab = AssetBundle.LoadFromFile( fileFullPath );
                var info = new AssetBundleCache( ab );
                _assetBundleCacheMap.Add( assetBundlePath, info );

                AssetBundleLoader abLoader = new AssetBundleLoader( assetBundlePath );
                abLoader.assetBundleCache = info;
                _loaderMap.Add( assetBundlePath, abLoader );

                //load all assets from assetBundle
                Object[] allAssets = ab.LoadAllAssets();
                foreach( Object o in allAssets )
                    info.AddAsset( o.name, o );

                //load all dependences
                string[] depends = GetDependences( assetBundlePath );
                abLoader.depLoaders = new AssetBundleLoader[depends.Length];

                for( int i = 0; i < depends.Length; i++ )
                {
                    string dep = depends[i];
                    if( _loaderMap.ContainsKey( dep ) )
                    {
                        abLoader.depLoaders[i] = _loaderMap[dep];
                    }
                    else
                    {
                        if( LoadAssetBundle( dep ) != null )
                        {
                            abLoader.depLoaders[i] = _loaderMap[dep];
                        }
                    }
                }
                return ab;
            }
        }

        //异步加载AssetBundle与其依赖的AssetBundle，并把里面的内容全部读取出来，如果本地没有，会从网络下载到本地。区别于LoadAssetBundleAsync。
        public AssetBundleLoader LoadAllAssetBundleAsync( string assetBundlePath, LoadAllAssetCallback callback )
        {
            if( _assetBundleCacheMap.ContainsKey( assetBundlePath ) )
            {
                int count = _assetBundleCacheMap[assetBundlePath].assetMap.Values.Count;
                Object[] allAssets = new Object[count];
                _assetBundleCacheMap[assetBundlePath].assetMap.Values.CopyTo( allAssets, 0 );
                callback( allAssets );
            }
            else
            {
                FrameTask task = DownloadAndLoadAllAsset( assetBundlePath, ( flag, obj ) => { } );
                task.AddCallback( ( flag, obj ) =>
                {
                    if( flag )
                    {
                        callback( (Object[])obj );
                    }
                    else
                    {
                        callback( null );
                    }
                    return true;
                } );
            }

            _loaderMap[assetBundlePath].RefCount++;
            return _loaderMap[assetBundlePath];
        }

        #endregion


        //释放LoaderHandle
        private void ReleaseLoader( AssetBundleLoader loaderHandle, bool unloadAllLoadedObjects = false )
        {
            if( loaderHandle == null )
                return;

            if( loaderHandle.assetBundleCache != null )
            {
                loaderHandle.assetBundleCache.Clear( unloadAllLoadedObjects );
                loaderHandle.assetBundleCache = null;
            }

            if( loaderHandle.task != null )
            {
                loaderHandle.task.CleanCallback();
                loaderHandle.task = null;
            }

            //如果Loader正在下载网络资源，需要中断
            if( loaderHandle.downloadHandle != null )
            {
                HttpClient.Instance.CancelDownload( loaderHandle.downloadHandle );
                loaderHandle.downloadHandle = null;
            }

            if( loaderHandle.depLoaders != null )
            {
                foreach( AssetBundleLoader abLoader in loaderHandle.depLoaders )
                    DestroyLoader( abLoader );

                loaderHandle.depLoaders = null;
            }
        }

        public void DestroyLoader( AssetBundleLoader loaderHandle, bool unloadAllLoadedObjects = true )
        {
            if( loaderHandle == null )
            {
                return;
            }

            loaderHandle.RefCount--;

            if( loaderHandle.RefCount == 1 )//防止相互依赖死锁
            {
                if( _assetBundleCacheMap.ContainsKey( loaderHandle.srcName ) )
                {
                    _assetBundleCacheMap.Remove( loaderHandle.srcName );
                }
                if( _loaderMap.ContainsKey( loaderHandle.srcName ) )
                {
                    _loaderMap.Remove( loaderHandle.srcName );
                }

                ReleaseLoader( loaderHandle, unloadAllLoadedObjects );
            }
        }

        public void DestroyLoaderByName( string assetBudleName, bool unloadAllLoadedObjects = true )
        {
            if( _loaderMap.ContainsKey( assetBudleName ) )
            {
                DestroyLoader( _loaderMap[assetBudleName], unloadAllLoadedObjects );
            }
        }
*/
    } //end class

}
