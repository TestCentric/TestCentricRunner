// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Globalization;
using System.Xml;
using NUnit;

namespace TestCentric.Engine.TestBed
{
    static class ResultReporter
    {
        private static ResultSummary Summary;

        public static void ReportResults(XmlNode resultNode)
        {
            Summary = new ResultSummary(resultNode);

            if (Summary.ExplicitCount + Summary.SkipCount + Summary.IgnoreCount > 0)
                DisplayNotRunReport(resultNode);

            if (Summary.FailureCount + Summary.ErrorCount + Summary.WarningCount + Summary.InvalidAssemblies > 0)
                DisplayErrorsFailuresAndWarningsReport(resultNode);

            DisplaySummaryReport(resultNode);
        }

        static void DisplayNotRunReport(XmlNode resultNode)
        {

        }

        static void DisplayErrorsFailuresAndWarningsReport(XmlNode resultNode)
        {
            Console.WriteLine("Errors, Failures and Warnings");
            int index = 0;
            DisplayErrorsFailuresAndWarnings(resultNode, ref index);
        }

        static void DisplayErrorsFailuresAndWarnings(XmlNode resultNode, ref int index)
        {
                string resultState = resultNode.GetAttribute("result");

                var name = resultNode.GetAttribute("fullname");
                var label = resultNode.GetAttribute("label");
                var status = label == null ? "FAILED" : label.ToUpper();
                var message =
                    resultNode.SelectSingleNode("failure/message")?.InnerText ??
                    resultNode.SelectSingleNode("reason/message")?.InnerText;

            switch (resultNode.Name)
            {
                case "test-case":
                    if (resultState == "Failed" || resultState == "Warning")
                    {
                        Console.WriteLine($"\n{++index}) {status} {name}");
                        if (message != null)
                            Console.WriteLine(message);
                    }
                    return;

                case "test-run":
                    foreach (XmlNode childResult in resultNode.ChildNodes)
                        DisplayErrorsFailuresAndWarnings(childResult, ref index);
                    break;

                case "test-suite":
                    if (resultState == "Failed" || resultState == "Warning")
                    {
                        var suiteType = resultNode.GetAttribute("type");
                        if (suiteType == "Theory")
                        {
                            // Report failure of the entire theory and then go on
                            // to list the individual cases that failed
                            Console.WriteLine($"\n{++index}) {status} {name}");
                            if (message != null)
                                Console.WriteLine(message);
                        }
                        else
                        {
                            // Where did this happen? Default is in the current test.
                            var site = resultNode.GetAttribute("site");

                            // Correct a problem in some framework versions, whereby warnings and some failures 
                            // are promulgated to the containing suite without setting the FailureSite.
                            if (site == null)
                            {
                                if (resultNode.SelectSingleNode("reason/message")?.InnerText == "One or more child tests had warnings" ||
                                    resultNode.SelectSingleNode("failure/message")?.InnerText == "One or more child tests had errors")
                                {
                                    site = "Child";
                                }
                                else
                                    site = "Test";
                            }

                            // Only report errors in the current test method, setup or teardown
                            if (site == "SetUp" || site == "TearDown" || site == "Test")
                            {
                                Console.WriteLine($"\n{++index}) {status} {name}");
                                if (message != null)
                                    Console.WriteLine(message);
                            }

                            // Do not list individual "failed" tests after a one-time setup failure
                            if (site == "SetUp") return;
                        }

                        foreach (XmlNode childResult in resultNode.ChildNodes)
                            DisplayErrorsFailuresAndWarnings(childResult, ref index);
                    }
                    break;
            }
        }

        static void DisplaySummaryReport(XmlNode resultNode)
        {
            string overallResult = resultNode.Attributes["result"]?.Value ?? "Unknown";
            string label = resultNode.Attributes["label"]?.Value;
            int totalFailed = Summary.FailureCount + Summary.InvalidCount + Summary.ErrorCount;
            int totalSkipped = Summary.SkipCount + Summary.IgnoreCount + Summary.ExplicitCount;

            Console.WriteLine("\nTest Run Summary");

            var overallResultLine = "  " +
                $"Test Count: {Summary.TestCount}, " +
                $"Overall Result: {overallResult}";
            if (label != null)
                overallResultLine += $" ({label})";
            Console.WriteLine(overallResultLine);
            
            Console.WriteLine("  " +
                $"Passed: {Summary.PassCount}, " +
                $"Failed: {totalFailed}, " +
                $"Warnings: {Summary.WarningCount}, " +
                $"Inconclusive: {Summary.InconclusiveCount}, " +
                $"Skipped: {totalSkipped}");

            if (totalFailed > 0)
                Console.WriteLine("    " +
                    $"Failed Tests - Failures: {Summary.FailureCount}, " +
                    $"Errors: {Summary.ErrorCount}, " +
                    $"Invalid: {Summary.InvalidCount}");

            if (totalSkipped > 0)
                Console.WriteLine("    " +
                    $"Skipped Tests - Ignored: {Summary.IgnoreCount}, " +
                    $"Explicit: {Summary.ExplicitCount}, " +
                    $"Other: {Summary.SkipCount}");

            var duration = resultNode.GetAttribute("duration", 0.0);
            var startTime = resultNode.GetAttribute("start-time", DateTime.MinValue);
            var endTime = resultNode.GetAttribute("end-time", DateTime.MaxValue);

            Console.WriteLine($"  Start time: {startTime.ToString("u")}");
            Console.WriteLine($"    End time: {endTime.ToString("u")}");
            Console.WriteLine($"    Duration: {string.Format(NumberFormatInfo.InvariantInfo, "{0:0.000} seconds", duration)}");
            Console.WriteLine();
        }
    }
}
