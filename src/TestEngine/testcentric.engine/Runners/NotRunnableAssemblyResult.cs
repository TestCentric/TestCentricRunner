// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Collections.Generic;

namespace TestCentric.Engine.Runners
{
    public class NotRunnableAssemblyResult
    {
        private string _name;
        private string _fullname;
        private string _message;
        private string _type;

        protected string _runstate;
        protected string _result;
        protected string _label;

        public NotRunnableAssemblyResult(string assemblyPath, string message)
        {
            _name = Escape(Path.GetFileName(assemblyPath));
            _fullname = Escape(Path.GetFullPath(assemblyPath));
            _message = Escape(message);
            _type = new List<string> { ".dll", ".exe" }.Contains(Path.GetExtension(assemblyPath)) ? "Assembly" : "Unknown";
        }

        public string TestID { get; set; }

        public string LoadResult =>
            $"<test-suite type='{_type}' id='{TestID}' name='{_name}' fullname='{_fullname}' testcasecount='0' runstate='{_runstate}'>" +
                "<properties>" +
                    $"<property name='_SKIPREASON' value='{_message}'/>" +
                "</properties>" +
            "</test-suite>";

        public string RunResult =>
            $"<test-suite type='{_type}' id='{TestID}' name='{_name}' fullname='{_fullname}' testcasecount='0' runstate='{_runstate}' result='{_result}' label='{_label}'>" +
                "<properties>" +
                    $"<property name='_SKIPREASON' value='{_message}'/>" +
                "</properties>" +
                "<reason>" +
                    $"<message>{_message}</message>" +
                "</reason>" +
            "</test-suite>";

        private static string Escape(string original)
        {
            return original
                .Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }

    public class InvalidAssemblyResult : NotRunnableAssemblyResult
    {
        public InvalidAssemblyResult(string assemblyPath, string message)
            : base(assemblyPath, message)
        {
            _runstate = "NotRunnable";
            _result = "Failed";
            _label = "Invalid";
        }
    }

    public class SkippedAssemblyResult : NotRunnableAssemblyResult
    {
        public SkippedAssemblyResult(string assemblyPath)
            : base(assemblyPath, "Skipping non-test assembly")
        {
            _runstate = "Runnable";
            _result = "Skipped";
            _label = "NoTests";
        }
    }
}
