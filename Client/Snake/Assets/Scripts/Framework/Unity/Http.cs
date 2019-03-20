using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class Http : MonoSingleton<Http> 
    {
        public enum Result
        {
            Success = 0,
            Failed,
            Timeout,
        }

        public void Get(string url, Action<WWW> callback, uint retry=0)
        {
            StartCoroutine(Do_Internel(url, null, callback, retry));
        }

        public void Post(string url, WWWForm data, Action<WWW> callback, uint retry=0)
        {
            StartCoroutine(Do_Internel(url, data, callback, retry));
        }

        private IEnumerator Do_Internel(string url, WWWForm data, Action<WWW> callback, uint retry=0)
        {
            WWW www = null;
            Result result = Result.Success;

            for( uint i = 0; i < retry + 1 ; i++ )
            {
                if( data == null )
                    www = new WWW(url);
                else
                    www = new WWW(url, data);

                float time = Time.unscaledTime;

                yield return www;

                if( www.error != null ){
                    result = Result.Failed;
                }
                else{
                    if( Time.unscaledTime - time > 10f ){
                        result = Result.Timeout;
                    }
                    else{
                        result = Result.Success;
                        break;
                    }
                }

                yield return new WaitForSeconds(3f);
            }

            if( result != Result.Success ){
                Debug.LogWarningFormat("[http] www to {0} {1}", url, result.ToString());
            }

            if( callback != null ){
                callback(www);
            }

            if( www != null ){
                www.Dispose();
                www = null;
            }
        }
    
    }
}
