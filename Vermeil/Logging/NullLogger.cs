#region

using System;

#endregion

namespace Vermeil.Logging
{
    public class NullLogger : ILogger
    {
        public void Trace(string message, Exception ex = null)
        {
        }

        public void Debug(string message, Exception ex = null)
        {
        }

        public void Info(string message, Exception ex = null)
        {
        }

        public void Warning(string message, Exception ex = null)
        {
        }

        public void Error(string message, Exception ex = null)
        {
        }

        public void Fatal(string message, Exception ex = null)
        {
        }
    }
}