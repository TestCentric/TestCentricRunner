// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using TestCentric.Engine.Internal;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// TestEventDispatcher is used to send test events to a number of listeners
    /// </summary>
    class TestEventDispatcher : MarshalByRefObject, ITestEventListener, IService
    {
        private object _eventLock = new object();
        private ExtensionService _extensionService;
        private List<ITestEventListener> _listenerExtensions = new List<ITestEventListener>();
        private WorkItemTracker _workItemTracker = new WorkItemTracker();
        private ManualResetEvent _allItemsComplete = new ManualResetEvent(false);
        private bool _runCancelled;

        public TestEventDispatcher()
        {
            Listeners = new List<ITestEventListener>();
        }

        public IList<ITestEventListener> Listeners { get; private set; }

        public void InitializeForRun()
        {
            _runCancelled = false;
            _workItemTracker.Clear();
            _allItemsComplete.Reset();
            Listeners = new List<ITestEventListener>(_listenerExtensions);
        }

        public bool WaitForCompletion(int millisecondsTimeout)
        {
            return _allItemsComplete.WaitOne(millisecondsTimeout);
        }

        public void IssuePendingNotifications()
        {
            lock (_eventLock)
            {
                _runCancelled = true;
                foreach(XmlNode notification in _workItemTracker.CreateCompletionNotifications())
                    DispatchEvent(notification.OuterXml);

                _allItemsComplete.Set();
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #region ITestEventListener Implementation

        public void OnTestEvent(string report)
        {
            lock (_eventLock)
            {
                if (!_runCancelled)
                    DispatchEvent(report);
            }
        }

        internal void DispatchEvent(string report)
        {
            foreach (var listener in Listeners)
                listener.OnTestEvent(report);

            XmlNode xmlNode = XmlHelper.CreateXmlNode(report);

            switch (xmlNode.Name)
            {
                case "start-test":
                case "start-suite":
                    _workItemTracker.AddItem(xmlNode);
                    break;

                case "test-case":
                case "test-suite":
                    string id = xmlNode.GetAttribute("id");
                    _workItemTracker.RemoveItem(id);

                    if (!_workItemTracker.HasPendingItems)
                        _allItemsComplete.Set();
                    break;
            }
        }

        #endregion

        #region IService Implementation

        public IServiceLocator ServiceContext { get; set; }

        public ServiceStatus Status { get; private set; }

        void IService.StartService()
        {
            _extensionService = ServiceContext.GetService<ExtensionService>();
            foreach (var extension in _extensionService.GetExtensions<ITestEventListener>())
                _listenerExtensions.Add(extension);
            foreach (var extension in _extensionService.GetExtensions<NUnit.Engine.ITestEventListener>())
                _listenerExtensions.Add(new TestEventListenerWrapper(extension));

            Status = _extensionService == null
                ? ServiceStatus.Error
                : ServiceStatus.Started;
        }

        void IService.StopService()
        {
            Status = ServiceStatus.Stopped;
        }

        #endregion
    }
}
