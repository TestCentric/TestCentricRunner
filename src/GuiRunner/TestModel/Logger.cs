// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************
#if false
using System;
using NUnit.Engine;

namespace TestCentric.Gui
{
    public class Logger
    {
        public Logger(string name)
        {
            FullName = Name = name;
            int index = FullName.LastIndexOf('.');
            if (index >= 0)
                Name = FullName.Substring(index + 1);
        }

        #region Properties

        public string Name { get; }

        public string FullName { get; }

        #endregion

        #region Error

        public void Error(string message)
        {
            Log(InternalTraceLevel.Error, message);
        }

        public void Error(string message, params object[] args)
        {
            Log(InternalTraceLevel.Error, message, args);
        }

        public void Error(string message, Exception ex)
        {
            Log(message, ex);
        }

        #endregion

        #region Warning
        public void Warning(string message)
        {
            Log(InternalTraceLevel.Warning, message);
        }

        public void Warning(string message, params object[] args)
        {
            Log(InternalTraceLevel.Warning, message, args);
        }
        #endregion

        #region Info
        public void Info(string message)
        {
            Log(InternalTraceLevel.Info, message);
        }

        public void Info(string message, params object[] args)
        {
            Log(InternalTraceLevel.Info, message, args);
        }
        #endregion

        #region Debug
        public void Debug(string message)
        {
            Log(InternalTraceLevel.Verbose, message);
        }

        public void Debug(string message, params object[] args)
        {
            Log(InternalTraceLevel.Verbose, message, args);
        }
        #endregion

        #region Helper Methods
        public void Log(InternalTraceLevel level, string message)
        {
            if (InternalTrace.Level >= level)
                InternalTrace.Log(level, message, Name);
        }

        private void Log(InternalTraceLevel level, string format, params object[] args)
        {
            if (InternalTrace.Level >= level)
                Log(level, string.Format(format, args));
        }

        public void Log(string message, Exception ex)
        {
            if (InternalTrace.Level >= InternalTraceLevel.Error)
            {
                InternalTrace.Log(InternalTraceLevel.Error, message, Name, ex);
            }
        }

        #endregion
    }
}
#endif
