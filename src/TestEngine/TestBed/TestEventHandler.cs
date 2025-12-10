// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Xml;
using NUnit;

namespace TestCentric.Engine.TestBed
{
    delegate void StartRunHandler();


    class TestEventHandler : NUnit.Engine.ITestEventListener
    {
        public event StartRunHandler RunStarted;

        string _lastTestOutput = null;

        public void OnTestEvent(string report)
        {
            var doc = new XmlDocument();
            doc.LoadXml(report);

            var testEvent = doc.FirstChild;
            switch (testEvent.Name)
            {
                case "start-run":
                    RunStarted?.Invoke();
                    break;

                case "test-suite":
                    if (testEvent.GetAttribute("type") == "Assembly" && testEvent.GetAttribute("runstate") == "NotRunnable")
                    {
                        var msg = testEvent.SelectSingleNode("reason/message")?.InnerText;
                        if (msg != null)
                            Console.WriteLine(msg);
                    }
                    break;

                case "test-output":
                    if (testEvent.GetAttribute("stream") == "Progress")
                    {
                        var testname = testEvent.GetAttribute("testname");

                        if (testname != _lastTestOutput)
                        {
                            Console.WriteLine($"\n==> {testname}");
                            _lastTestOutput = testname;
                        }

                        Console.Write(testEvent.InnerText);
                    }
                    break;

                case "unhandled-exception":
                    Console.WriteLine(testEvent.GetAttribute("message"));
                    break;
            }
        }
    }
}
