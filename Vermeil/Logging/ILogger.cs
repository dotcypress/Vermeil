#region

using System;

#endregion

namespace Vermeil.Logging
{
    public interface ILogger
    {
        void Trace(string message, Exception ex = null);
        void Debug(string message, Exception ex = null);
        void Info(string message, Exception ex = null);
        void Warning(string message, Exception ex = null);
        void Error(string message, Exception ex = null);
        void Fatal(string message, Exception ex = null);
    }
}