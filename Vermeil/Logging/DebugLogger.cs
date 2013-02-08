#define DEBUG

#region

using System;

#endregion

namespace Vermeil.Logging
{
    public class DebugLogger : ILogger
    {
        public void Trace(string message, Exception ex = null)
        {
            WriteLine(LogType.Trace, message, ex);
        }

        public void Debug(string message, Exception ex = null)
        {
            WriteLine(LogType.Debug, message, ex);
        }

        public void Info(string message, Exception ex = null)
        {
            WriteLine(LogType.Info, message, ex);
        }

        public void Warning(string message, Exception ex = null)
        {
            WriteLine(LogType.Warn, message, ex);
        }

        public void Error(string message, Exception ex = null)
        {
            WriteLine(LogType.Error, message, ex);
        }

        public void Fatal(string message, Exception ex = null)
        {
            WriteLine(LogType.Fatal, message, ex);
        }

        private static void WriteLine(LogType logType, string message, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(new LogItem(message, logType, ex != null ? ex.ToString() : null));
        }
    }
}