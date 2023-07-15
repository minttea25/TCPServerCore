using System;
using System.IO;
using System.Text;

using static ServerCoreTCP.ILogger;

namespace ServerCoreTCP
{
    public interface ILogger
    {
        public enum LogHeader
        {
            DEBUG = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3,
        }

        public void LogDebug(string log);
        public void LogInfo(string log);
        public void LogWarning(string log);
        public void LogError(string log);
    }

    public class Logger : ILogger
    {
        #region Singleton
        public static Logger Instance => _instance;
        static readonly Logger _instance = new();
        #endregion

        public Encoding Encoding { get; set; } = Encoding.Unicode;
        public string DirNameFormat { get; set; } = "yyyy_MM_dd";
        public string FileNameFormat { get; set; } = "yyyy_MM_dd_HH_mm";
        public string LogTimeFormat { get; set; } = "HH:mm:ss";

        string _logFileExtension = ".txt";
        string _dir_path = "Logs";
        readonly string _logDir;
        readonly string _fileName;

        readonly object _lock = new();

        string FullPath
        {
            get
            {
                return _dir_path + Path.DirectorySeparatorChar 
                    + _logDir + Path.DirectorySeparatorChar 
                    + _fileName + _logFileExtension;
            }
        }

        StreamWriter _sw = null;
        StreamWriter Sw
        {
            get
            {
                if (_sw == null)
                {
                    CheckDirectories();
                    _sw = new(FullPath, true, Encoding);
                }
                return _sw;
            }
        }

        Logger()
        {
            _logDir = DateTime.Today.ToString(DirNameFormat);
            _fileName = DateTime.Now.ToString(FileNameFormat);
        }

        // Note: In C#, finalizer of an object ia not called automatically when it goes out of scope.
        // However, finalizers are called by the garbage collector when the object is no longer needed.
        ~Logger()
        {
            _sw.Close();
            _sw = null;
        }

        public void SetLogFormat(string formatText)
        {
            _logFileExtension = $".{formatText}";
        }

        public void LogDebug(string log) => Log(log, LogHeader.DEBUG);
        public void LogInfo(string log) => Log(log, LogHeader.INFO);
        public void LogWarning(string log) => Log(log, LogHeader.WARNING);
        public void LogError(string log) => Log(log, LogHeader.ERROR);

        public void Log(string log, LogHeader header)
        {
            lock (_lock)
            {
                string l = GetLogFormat(log, header);
                Sw.WriteLine(l);
                Sw.Flush();
            }
        }

        void CheckDirectories()
        {
            string dir = _dir_path + Path.DirectorySeparatorChar + _logDir;
            if (Directory.Exists(dir) == false)
            {
                _ = Directory.CreateDirectory(dir);
            }
        }

        string GetLogFormat(string log, LogHeader header) => header switch
        {
            LogHeader.DEBUG => $@"[DEBUG] [{DateTime.Now.ToString(LogTimeFormat)}] {log}",
            LogHeader.INFO => $@"[INFO] [{DateTime.Now.ToString(LogTimeFormat)}] {log}",
            LogHeader.WARNING => $@"[WARNING] [{DateTime.Now.ToString(LogTimeFormat)}] {log}",
            LogHeader.ERROR => $@"[ERROR] [{DateTime.Now.ToString(LogTimeFormat)}] {log}",
            _ => throw new NotImplementedException(),
        };
    }
}
