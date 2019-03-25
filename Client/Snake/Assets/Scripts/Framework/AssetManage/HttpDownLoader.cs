using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{
    public class HttpDownLoader
    {
        private UnityWebRequest _request;
        private bool _isClosed = false;

        public UnityWebRequest Request { get { return _request; } }
        public bool IsClosed { get { return _isClosed; } }

        public HttpDownLoader( UnityWebRequest request )
        {
            _request = request;
        }

        public float Progress
        {
            get
            {
                if( _isClosed )
                    return 1f;

                if( _request.isNetworkError || _request.isHttpError )
                    return 0f;

                float n = _request.downloadProgress;
                if( n < 0f )
                    return 0f;

                return n;
            }
        }

        public int ProgressPercent
        {
            get
            {
                return Mathf.FloorToInt( this.Progress * 100f );
            }
        }

        public void Close( UnityWebRequest request )
        {
            if( request == _request )
            {
                _isClosed = true;
                HttpClient.Instance.DisposeWebRequestCaster -= Close;
            }
        }
    }
}
