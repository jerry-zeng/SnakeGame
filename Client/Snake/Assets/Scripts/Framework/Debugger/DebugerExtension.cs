////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////

using System.Reflection;

namespace Framework
{
    public static class DebugerExtension
    {
        //----------------------------------------------------------------------
        public static void Log(this object obj, string message)
        {
            Debuger.Log(GetLogTag(obj), message);
        }

        public static void Log(this object obj, string format, params object[] args)
        {
            Debuger.Log(GetLogTag(obj), string.Format(format, args));
        }


        //----------------------------------------------------------------------
        public static void LogError(this object obj, string message)
        {
            Debuger.LogError(GetLogTag(obj), message);
        }

        public static void LogError(this object obj, string format, params object[] args)
        {
            Debuger.LogError(GetLogTag(obj), string.Format(format, args));
        }


        //----------------------------------------------------------------------
        public static void LogWarning(this object obj, string message)
        {
            Debuger.LogWarning(GetLogTag(obj), message);
        }

        public static void LogWarning(this object obj, string format, params object[] args)
        {
            Debuger.LogWarning(GetLogTag(obj), string.Format(format, args));
        }

        //----------------------------------------------------------------------

        //----------------------------------------------------------------------
        private static string GetLogTag(object obj)
        {
            FieldInfo fi = obj.GetType().GetField("LOG_TAG");
            if (fi != null)
            {
                return (string) fi.GetValue(obj);
            }

            return obj.GetType().Name;
        }
    }
}
