// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;

namespace TestCentric.Engine.Runners
{
    public class TestExecutionTask : ITestExecutionTask
    {
        private readonly NUnit.Engine.ITestEngineRunner _runner;
        private readonly NUnit.Engine.ITestEventListener _listener;
        private readonly NUnit.Engine.TestFilter _filter;
        private volatile NUnit.Engine.TestEngineResult _result;
        private readonly bool _disposeRunner;
        private bool _hasExecuted = false;
        private Exception _unloadException;

        public TestExecutionTask(NUnit.Engine.ITestEngineRunner runner, NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter, bool disposeRunner)
        {
            _disposeRunner = disposeRunner;
            _filter = filter;
            _listener = listener;
            _runner = runner;
        }

        public void Execute()
        {
            _hasExecuted = true;
            try
            {
                _result = _runner.Run(_listener, _filter);
            }
            finally
            {
                try
                {
                    if (_disposeRunner)
                        _runner.Dispose();
                }
                catch (Exception e)
                {
                    _unloadException = e;
                }
            }
        }

        public NUnit.Engine.TestEngineResult Result
        {
            get
            {
                Guard.OperationValid(_hasExecuted, "Can not access result until task has been executed");
                return _result;
            }
        }

        /// <summary>
        /// Stored exception thrown during test assembly unload.
        /// </summary>
        public Exception UnloadException
        {
            get
            {
                Guard.OperationValid(_hasExecuted, "Can not access thrown exceptions until task has been executed");
                return _unloadException;
            }
        }
    }
}
