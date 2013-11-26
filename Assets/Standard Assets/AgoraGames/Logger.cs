using System;
using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class Logger
    {
        public enum Level
        {
            Info,
            Warn,
            Error
        }

        public delegate void LogHandler(Level level, string message);
        public event LogHandler Handler;

        public void Info(string message)
        {
            if (Handler != null)
            {
                Handler(Level.Info, message);
            }
        }

        public void Warn(string message)
        {
            if (Handler != null)
            {
                Handler(Level.Warn, message);
            }
        }

        public void Error(string message)
        {
            if (Handler != null)
            {
                Handler(Level.Error, message);
            }
        }
    }
}
