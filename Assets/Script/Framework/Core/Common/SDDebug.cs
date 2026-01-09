//#define DEBUG_LEVEL_NORMAL // 普通
//#define DEBUG_LEVEL_WARNING // 警告
//#define DEBUG_LEVEL_ERROR // 错误
//#define DEBUG_LEVEL_EXCEPTION // 异常
//#define DEBUG_LEVEL_ASSERT // 断言
// 日志开关
// DEBUG_LEVEL_NORMAL;DEBUG_LEVEL_WARNING;DEBUG_LEVEL_ERROR;
//DEBUG_LEVEL_EXCEPTION;DEBUG_LEVEL_ASSERT;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;


public static class SDDebug
{
    private static object mThreadLock = new object();
    private static List<string> _outPutLog = new List<string>();
    private static List<string> _outPutLogBuffer = new List<string>();
    private static Thread _thread;
    private static string _logfilePath;
    private static bool mIsInit = false;

    public static void Init(string logPath)
    {
        _logfilePath = logPath;
        if (_thread == null)
        {
            mIsInit = true;
            _thread = new Thread(new ThreadStart(ThreadRecv));
            _thread.IsBackground = true;
            _thread.Start();

            //Application.logMessageReceived += Application_Log;
            Application.logMessageReceivedThreaded += Application_Log;
        }
    }

    private static void Application_Log(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                AddToPutLog(string.Format("Error: {0}\nStack: {1}", condition, stackTrace));
                break;
            case LogType.Exception:
                AddToPutLog(string.Format("Exception: {0}\nStack: {1}", condition, stackTrace));
                break;
        }
    }
    public static void AddToPutLog(string log, params object[] args)
    {
        AddToPutLog(string.Format(log, args));
    }

    public static void AddToPutLog(string log)
    {
        if (mIsInit)
        {
            lock (mThreadLock)
            {
                _outPutLog.Add(log);
            }
        }
    }

    public static void Clear()
    {
        mIsInit = false;
        _thread.Abort();
        LogShootClear();
    }

    public static void LogInFile(string message, bool reWrite = false)
    {
        string filePath;
#if UNITY_EDITOR
        filePath = Application.dataPath + "/StreamingAssets/log.txt";
#else
        filePath = Application.persistentDataPath + "/log.txt";
#endif

        StreamWriter sw;
        FileInfo fi = new FileInfo(filePath);
        if (reWrite)
            sw = fi.CreateText();        //直接重新写入，如果要在原文件后面追加内容，应用fi.AppendText()
        else
            sw = fi.AppendText();
        sw.WriteLine(message);
        sw.Close();
        sw.Dispose();

    }

    public static void ThreadRecv()
    {
        StreamWriter writer = new StreamWriter(_logfilePath, true, Encoding.UTF8);

        //bool write = false;
        int step = 10;
        while (true)
        {
            Thread.Sleep(100);
            lock (mThreadLock)
            {
                _outPutLogBuffer.AddRange(_outPutLog);
                _outPutLog.Clear();
            }

            for (int i = 0; i < _outPutLogBuffer.Count; i++)
            {
                writer.WriteLine(_outPutLogBuffer[i]);
            }
            _outPutLogBuffer.Clear();

            step--;
            if (step <= 0)
            {
                //writer.WriteLine(DateTime.Now.ToString("tick :----------------yy_MM_dd_hh_mm_ss"));
                step = 10;
            }


            writer.Flush();
        }
    }

    private const string mStrLogModel = "Log Model: ";

    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void Log(string message)
    {
        Log_Force(mStrLogModel + message);
    }
    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void Log(string message, UnityEngine.Object context)
    {
        Log_Force(mStrLogModel + message, context);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void LogFormat(string format, params object[] args)
    {
        Debug.LogFormat(mStrLogModel + format, args);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_WARNING")]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(mStrLogModel + message);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_WARNING")]
    public static void LogWarning(string message, UnityEngine.Object context)
    {
        Debug.LogWarning(mStrLogModel + message, context);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_WARNING")]
    public static void LogWarningFormat(string format, params object[] args)
    {
        Debug.LogWarningFormat(mStrLogModel + format, args);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_ERROR")]
    public static void LogError(string message)
    {
        LogError_Force(mStrLogModel + message);
    }
    [System.Diagnostics.Conditional("DEBUG_LEVEL_ERROR")]
    public static void LogError(string message, UnityEngine.Object context)
    {
        LogError_Force(mStrLogModel + message, context);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_ERROR")]
    public static void LogErrorFormat(string format, params object[] args)
    {
        LogErrorFormat_Force(mStrLogModel + format, args);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_ERROR")]
    public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
    {
        LogErrorFormat_Force(mStrLogModel + format, args);
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void LogCodeState(string content)
    {
        if (mIsInit)
        {
            string log = FormatLog("CodeState", content);
            AddToPutLog(log);
        }
    }
    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void LogInfo(string content)
    {
        Debug.Log(content);
        if (mIsInit)
        {
            string log = FormatLog("CodeState", content);
            AddToPutLog(log);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_LEVEL_NORMAL")]
    public static void LogInfoFormat(string content, params object[] args)
    {
        var str = string.Format(content, args);
        Debug.Log(str);
        if (mIsInit)
        {
            string log = FormatLog("CodeState", str);
            AddToPutLog(log);
        }
    }

    private static string FormatLog(string type, string content)
    {
        return string.Format("SDDebug##{2}:{0}:{1}", DateTime.Now, content, type);
    }

    private enum ELogType
    {
        Debug,
        Warning,
        Error,
    }

    public static void LogError_Force(string message)
    {
        //string log = FormatLog("Error: ", message == null ? "null" : message.ToString());
        //AddToPutLog(log);
        Debug.LogError(message);
    }

    public static void LogError_Force(string message, UnityEngine.Object context)
    {
        //string log = FormatLog("Error: ", message == null ? "null" : message.ToString());
        //AddToPutLog(log);
        Debug.LogError(message, context);
    }

    public static void LogErrorFormat_Force(string format, params object[] args)
    {
        LogError_Force(string.Format(format, args));
    }

    public static void Log_Force(string message)
    {
        string log = FormatLog("Log: ", message == null ? "null" : message.ToString());
        AddToPutLog(log);
        Debug.Log(message);
    }

    public static void Log_Force(string message, UnityEngine.Object context)
    {
        string log = FormatLog("Log: ", message == null ? "null" : message);
        AddToPutLog(log);
        Debug.Log(message, context);
    }

    public static void LogFormat_Force(string format, params object[] args)
    {
        Log_Force(string.Format(format, args));
    }

    public static void LogWarning_Force(string message)
    {
        string log = FormatLog("Warning: ", message == null ? "null" : message.ToString());
        AddToPutLog(log);
        Debug.LogWarning(message);
    }

    public static void LogWarningFormat_Force(string format, params object[] args)
    {
        LogWarning_Force(string.Format(format, args));
    }


    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogInfoFormat_Editor(string content, params object[] args)
    {
        var str = string.Format(content, args);
        Debug.Log(str);
        if (mIsInit)
        {
            string log = FormatLog("CodeState", str);
            AddToPutLog(log);
        }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogInfo_Editor(string content)
    {
        Debug.Log(content);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError_Editor(string content)
    {
        Debug.LogError(content);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError_BatBuild(string content)
    {
        if (Environment.GetCommandLineArgs().Length > 0)
            throw new Exception("-------------UnityError---------------\n" + FormatLog("CodeState", content));
        else
            throw new Exception(content);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogErrorFormat_BatBuild(string content, params object[] args)
    {
        var str = string.Format(content, args);
        if (Environment.GetCommandLineArgs().Length > 0)
            throw new Exception("-------------UnityError---------------\n" + FormatLog("CodeState", str));
        else
            throw new Exception(str);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log_BatBuild(string content)
    {
        if (Environment.GetCommandLineArgs().Length > 0)
            Debug.Log("-------------UnityLog---------------\n" + FormatLog("CodeState", content));
        else
            Debug.Log(content);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogFormat_BatBuild(string content, params object[] args)
    {
        var str = string.Format(content, args);
        if (Environment.GetCommandLineArgs().Length > 0)
            Debug.Log("-------------UnityLog---------------\n" + FormatLog("CodeState", str));
        else
            Debug.Log(str);
    }

    #region 命中率打印
    private const string LogShootCondition = "UNITY_EDITOR";
    private static bool mIsLogShootInit = false;
    private static string mLogShootPath;
    private static List<string> mOutPutShootLog = new List<string>();
    private static List<string> mOutPutShootLogBuffer = new List<string>();
    private static Thread mLogShootThread;
    private static object mLogShootThreadLock = new object();
    private static bool mLogShootEnable = false;

    [System.Diagnostics.Conditional(LogShootCondition)]
    public static void InitLogShoot(bool logEnable, string logShootPath, string logShootFilePath)
    {
        mLogShootEnable = logEnable;
        if (!mLogShootEnable)
        {
            return;
        }

        if (mIsLogShootInit)
        {
            return;
        }

        if (!Directory.Exists(logShootFilePath))
        {
            Directory.CreateDirectory(logShootFilePath);
        }

        mLogShootPath = logShootPath;
        mIsLogShootInit = true;
        if (mLogShootThread == null)
        {
            mLogShootThread = new Thread(new ThreadStart(LogShootThreadRecv));
        }
        mLogShootThread.IsBackground = true;
        mLogShootThread.Start();
    }

    [System.Diagnostics.Conditional(LogShootCondition)]
    public static void LogShootFile(string message)
    {
        if (!mLogShootEnable)
        {
            return;
        }

        string log = string.Format("{0}:{1}", DateTime.Now, message ?? "null");
        if (mIsLogShootInit)
        {
            lock (mLogShootThreadLock)
            {
                mOutPutShootLog.Add(log);
            }
        }
        //Debug.Log(message);
    }

    //[System.Diagnostics.Conditional(LogShootCondition)]
    private static void LogShootThreadRecv()
    {
        if (!mLogShootEnable)
        {
            return;
        }

        StreamWriter logShootWriter = new StreamWriter(mLogShootPath, true, Encoding.UTF8);

        int step = 10;
        while (true)
        {
            Thread.Sleep(100);
            lock (mLogShootThreadLock)
            {
                mOutPutShootLogBuffer.AddRange(mOutPutShootLog);
                mOutPutShootLog.Clear();
            }

            for (int i = 0, len = mOutPutShootLogBuffer.Count; i < len; i++)
            {
                logShootWriter.WriteLine(mOutPutShootLogBuffer[i]);
            }
            mOutPutShootLogBuffer.Clear();

            step--;
            if (step <= 0)
            {
                //writer.WriteLine(DateTime.Now.ToString("tick :----------------yy_MM_dd_hh_mm_ss"));
                step = 10;
            }


            logShootWriter.Flush();
        }
    }

    [System.Diagnostics.Conditional(LogShootCondition)]
    private static void LogShootClear()
    {
        if (!mLogShootEnable)
        {
            return;
        }
        mLogShootEnable = false;
        mIsLogShootInit = false;
        mLogShootThread.Abort();
    }
    #endregion
}