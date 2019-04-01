using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;  //joined from .Net 4.x
using System;

namespace Framework
{
    public delegate void HttpCallback( Dictionary<string, object> dict );


    public sealed class HttpClient : MonoSingleton<HttpClient> 
    {
        public class IOTask
        {
            public const int RESULT_SUCCESS = 0;
            public const int RESULT_FAIL = -1;

            /// <summary>
            /// The thread task to do i/o.
            /// </summary>
            public Task threadTask;

            /// <summary>
            /// The callback, the parameter would be RESULT_SUCCESS or RESULT_FAIL.
            /// </summary>
            public Action<int> callback;
        }

        public class RequestTask
        {
            public enum EStatus
            {
                Created = 0,
                Running,
                Completed
            }

            /// <summary>
            /// The start function, and a parameter usually is RequestTask itself.
            /// </summary>
            public Action<RequestTask> run;
            public EStatus status;

            public Task ioTask;//  下载时有io任务
            public UnityWebRequest request;
        }


        public const int IO_RESULT_SUCCESS = 0;
        public const int IO_RESULT_FAIL = -1;

        private List<IOTask> _ioTaskList = new List<IOTask>();
        private List<RequestTask> _httpRequestList = new List<RequestTask>();

        private Action<UnityWebRequest> _disposeWebRequestCaster;
        public Action<UnityWebRequest> DisposeWebRequestCaster
        {
            get{ return _disposeWebRequestCaster; }
            set{ _disposeWebRequestCaster = value; }
        }

        //最大io线程数量
        private int _maxIOTask = 5;
        public int MaxIOTask 
        { 
            get { return _maxIOTask; } set{ _maxIOTask = value; } 
        }

        //最大下载线程数量
        private int _maxRequestTask = 5;
        public int MaxRequestTask
        { 
            get { return _maxRequestTask; } set{ _maxRequestTask = value; } 
        }

        void Update()
        {
            if( _ioTaskList.Count > 0 )
            {
                int runningCount = 0;
                Task canRunTask = null;

                //倒序删除
                for( int i = _ioTaskList.Count - 1; i >= 0; i-- )
                {
                    IOTask ioTask = _ioTaskList[i];
                    if( ioTask.threadTask.IsCompleted )
                    {
                        _ioTaskList[i] = _ioTaskList[_ioTaskList.Count-1];
                        _ioTaskList.RemoveAt( _ioTaskList.Count-1 );

                        if( ioTask.threadTask.Status == TaskStatus.RanToCompletion )
                            ioTask.callback( IOTask.RESULT_SUCCESS );
                        else
                            ioTask.callback( IOTask.RESULT_FAIL );

                        continue;
                    }

                    if( ioTask.threadTask.Status == TaskStatus.Created )
                        if( canRunTask == null )
                            canRunTask = ioTask.threadTask;
                    else
                        runningCount++;
                }

                if( canRunTask != null && runningCount < MaxIOTask )
                    canRunTask.Start();
            }

            //请求管理
            if( _httpRequestList.Count > 0 )
            {
                RequestTask canRunRequest = null;
                int runningCount = 0;

                for( int i = _httpRequestList.Count - 1; i >= 0; i-- )
                {
                    var requestTask = _httpRequestList[i];
                    if( requestTask.status == RequestTask.EStatus.Completed )
                    {
                        _httpRequestList[i] = _httpRequestList[_httpRequestList.Count-1];
                        _httpRequestList.RemoveAt( _httpRequestList.Count-1 );
                        continue;
                    }

                    if( requestTask.status == RequestTask.EStatus.Running )
                    {
                        if( requestTask.ioTask != null && requestTask.ioTask.IsCompleted )
                            requestTask.status = RequestTask.EStatus.Completed;
                        else
                            runningCount++;
                    }
                    else
                    {
                        if( canRunRequest == null )
                            canRunRequest = requestTask;
                    }
                }

                if( canRunRequest != null && runningCount < MaxRequestTask )
                {
                    canRunRequest.run( canRunRequest );
                }
            }
        }

        protected override void OnDestroy()
        {
            ClearAllTask();

            base.OnDestroy();
        }

        public void DisposeWebRequesat( UnityWebRequest request )
        {
            if( request == null ) return;

            //可能request是Get, Post之后传过来的，不是Download，此时s_disposeWebRequestCast还没有。
            if( _disposeWebRequestCaster != null )  
                _disposeWebRequestCaster.Invoke( request );

            request.Dispose();
        }

        public void CancelDownload( HttpDownLoader downLoadHandle )
        {
            if( downLoadHandle == null || downLoadHandle.IsClosed ) return;

            downLoadHandle.Request.Abort();
        }

        public void ClearAllTask()
        {
            foreach( var requestTask in _httpRequestList )
            {
                if( requestTask.status == RequestTask.EStatus.Running )
                {
                    if( !requestTask.request.isDone )
                        requestTask.request.Abort();
                    else
                        DisposeWebRequesat( requestTask.request );
                }
                else
                {
                    DisposeWebRequesat( requestTask.request );
                }
            }
            _httpRequestList.Clear();

            _ioTaskList.Clear();
        }


        Dictionary<string, string> GetPostData(string data)
        {
            Dictionary<string, string> formFields = new Dictionary<string, string>();
            if( string.IsNullOrEmpty(data) )
                return formFields;

            string[] dataList = data.Split( new char[1] { '&' }, System.StringSplitOptions.RemoveEmptyEntries );
            foreach( string str in dataList )
            {
                string[] strList = str.Split( new char[1] { '=' }, System.StringSplitOptions.RemoveEmptyEntries );
                if( strList.Length >= 2 )
                {
                    formFields.Add( strList[0], strList[1] );
                }
            }

            return formFields;
        }

        public void Get( string url, bool needUnZip, int timeout, HttpCallback callback )
        {
            UnityWebRequest request = UnityWebRequest.Get( url );
            if( timeout > 0 )
            {
                request.timeout = timeout;
            }
            AddRequestTask( request, url, "", needUnZip, callback );
        }

        public void Post( string url, Dictionary<string, string> formFields, bool needUnZip, int timeout, HttpCallback callback )
        {
            UnityWebRequest request = UnityWebRequest.Post( url, formFields );
            if( timeout > 0 )
            {
                request.timeout = timeout;
            }
            AddRequestTask( request, url, "", needUnZip, callback );
        }
        public void Post( string url, string data, bool needUnZip, int timeout, HttpCallback callback )
        {
            Post( url, GetPostData(data), needUnZip, timeout, callback );
        }

        public void UploadFile( string url, string fullFilePath, bool needUnZip, int timeout, HttpCallback callback )
        {
            if( !File.Exists( fullFilePath ) )
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add( "data", "file not exists !" );
                dict.Add( "status", IO_RESULT_FAIL );
                callback( dict );
                return;
            }

            byte[] fileData = null;

            //read file data
            Task threadTask = new Task( () =>
            {
                using( FileStream fileStream = new FileStream( fullFilePath, FileMode.Open ) )
                {
                    int dataLen = (int)fileStream.Length;
                    if( dataLen > 0 )
                    {
                        fileData = new byte[dataLen];
                        fileStream.Read( fileData, 0, dataLen );
                    }
                }
            } );

            //add to io task queue
            lock( _ioTaskList )
            {
                IOTask newTask = new IOTask();
                newTask.threadTask = threadTask;
                newTask.callback = ( status ) =>
                {
                    if( status == IOTask.RESULT_SUCCESS )
                    {
                        if( fileData == null )
                        {
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict.Add("data", "file data is empty!");
                            dict.Add("status", IO_RESULT_FAIL);
                            callback(dict);
                        }
                        else
                        {
                            UnityWebRequest request = UnityWebRequest.Put(url, fileData);
                            if( timeout > 0 )
                            {
                                request.timeout = timeout;
                            }
                            AddRequestTask(request, url, "", needUnZip, callback);
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach( var ex in threadTask.Exception.InnerExceptions )
                        {
                            sb.Append( ex.Message );
                            sb.Append("\n");
                        }

                        string data = string.Format("{{\"code\":0,\"message\":\"read error:{0}\"}}", sb.ToString());

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("data", data);
                        dict.Add("status", IO_RESULT_FAIL);

                        callback(dict);
                    }
                };

                _ioTaskList.Add( newTask );
            }
        }

        public HttpDownLoader DownloadFile( string url, string saveFileFullPath, bool needUnZip, int timeout, HttpCallback callback, bool first = false, string matchMd5 = null )
        {
            if( string.IsNullOrEmpty(saveFileFullPath) ){
                Debug.LogWarning("No fullFilePath, is there any error?");
            }

            UnityWebRequest request = UnityWebRequest.Get( url );
            if( timeout > 0 )
            {
                request.timeout = timeout;
            }
            AddRequestTask( request, url, saveFileFullPath, needUnZip, callback, first, false, matchMd5 );

            HttpDownLoader handler = new HttpDownLoader( request );
            _disposeWebRequestCaster += handler.Close;

            return handler;
        }

        public HttpDownLoader DownLoadAssetBundle( string url, string saveFileFullPath, int timeout, HttpCallback callback, bool first = true, string matchMd5 = null )
        {
            if( string.IsNullOrEmpty(saveFileFullPath) ){
                Debug.LogError("No fullFilePath, cancel the downloading op");
                return null;
            }

            UnityWebRequest request = UnityWebRequest.Get( url );
            if( timeout > 0 )
            {
                request.timeout = timeout;
            }
            AddRequestTask( request, url, saveFileFullPath, false, callback, first, true, matchMd5 );

            HttpDownLoader handler = new HttpDownLoader( request );
            _disposeWebRequestCaster += handler.Close;

            return handler;
        }


        private void SaveFile( string fullPath, byte[] data )
        {
            if( data == null )
                return;

            try
            {
                string dir = Path.GetDirectoryName( fullPath );
                if( !Directory.Exists( dir ) )
                    Directory.CreateDirectory( dir );

                if( File.Exists(fullPath) )
                    File.Delete(fullPath);
                
                File.WriteAllBytes( fullPath, data );

                //TODO: update the file md5 on local record?
            }
            catch( System.Exception e )
            {
                Debug.LogError(e.Message);
            }
        }

        private int TryGetHttpData( byte[] srcData, out string outstring, bool unzip = false )
        {
            try
            {
                if( unzip )
                    outstring = StringUtils.DecompressToString(srcData);
                else
                    outstring = StringUtils.ToString(srcData);

                return IO_RESULT_SUCCESS;
            }
            catch( System.Exception )
            {
                outstring = "data uncompress failed";
                return IO_RESULT_FAIL;
            }
        }

        private Task OnRequestComplete( UnityWebRequest request, float startTime, bool needUnZip, string saveFileFullPath, HttpCallback callback, string matchMd5 = null )
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            int respCode = (int)request.responseCode;
            dict.Add( "status", respCode );

            bool succeed = !request.isHttpError && !request.isNetworkError;
            if( succeed )
            {
                float endTime = Time.realtimeSinceStartup;

                ulong size = request.downloadedBytes;
                float speed = size / (endTime - startTime);

                dict.Add( "size", size );
                dict.Add( "speed", speed );

                string data = "";

                if( needUnZip )
                {
                    int ret = TryGetHttpData( request.downloadHandler.data, out data, true );
                    if( ret != IO_RESULT_SUCCESS )
                    {
                        data = "{\"code\":0,\"data\":\"data unzip fail\"}";
                    }
                }
                else
                {
                    // no need to save.
                    if( string.IsNullOrEmpty( saveFileFullPath ) )
                    {
                        data = request.downloadHandler.text;
                    }
                    else
                    {
                        byte[] downloadBytes = request.downloadHandler.data;

                        //compare md5
                        if( matchMd5 != null && StringUtils.BytesToMD5( downloadBytes ) != matchMd5 )
                        {
                            data = string.Format( "{{\"code\":5,\"message\":\"{0}\"}}", "md5 not match" );
                        }
                        else
                        {
                            Task threadTask = new Task( ( bytes ) => 
                            {
                                SaveFile( saveFileFullPath, (byte[])bytes );
                            }, downloadBytes );

                            lock( _ioTaskList )
                            {
                                IOTask newTask = new IOTask();
                                newTask.threadTask = threadTask;
                                newTask.callback = ( status ) =>
                                {
                                    if( status == IOTask.RESULT_SUCCESS )
                                    {
                                        data = "ok";
                                        dict.Add("data", data);
                                    }
                                    else
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        foreach( var ex in threadTask.Exception.InnerExceptions )
                                        {
                                            string s = ex.Message.Replace("\\", "\\\\");
                                            sb.Append(s.Replace("\"", "\\\""));
                                            sb.Append("\n");
                                        }
                                        data = string.Format("{{\"code\":0,\"message\":\"{0}\"}}", sb.ToString());
                                        Debug.LogError(data);

                                        dict.Add("data", data);
                                    }

                                    callback(dict);
                                };

                                _ioTaskList.Add( newTask );
                            }

                            DisposeWebRequesat(request);

                            return threadTask;
                        }
                    }
                }

                dict.Add( "data", data );
            }
            else
            {
                if( request.error != null )
                {
                    dict.Add( "data", request.error );
                }
                else
                {
                    dict.Add( "data", "request error, respCode:" + respCode.ToString() );
                }
            }

            callback( dict );

            DisposeWebRequesat( request );

            return null;
        }

        private Task OnRequestAssetBundleComplete( UnityWebRequest request, float startTime, string saveFileFullPath, HttpCallback callback, string matchMd5 = null )
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            int respCode = (int)request.responseCode;
            dict.Add( "status", respCode );

            bool succeed = !request.isHttpError && !request.isNetworkError;
            if( succeed )  //success and save assetBundle to disk.
            {
                float endTime = Time.realtimeSinceStartup;

                ulong size = request.downloadedBytes;
                float speed = size / (endTime - startTime);
                dict.Add( "size", size );
                dict.Add( "speed", speed );

                string data = "";

                // get assetBundle bytes
                byte[] downloadBytes = request.downloadHandler.data;

                // compare md5
                if( matchMd5 != null && StringUtils.BytesToMD5( downloadBytes ) != matchMd5 )
                {
                    data = string.Format( "{{\"code\":5,\"message\":\"{0}\"}}", "assetBundle md5 not match" );
                    dict.Add( "data", data );
                }
                else
                {
                    data = "ok";
                    dict.Add( "data", data );

                    AssetBundle assetBundle = AssetBundle.LoadFromMemory( downloadBytes );
                    dict.Add( "assetBundle", assetBundle );

                    // save assetBundle to disk.
                    Task threadTask = new Task( ( bytes ) =>
                    {
                        SaveFile( saveFileFullPath, (byte[])bytes );
                    }, downloadBytes );

                    lock( _ioTaskList )
                    {
                        IOTask newTask = new IOTask();
                        newTask.threadTask = threadTask;
                        newTask.callback = ( status ) =>
                        {
                            if( status != IOTask.RESULT_SUCCESS )
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach( var ex in threadTask.Exception.InnerExceptions )
                                {
                                    sb.Append( ex.Message );
                                    sb.Append("\n");
                                }
                                Debug.LogError(sb.ToString());
                            }
                        };

                        _ioTaskList.Add( newTask );
                    }

                    // no need to wait after assetBundle is saved to disk.
                    callback( dict );

                    DisposeWebRequesat( request );

                    return threadTask;
                }
            }
            else
            {
                if( request.error != null )
                {
                    dict.Add( "data", request.error );
                }
                else
                {
                    dict.Add( "data", "request error, respCode: " + respCode.ToString() );
                }
            }

            callback( dict );

            DisposeWebRequesat( request );

            return null;
        }


        private void AddRequestTask( UnityWebRequest request, string url, string fullFilePath, bool needUnZip, HttpCallback callback, bool first = false, bool isAssetBundle = false, string matchMd5 = null )
        {
            RequestTask requestTask = new RequestTask();
            requestTask.request = request;
            requestTask.status = RequestTask.EStatus.Created;
            requestTask.run = ( self ) =>
            {
                self.status = RequestTask.EStatus.Running;
                float startTime = Time.realtimeSinceStartup;

                UnityWebRequestAsyncOperation asyncOper = request.SendWebRequest();
                asyncOper.completed += ( result ) =>
                {
                    try
                    {
                        if( isAssetBundle )
                        {
                            OnRequestAssetBundleComplete(self.request, startTime, fullFilePath, callback, matchMd5);
                            self.status = RequestTask.EStatus.Completed;
                        }
                        else
                        {
                            Task task = OnRequestComplete(self.request, startTime, needUnZip, fullFilePath, callback, matchMd5);
                            if( task == null )
                            {
                                self.status = RequestTask.EStatus.Completed;
                            }
                            else
                            {
                                self.ioTask = task;
                            }
                        }
                    }
                    catch( System.Exception e )
                    {
                        Debug.LogWarning(e);
                        self.status = RequestTask.EStatus.Completed;
                    }
                };
            };

            if( first )
            {
                _httpRequestList.Insert( 0, requestTask );
            }
            else
            {
                _httpRequestList.Add( requestTask );
            }
        }

    }
}
