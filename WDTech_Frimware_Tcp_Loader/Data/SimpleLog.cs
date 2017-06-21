using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

namespace WDTech_Frimware_Tcp_Loader.Data
{
    public class SimpleLog
    {
        #region Private Fileds
        private static readonly string LogFilePath;

        private static readonly string LogFileName;

        #endregion
        static SimpleLog()
        {
            LogFilePath = ConfigurationManager.AppSettings.Get("SampleLogFilePath");
            if (string.IsNullOrWhiteSpace(LogFilePath))
            {
                throw new InvalidDataException("Log File Path Should Not be NULL");
            }

            LogFileName = $"{LogFilePath}\\SimpleLog_{DateTime.Now:yyyy_MM_dd}.log";
            EnsureDirectory();
        }

        public static void Info(string message, Exception exception = null)
        {
            WriteLog(FormatMessage(LogCat.Error, message, exception));
        }

        public static void Warn(string message, Exception exception = null)
        {
            WriteLog(FormatMessage(LogCat.Error, message, exception));
        }

        public static void Error(string message, Exception exception = null)
        {
            WriteLog(FormatMessage(LogCat.Error, message, exception));
        }

        public static void Fatal(string message, Exception exception = null)
        {
            WriteLog(FormatMessage(LogCat.Error, message, exception));
        }

        #region Private Methods

        private static void EnsureDirectory()
        {
            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }
            if (File.Exists(LogFileName)) return;
            var stream = File.Create(LogFileName);
            stream.Dispose();
        }


        private static StringBuilder FormatMessage(LogCat cat, string message, Exception ex)
        {
            var builder = new StringBuilder();
            builder.Append("####################日志开始####################\r\n");
            builder.Append($"日志时间：{DateTime.Now: yyyy-MM-dd HH:mm:ss fff}。线程ID：{Thread.CurrentContext.ContextID}。\r\n");
            builder.Append($"日志级别：{GetLogCat(cat)}。\r\n");
            builder.Append($"日志消息：{message}\r\n");
            if (ex != null)
            {
                builder.Append("----------异常信息----------\r\n");
                builder.Append($"{ex.Message}\r\n");
                builder.Append("----------调用堆栈----------\r\n");
                builder.Append($"{ex.StackTrace}\r\n");
            }
            builder.Append("####################日志结束####################\r\n");

            return builder;
        }

        private static string GetLogCat(LogCat cat)
        {
            switch (cat)
            {
                case LogCat.Info:
                    return "信息";
                case LogCat.Warn:
                    return "警告";
                case LogCat.Error:
                    return "错误";
                case LogCat.Fatal:
                    return "严重错误";
                default:
                    return "一般信息";
            }
        }

        private static void WriteLog(StringBuilder builder)
        {
            using (var streamWriter = File.AppendText(LogFileName))
            {
                streamWriter.Write(builder.ToString());
            }
        }
        #endregion
    }

    public enum LogCat
    {
        Info,

        Warn,

        Error,

        Fatal
    }
}
