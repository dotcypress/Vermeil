#region

using System;

#endregion

namespace Vermeil.Logging
{
    internal class LogItem
    {
        public LogItem(string message, LogType logType, string exception = null)
        {
            Message = message;
            LogType = logType;
            Exception = exception;
        }

        public string Message { get; set; }

        public LogType LogType { get; set; }

        public string Exception { get; set; }

        public override string ToString()
        {
            return (Exception == null
                ? string.Format("[{0}]\t: {1}\t > {2}", DateTime.Now, LogType, Message)
                : string.Format("[{0}]\t: {1}\t > {2}\n--------\n{3}\n--------", DateTime.Now, LogType, Message, Exception));
        }
    }
}
