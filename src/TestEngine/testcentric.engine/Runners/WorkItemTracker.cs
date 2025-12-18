// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Xml;
using NUnit;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// Test Frameworks, don't always handle cancellation. Those that do may
    /// not always handle it correctly. In fact, even NUnit itself, through 
    /// at least release 3.12, fails to cancel a test that's in an infinite
    /// loop. In addition, it's possible for user code to defeat cancellation,
    /// even forced cancellation or kill.
    /// 
    /// The engine needs to protect itself from such problems by assuring that
    /// any test for which cancellation is requested is fully cancelled, with
    /// nothing left behind. Further, it needs to generate notifications to
    /// tell the runner what happened.
    /// 
    /// WorkItemTracker examines test events and keeps track of those tests
    /// that have started but not yet finished. It implements ITestEventListener
    /// in order to capture events and provides a property to return a list of 
    /// all item ids that have started but not yet finished and a Clear() method
    /// to clear the contents of that list. It allows waiting for all items to
    /// complete. Once the test has been cancelled, it provide notifications
    /// to the runner so the information may be displayed.
    /// </summary>
    internal class WorkItemTracker
    {
        private List<XmlNode> _itemsInProcess = new List<XmlNode>();

        public bool HasPendingItems => _itemsInProcess.Count > 0;

        public void Clear()
        {
            _itemsInProcess.Clear();
        }

        public IEnumerable<XmlNode> CreateCompletionNotifications()
        {
            // Generate completion notification for all pending items, in reverse order
            int count = _itemsInProcess.Count;
            
            while (--count >= 0)
            {
                var startElement = _itemsInProcess[count];
                _itemsInProcess.RemoveAt(count);
                yield return CreateCompletionNotification(startElement);
            }
        }

        private static XmlNode CreateCompletionNotification(XmlNode startElement)
        {
            bool isSuite = startElement.Name == "start-suite";

            XmlNode notification = XmlHelper.CreateTopLevelElement(isSuite ? "test-suite" : "test-case");
            if (isSuite)
                notification.AddAttribute("type", startElement.GetAttribute("type"));
            notification.AddAttribute("id", startElement.GetAttribute("id"));
            notification.AddAttribute("name", startElement.GetAttribute("name"));
            notification.AddAttribute("fullname", startElement.GetAttribute("fullname"));
            notification.AddAttribute("result", "Failed");
            notification.AddAttribute("label", "Cancelled");
            XmlNode failure = notification.AddElement("failure");
            XmlNode message = failure.AddElementWithCDataSection("message", "Test run cancelled by user");
            return notification;
        }

        public void AddItem(XmlNode xmlNode)
        {
            _itemsInProcess.Add(xmlNode);
        }

        public void RemoveItem(string id)
        {
            foreach (XmlNode item in _itemsInProcess)
            {
                if (item.GetAttribute("id") == id)
                {
                    _itemsInProcess.Remove(item);

                    return;
                }
            }
        }
    }
}
