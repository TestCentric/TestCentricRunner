// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Collections;
    using System.IO;
    using Dialogs;
    using Model;
    using TestCentric.Gui.Controls;
    using TestCentric.Gui.Model.Settings;
    using Views;

    /// <summary>
    /// TreeViewPresenter is the presenter for the TestTreeView
    /// </summary>
    public class TreeViewPresenter
    {
        private ITestTreeView _view;
        private ITestModel _model;
        private ITreeDisplayStrategyFactory _treeDisplayStrategyFactory;

        // Accessed by tests
        public ITreeDisplayStrategy Strategy { get; private set; }

        public ITreeConfiguration TreeConfiguration { get; }

        #region Constructor

        public TreeViewPresenter(ITestTreeView treeView, ITestModel model, ITreeDisplayStrategyFactory factory)
        {
            _view = treeView;
            _model = model;
            _treeDisplayStrategyFactory = factory;

            TreeConfiguration = _model.TreeConfiguration;

            UpdateTreeViewSortMode();
            WireUpEvents();
        }

        #endregion

        #region Private Members

        private void WireUpEvents()
        {
            // Model actions
            _model.Events.TestLoaded += (ea) =>
            {
                EnsureNonRunnableFilesAreVisible(ea.Test);

                bool visualStateLoaded = _model.TryLoadVisualState(out VisualState visualState);
                if (visualStateLoaded)
                    this.UpdateTreeConfiguration(visualState);
                Strategy = _treeDisplayStrategyFactory.Create(TreeConfiguration.DisplayFormat, _view, _model);

                _view.CategoryFilter.Init(_model);
                Strategy.OnTestLoaded(ea.Test, visualState);
                CheckPropertiesDisplay();
                CheckXmlDisplay();
            };

            _model.Events.TestReloaded += (ea) =>
            {
                _view.InvokeIfRequired(() =>
                {
                    EnsureNonRunnableFilesAreVisible(ea.Test);

                    // Handle category filter identically to close/load project
                    ResetTestFilterUIElements();
                    _view.CategoryFilter.Close();
                    _view.CategoryFilter.Init(_model);

                    _model.TryLoadVisualState(out VisualState visualState);
                    Strategy.OnTestLoaded(ea.Test, visualState);
                    _view.CheckBoxes = _view.ShowCheckBoxes.Checked; // TODO: View should handle this
                });
            };

            _model.Events.TestUnloaded += (ea) =>
            {
                Strategy.OnTestUnloaded();
                _view.CategoryFilter.Close();
                ResetTestFilterUIElements();
            };

            _model.Events.TestsUnloading += ea =>
            {
                ClosePropertiesDisplay();
                CloseXmlDisplay();
            };

            _model.Events.RunStarting += (ea) =>
            {
                Strategy.OnTestRunStarting();
                CheckPropertiesDisplay();
                CheckXmlDisplay();
            };

            _model.Events.RunFinished += (ea) =>
            {
                Strategy.OnTestRunFinished();
            };

            _model.Events.TestFilterChanged += (ea) =>
            {
                Strategy?.Reload(true);
            };

            _model.Events.TestFinished += OnTestFinished;
            _model.Events.SuiteFinished += OnTestFinished;

            _model.Events.VisualStateRequest += (ea) =>
            {
                ea.VisualState = Strategy.CreateVisualState();
            };

            _model.Settings.Changed += OnSettingsChanged;
            TreeConfiguration.Changed += OnTreeConfigurationChanged;

            // View context commands

            // Test for null is a hack that allows us to avoid
            // a problem under Linux creating a ContextMenuStrip
            // when no display is present.
            _view.ContextMenuOpening += (s, e) => InitializeContextMenu();

            _view.CollapseAllCommand.Execute += () => _view.CollapseAll();
            _view.ExpandAllCommand.Execute += () => _view.ExpandAll();
            _view.CollapseToFixturesCommand.Execute += () => Strategy.CollapseToFixtures();
            _view.RemoveTestPackageCommand.Execute += () => RemoveTestPackage();
            _view.TreeViewDeleteKeyCommand.KeyUp += () => RemoveTestPackage();

            _view.ShowCheckBoxes.CheckedChanged += () =>
            {
                TreeConfiguration.ShowCheckBoxes = _view.ShowCheckBoxes.Checked;
                _view.CheckBoxes = TreeConfiguration.ShowCheckBoxes;
            };

            _view.ShowTestDuration.CheckedChanged += () =>
            {
                TreeConfiguration.ShowTestDuration = _view.ShowTestDuration.Checked;
                Strategy?.UpdateTreeNodeNames();
            };

            _view.SortCommand.SelectionChanged += () => UpdateTreeViewSortMode();

            _view.SortDirectionCommand.SelectionChanged += () => UpdateTreeViewSortMode();

            _view.RunContextCommand.Execute += () =>
            {
                if (_view.ContextNode != null)
                {
                    if (_view.ContextNode.Tag is TestNode testNode)
                        _model.RunTests(testNode);
                    else if (_view.ContextNode.Tag is TestGroup groupNode)
                        _model.RunTests(groupNode);
                }
            };

            _view.TreeNodeDoubleClick += (treeNode) =>
            {
                var testNode = treeNode.Tag as TestNode;
                if (testNode != null && testNode.Type == "TestCase" && !_model.IsTestRunning)
                    _model.RunTests(testNode);
            };

            _view.DebugContextCommand.Execute += () =>
            {
                if (_view.ContextNode != null)
                {
                    if (_view.ContextNode.Tag is TestNode testNode)
                        _model.DebugTests(testNode);
                    else if (_view.ContextNode.Tag is TestGroup groupNode)
                        _model.DebugTests(groupNode);
                }
            };

            _view.ClearResultsContextCommand.Execute += () =>
            {
                _model.ClearResults();
                Strategy.Reload();
            };

            _view.TestPropertiesCommand.Execute += () => ShowPropertiesDisplay();

            _view.ViewAsXmlCommand.Execute += () => ShowXmlDisplayDialog();

            _view.SelectedNodeChanged += (treeNode) =>
            {
                var testItem = treeNode.Tag as ITestItem;
                if (testItem != null)
                {
                    _model.ActiveTestItem = testItem;

                    // Selected item is either a TestSelection or a TestNode. When
                    // CheckBoxes are off, the active item is used as the selection.
                    var selection = testItem as TestSelection;
                    var node = testItem as TestNode;

                    if (!_view.CheckBoxes)
                    {
                        // If it's a TestNode, make a TestSelection
                        if (selection == null && node != null)
                            selection = new TestSelection() { node };
                        _model.SelectedTests = selection;
                    }

                    if (_propertiesDisplay != null)
                    {
                        if (_propertiesDisplay.Pinned)
                            _propertiesDisplay.Display(treeNode);
                        else
                            ClosePropertiesDisplay();
                    }

                    if (_xmlDisplay != null)
                    {
                        if (_xmlDisplay.Pinned)
                            _xmlDisplay.Display(treeNode);
                        else
                            CloseXmlDisplay();
                    }
                }
            };

            _view.AfterCheck += (treeNode) =>
            {
                var checkedNodes = _view.CheckedNodes;
                var selection = new TestSelection();

                foreach (var node in checkedNodes)
                    selection.Add(node.Tag as ITestItem);
                
                selection.AddExplicitChildTests();
                _model.SelectedTests = selection;
            };

            _view.OutcomeFilter.SelectionChanged += () =>
            {
                var filter = _view.OutcomeFilter.SelectedItems;
                _model.TestCentricTestFilter.OutcomeFilter = filter;
            };

            _view.TextFilter.Changed += () =>
            {
                var text = _view.TextFilter.Text;
                _model.TestCentricTestFilter.TextFilter = text;
            };

            _view.CategoryFilter.SelectionChanged += () =>
            {
                _model.TestCentricTestFilter.CategoryFilter = _view.CategoryFilter.SelectedItems;
            };

            _view.ResetFilterCommand.Execute += () => ResetTestFilter();

            // TODO: Verify that this is no longer needed and remove code
            // Node selected in tree
            //_treeView.SelectedNodesChanged += (nodes) =>
            //{
            //    var selection = new TestSelection();
            //    foreach (TreeNode tn in nodes)
            //    {
            //        var test = tn.Tag as TestNode;
            //        if (test != null)
            //            selection.Add(test);
            //        _model.NotifyTestSelectionChanged(selection);
            //    }

            //    if (_propertiesDisplay != null)
            //    {
            //        if (_propertiesDisplay.Pinned && nodes.Count == 1)
            //            _propertiesDisplay.Display(nodes[0]);
            //        else
            //            ClosePropertiesDisplay();
            //    }

            //    if (_xmlDisplay != null)
            //    {
            //        if (_xmlDisplay.Pinned && nodes.Count == 1)
            //            _xmlDisplay.Display(nodes[0]);
            //        else
            //            CloseXmlDisplay();
            //    }
            //};
        }

        private void OnSettingsChanged(object sender, SettingsEventArgs e)
        {
            switch (e.SettingName)
            {

                case "TestCentric.Gui.GuiLayout":
                    if (_model.Settings.Gui.GuiLayout == "Full")
                        ClosePropertiesDisplay();
                    break;

                case "TestCentric.Gui.TestTree.ShowFilter":
                    _view.SetTestFilterVisibility(_model.Settings.Gui.TestTree.ShowFilter);
                    break;
            }
        }

        private void OnTreeConfigurationChanged(object sender, SettingsEventArgs e)
        {
            switch (e.SettingName)
            {
                case nameof(TreeConfiguration.DisplayFormat):
                    Strategy = _treeDisplayStrategyFactory.Create(TreeConfiguration.DisplayFormat, _view, _model);
                    Strategy.Reload();
                    break;
                case nameof(TreeConfiguration.NUnitGroupBy):
                case nameof(TreeConfiguration.TestListGroupBy):
                case nameof(TreeConfiguration.ShowNamespaces):
                    Strategy?.Reload();
                    break;
                case nameof(TreeConfiguration.ShowCheckBoxes):
                    _view.ShowCheckBoxes.Checked = TreeConfiguration.ShowCheckBoxes;
                    break;
            }
        }

        private void ResetTestFilter()
        {
            _model.TestCentricTestFilter.ResetAll();
            ResetTestFilterUIElements();
        }

        private void ResetTestFilterUIElements()
        {
            _view.TextFilter.Text = "";
            _view.OutcomeFilter.SelectedItems = _model.TestCentricTestFilter.OutcomeFilter;
            _view.CategoryFilter.SelectedItems = _model.TestCentricTestFilter.CategoryFilter;
        }
        private void UpdateTreeViewSortMode()
        {
            var sortMode = _view.SortCommand.SelectedItem;

            // Activate 'ShowTestDuration' in case sort by duration is selected
            if (sortMode == TreeViewNodeComparer.Duration)
                _view.ShowTestDuration.Checked = true;
          
            IComparer comparer = TreeViewNodeComparer.GetComparer(_model, sortMode, _view.SortDirectionCommand.SelectedItem);
            _view.Sort(comparer);
        }

        private void UpdateTreeConfiguration(VisualState visualState)
        {
            // 1. Unsubscribe from setting changed events
            // (Avoid triggering reload while loading the tree)
            _model.Settings.Changed -= OnSettingsChanged;
            TreeConfiguration.Changed -= OnTreeConfigurationChanged;

            // 2. Update tree configuration
            if (visualState != null)
            {
                // Update from VisualState
                TreeConfiguration.ShowCheckBoxes = visualState.ShowCheckBoxes;
                TreeConfiguration.DisplayFormat = visualState.DisplayStrategy;
                TreeConfiguration.ShowNamespaces = visualState.ShowNamespaces;
                TreeConfiguration.NUnitGroupBy = TreeConfiguration.DisplayFormat == "NUNIT_TREE" ? visualState.GroupBy : "UNGROUPED";
                TreeConfiguration.TestListGroupBy = TreeConfiguration.DisplayFormat == "TEST_LIST" ? visualState.GroupBy : "UNGROUPED";
            }
            else
            {
                // Reset to default values for new projects
                ITestTreeSettings treeSettings = _model.Settings.Gui.TestTree;
                TreeConfiguration.ShowCheckBoxes = treeSettings.ShowCheckBoxes;
                TreeConfiguration.DisplayFormat = treeSettings.DisplayFormat;
                TreeConfiguration.NUnitGroupBy = "UNGROUPED";
                TreeConfiguration.TestListGroupBy = "UNGROUPED";
                TreeConfiguration.ShowNamespaces = true;
                TreeConfiguration.ShowTestDuration = false;
            }

            // Update UI elements according to latest values
            _view.ShowCheckBoxes.Checked = TreeConfiguration.ShowCheckBoxes;
            _view.ShowTestDuration.Checked = TreeConfiguration.ShowTestDuration;

            // 3. Subscribe again to setting changed events
            _model.Settings.Changed += OnSettingsChanged;
            TreeConfiguration.Changed += OnTreeConfigurationChanged;
        }

        private void EnsureNonRunnableFilesAreVisible(TestNode testNode)
        {
            // HACK: Temporary fix switches the display strategy if no
            // tests are found. Should handle other error situations
            // including one non-runnable file out of several files.
            if (testNode.TestCount == 0)
                Strategy = _treeDisplayStrategyFactory.Create("NUNIT_TREE", _view, _model);
        }

        private void OnTestFinished(TestResultEventArgs args)
        {
            Strategy.OnTestFinished(args.Result);

            _propertiesDisplay?.OnTestFinished(args.Result);
            _xmlDisplay?.OnTestFinished(args.Result);
        }

        TestPropertiesDialog _propertiesDisplay;

        private void ShowPropertiesDisplay()
        {
            if (_propertiesDisplay == null)
            {
                var mainForm = ((Control)_view).FindForm();

                _propertiesDisplay = new TestPropertiesDialog(_model, _view)
                {
                    Owner = mainForm,
                    Font = mainForm.Font,
                    StartPosition = FormStartPosition.Manual
                };

                var midScreen = Screen.FromHandle(mainForm.Handle).WorkingArea.Width / 2;
                var midForm = (mainForm.Left + mainForm.Right) / 2;

                _propertiesDisplay.Left = midForm < midScreen
                    ? mainForm.Right
                    : Math.Max(0, mainForm.Left - _propertiesDisplay.Width);

                _propertiesDisplay.Top = mainForm.Top;

                _propertiesDisplay.Closed += (s, e) => _propertiesDisplay = null;
            }

            _propertiesDisplay.Display(_view.ContextNode);
        }

        private void ClosePropertiesDisplay()
        {
            if (_propertiesDisplay != null)
            {
                _propertiesDisplay.InvokeIfRequired(() => _propertiesDisplay.Close());
                _propertiesDisplay = null;
            }
        }

        private void CheckPropertiesDisplay()
        {
            if (_propertiesDisplay != null && !_propertiesDisplay.Pinned)
                ClosePropertiesDisplay();
        }

        private XmlDisplay _xmlDisplay;

        private void ShowXmlDisplayDialog()
        {
            if (_xmlDisplay == null)
            {
                var treeView = (Control)_view;
                var mainForm = treeView.FindForm();

                _xmlDisplay = new XmlDisplay(_model)
                {
                    Owner = mainForm,
                    Font = mainForm.Font,
                    StartPosition = FormStartPosition.Manual
                };

                var midForm = (mainForm.Left + mainForm.Right) / 2;
                var screenArea = Screen.FromHandle(mainForm.Handle).WorkingArea;
                var midScreen = screenArea.Width / 2;

                var myLeft = mainForm.Left;
                var myRight = mainForm.Right;

                if (_propertiesDisplay != null)
                {
                    myLeft = Math.Min(myLeft, _propertiesDisplay.Left);
                    myRight = Math.Max(myRight, _propertiesDisplay.Right);
                }

                _xmlDisplay.Left = myLeft > screenArea.Width - myRight
                    ? Math.Max(0, myLeft - _xmlDisplay.Width)
                    : Math.Min(myRight, screenArea.Width - _xmlDisplay.Width);

                _xmlDisplay.Top = mainForm.Top + (mainForm.Height - _xmlDisplay.Height) / 2;

                _xmlDisplay.Closed += (s, e) => _xmlDisplay = null;
            }

            _xmlDisplay.Display(_view.ContextNode);
        }

        private void CloseXmlDisplay()
        {
            _xmlDisplay?.InvokeIfRequired(() => _xmlDisplay?.Close());
        }

        private void CheckXmlDisplay()
        {
            if (_xmlDisplay != null && !_xmlDisplay.Pinned)
                CloseXmlDisplay();
        }

        private void InitializeContextMenu()
        {
            // TODO: Config Menu is hidden until changing the config actually works
            bool displayConfigMenu = false;
            //var test = _treeView.ContextNode?.Tag as TestNode;
            //if (test != null && test.IsProject)
            //{
            //    NUnit.Engine.TestPackage package = _model.GetPackageForTest(test.Id);
            //    string activeConfig = package.GetActiveConfig();
            //    string[] configNames = package.GetConfigNames();

            //    if (configNames.Length > 0)
            //    {
            //        _treeView.ActiveConfiguration.MenuItems.Clear();
            //        foreach (string config in configNames)
            //        {
            //            var configEntry = new ToolStripMenuItem(config);
            //            configEntry.Checked = config == activeConfig;
            //            configEntry.Click += (sender, e) => _model.ReloadPackage(package, ((ToolStripMenuItem)sender).Text);
            //            _treeView.ActiveConfiguration.MenuItems.Add(configEntry);
            //        }

            //        //displayConfigMenu = true;
            //    }
            //}

            _view.ActiveConfiguration.Visible = displayConfigMenu;

            var layout = _model.Settings.Gui.GuiLayout;
            _view.TestPropertiesCommand.Visible = layout == "Mini";

            var selectedNode = _view.ContextNode?.Tag as TestNode;
            _view.RemoveTestPackageCommand.Visible = CanRemovePackageNode(selectedNode);

            // If a test is already running, no new test run should be started.
            _view.RunContextCommand.Enabled = _model.HasTests && !_model.IsTestRunning;
            _view.DebugContextCommand.Enabled = _model.HasTests && !_model.IsTestRunning;            _view.DebugContextCommand.Enabled = _model.HasTests && !_model.IsTestRunning;
            _view.ClearResultsContextCommand.Enabled = _model.HasResults && !_model.IsTestRunning;
        }

        private void RemoveTestPackage()
        {
            var testNode = _view.SelectedNode?.Tag as TestNode;
            if (CanRemovePackageNode(testNode))
            {
                var subPackage = _model.GetPackageForTest(testNode.Id);
                _model.RemoveTestPackage(subPackage);
            }
        }

        private bool CanRemovePackageNode(TestNode testNode)
        {
            return _model.HasTests && !_model.IsTestRunning && testNode != null && testNode.IsAssembly && _model.TopLevelPackage.SubPackages.Count > 1;
        }

        #endregion
    }
}
