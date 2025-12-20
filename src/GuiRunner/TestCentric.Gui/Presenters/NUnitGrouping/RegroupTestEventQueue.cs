// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************


using System.Collections.Generic;
using System;
using TestCentric.Gui.Model;
using System.Linq;
using System.Timers;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{

    /// <summary>
    /// This class is introduced to improve the TreeView performance for regrouping
    /// It introduces bulk processing of test events instead of processing test events one-by-one.
    /// This means that the TreeView only needs to be updated once instead of many times.
    /// 
    /// For this purpose this class uses
    /// - a queue containing the recorded test nodes of the test events
    /// - a Timer to trigger the bulk processing
    /// </summary>
    internal class RegroupTestEventQueue
    {
        private Timer _timer;
        private object _lockObj = new object();

        public RegroupTestEventQueue(ReGrouping regrouping)
        {
            Regrouping = regrouping;
            Queue = new List<TestNode>();
        }

        private ReGrouping Regrouping { get; }

        private IList<TestNode> Queue { get; }

        /// <summary>
        /// Checks if a testNode needs to be added to the queue
        /// For example: if TestNode is already in correct TestGroup, there's no regrouping to be executed
        /// </summary>
        internal bool IsQueueRequired(TestNode testNode)
        {
            lock (_lockObj)
            {
                return testNode.IsSuite ? Queue.Any() : Regrouping.IsRegroupRequired(testNode);
            }
        }

        /// <summary>
        /// Add one TestNode to the queue
        /// </summary>
        internal void AddToQueue(TestNode testNode)
        {
            lock (_lockObj)
            {
                Queue.Add(testNode);

                if (_timer == null)
                {
                    _timer = new Timer();
                    _timer.Interval = 800;
                    _timer.Elapsed += TimerTimeout;
                    _timer.Start();
                }
            };
        }

        internal void ForceStopTimer()
        {
            lock (_lockObj)
            {
                if (_timer != null)
                    TimerTimeout(null, new EventArgs());
            }
        }

        private void TimerTimeout(object sender, EventArgs e)
        {
            lock (_lockObj)
            {
                _timer.Stop();
                _timer = null;

                Regrouping.Regroup(Queue);
                Queue.Clear();
            }
        }
    }
}
