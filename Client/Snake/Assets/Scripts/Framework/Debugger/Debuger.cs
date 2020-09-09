
using System;
using System.Diagnostics;
using System.IO;

namespace Framework
{
    public static class Debuger
    {
        public static bool EnableLog = false;
        public static bool EnableTime = true;
        public static bool EnableSave = false;
        public static bool EnableStack = false;
        public static string LogFileDir = "";
        public static string LogFileName = "";
        public static string Prefix = "> ";
        private static StreamWriter LogFileWriter = null;
        public static bool UseUnityEngine = true;


        public static void Init()
        {
            CheckLogFileDir();
        }
        
        private static void Internal_Log(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.Log(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_LogWarning(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.LogWarning(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_LogError(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                UnityEngine.Debug.LogError(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }


        //----------------------------------------------------------------------
        [Conditional("ENABLE_LOG")]
        public static void Log(string message)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogTime() + message;
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(string tag, string message)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            message = GetLogText(tag, "", message);
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(string tag, string format, params object[] args)
        {
            if (!Debuger.EnableLog)
            {
                return;
            }

            string message = GetLogText(tag, "", string.Format(format, args));
            Internal_Log(Prefix + message);
            LogToFile("[I]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(string message)
        {
            message = GetLogTime() + message;
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(string tag, string message)
        {
            message = GetLogText(tag, "", message);
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(string tag, string format, params object[] args)
        {
            string message = GetLogText(tag, "", string.Format(format, args));
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(string message)
        {
            message = GetLogTime() + message;
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message,true);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(string tag, string message)
        {
            message = GetLogText(tag, "", message);
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message,true);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(string tag, string format, params object[] args)
        {
            string message = GetLogText(tag, "", string.Format(format, args));
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message,true);
        }


        //----------------------------------------------------------------------
        private static string GetLogText(string tag, string methodName, string message)
        {
            string str = GetLogTime();
            if( string.IsNullOrEmpty(methodName) )
                str = str + tag + "  " + message;
            else
                str = str + tag + "::" + methodName + "()  " + message;
            return str;
        }

        private static string GetLogTime()
        {
            string str = "";
            if (EnableTime)
            {
                str = DateTime.Now.ToString("HH:mm:ss.fff") + "  ";
            }
            return str;

        }

        //----------------------------------------------------------------------
        internal static string CheckLogFileDir()
        {
            if (string.IsNullOrEmpty(LogFileDir))
            {
                //该行代码无法在线程中执行！
                try
                {
                    if (UseUnityEngine)
                    {
                        if (UnityEngine.Application.isMobilePlatform)
                            LogFileDir = UnityEngine.Application.persistentDataPath + "/DebugerLog/";
                        else
                            LogFileDir = UnityEngine.Application.dataPath + "/../DebugerLog/";
                    }
                    else
                    {
                        LogFileDir = System.AppDomain.CurrentDomain.BaseDirectory + "/DebugerLog/";
                    }
                }
                catch (Exception e)
                {
                    Internal_LogError("Debuger::CheckLogFileDir()  " + e.Message + "\n" + e.StackTrace);
                    return "";
                }
            }

            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }
            }
            catch (Exception e)
            {
                Internal_LogError("Debuger::CheckLogFileDir()  " + e.Message + "\n" + e.StackTrace);
                return "";
            }

            return LogFileDir;
        }

        internal static string GenLogFileName()
        {
            DateTime now = DateTime.Now;
            string filename = now.GetDateTimeFormats('s')[0].ToString();//2005-11-05T14:06:25
            filename = filename.Replace("-", "_");
            filename = filename.Replace(":", "_");
            filename = filename.Replace(" ", "");
            filename += ".log";

            return filename;
        }

        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
            {
                return;
            }

            if (LogFileWriter == null)
            {
                LogFileName = GenLogFileName();
                LogFileDir = CheckLogFileDir();
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    return;
                }

                string fullpath = LogFileDir + LogFileName;
                try
                {
                    LogFileWriter = File.AppendText(fullpath);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                    Internal_LogError("Debuger::LogToFile()  " + e.Message + "\n" + e.StackTrace);
                    return;
                }
            }

            if (LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.WriteLine(message);
                    if ( (EnableStack || Debuger.EnableStack) && UseUnityEngine)
                    {
                        LogFileWriter.WriteLine(UnityEngine.StackTraceUtility.ExtractStackTrace());
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    
    }
}
