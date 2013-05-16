#define DEBUG

#region

using System;
using System.Collections.Generic;

#endregion

namespace Vermeil.Logging
{
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers;

        public CompositeLogger(params ILogger[] loggers)
        {
            _loggers = new List<ILogger>(loggers);
        }

        public void Trace(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Trace(message, ex));
        }

        public void Debug(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Debug(message, ex));
        }

        public void Info(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Info(message, ex));
        }

        public void Warning(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Warning(message, ex));
        }

        public void Error(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Error(message, ex));
        }

        public void Fatal(string message, Exception ex = null)
        {
            _loggers.ForEach(x => x.Fatal(message, ex));
        }
    }
}
