// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Xml;
using NUnit;

namespace TestCentric.Engine.TestBed
{
    class ResultSummary
    {
        public int TestCount = 0;
        public int PassCount = 0;
        public int FailureCount = 0;
        public int WarningCount = 0;
        public int ErrorCount = 0;
        public int InconclusiveCount = 0;
        public int SkipCount = 0;
        public int IgnoreCount = 0;
        public int ExplicitCount = 0;
        public int InvalidCount = 0;
        public int InvalidAssemblies = 0;
        public int InvalidTestFixtures = 0;
        public bool UnexpectedError;

        public ResultSummary(XmlNode result)
        {
            if (result.Name != "test-run")
                throw new InvalidOperationException("Expected <test-run> as top-level element but was <" + result.Name + ">");

            Summarize(result, false);
        }

        private void Summarize(XmlNode node, bool failedInFixtureTearDown)
        {
            string type = node.GetAttribute("type");
            string status = node.GetAttribute("result");
            string label = node.GetAttribute("label");
            string site = node.GetAttribute("site");

            switch (node.Name)
            {
                case "test-case":
                    TestCount++;

                    switch (status)
                    {
                        case "Passed":
                            if (failedInFixtureTearDown)
                                ErrorCount++;
                            else
                                PassCount++;
                            break;
                        case "Failed":
                            if (failedInFixtureTearDown)
                                ErrorCount++;
                            else if (label == null)
                                FailureCount++;
                            else if (label == "Invalid")
                                InvalidCount++;
                            else
                                ErrorCount++;
                            break;
                        case "Warning":
                            if (failedInFixtureTearDown)
                                ErrorCount++;
                            else
                                WarningCount++;
                            break;
                        case "Inconclusive":
                            if (failedInFixtureTearDown)
                                ErrorCount++;
                            else
                                InconclusiveCount++;
                            break;
                        case "Skipped":
                            if (label == "Ignored")
                                IgnoreCount++;
                            else if (label == "Explicit")
                                ExplicitCount++;
                            else
                                SkipCount++;
                            break;
                        default:
                            //_skipCount++;
                            break;
                    }
                    break;

                case "test-suite":
                    if (status == "Failed" && label == "Invalid")
                    {
                        if (type == "Assembly")
                            InvalidAssemblies++;
                        else
                            InvalidTestFixtures++;
                    }
                    if (type == "Assembly" && status == "Failed" && label == "Error")
                    {
                        InvalidAssemblies++;
                        UnexpectedError = true;
                    }
                    if ((type == "SetUpFixture" || type == "TestFixture") && status == "Failed" && label == "Error" && site == "TearDown")
                    {
                        failedInFixtureTearDown = true;
                    }

                    Summarize(node.ChildNodes, failedInFixtureTearDown);
                    break;

                case "test-run":
                    Summarize(node.ChildNodes, failedInFixtureTearDown);
                    break;
            }
        }

        private void Summarize(XmlNodeList nodes, bool failedInFixtureTearDown)
        {
            foreach (XmlNode childResult in nodes)
                Summarize(childResult, failedInFixtureTearDown);
        }
    }
}
