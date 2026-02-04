// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Model.Settings;

namespace TestCentric.Gui.Model
{
    public interface ITreeConfiguration
    {
        public event SettingsEventHandler Changed;

        bool ShowCheckBoxes { get; set; }

        bool ShowNamespaces {  get; set; }

        bool ShowTestDuration { get; set; }

        string DisplayFormat { get; set; }

        string NUnitGroupBy { get; set; }

        string TestListGroupBy { get; set; }
    }

    /// <summary>
    /// This class represents the visual configuration of the tree
    /// </summary>
    public class TreeConfiguration : ITreeConfiguration
    {
        public event SettingsEventHandler Changed;
        private bool _showCheckBoxes = false;
        private bool _showNamespaces = true;
        private bool _showTestDuration = false;
        private string _displayFormat = "NUNIT_TREE";
        private string _nunitGroupBy = "UNGROUPED";
        private string _testListGroupBy = "UNGROUPED";

        public bool ShowCheckBoxes
        {
            get => _showCheckBoxes;
            set
            {
                _showCheckBoxes = value;
                OnPropertyChanged(nameof(ShowCheckBoxes));
            }
        }

        public bool ShowNamespaces
        {
            get => _showNamespaces;
            set
            {
                _showNamespaces = value;
                OnPropertyChanged(nameof(ShowNamespaces));
            }
        }

        public bool ShowTestDuration
        {
            get => _showTestDuration;
            set
            {
                _showTestDuration = value;
                OnPropertyChanged(nameof(ShowTestDuration));
            }
        }

        public string DisplayFormat
        {
            get => _displayFormat;
            set
            {
                _displayFormat = value;
                OnPropertyChanged(nameof(DisplayFormat));
            }
        }

        public string NUnitGroupBy
        {
            get => _nunitGroupBy;
            set
            {
                _nunitGroupBy = value;
                OnPropertyChanged(nameof(NUnitGroupBy));
            }
        }

        public string TestListGroupBy
        {
            get => _testListGroupBy;
            set
            {
                _testListGroupBy = value;
                OnPropertyChanged(nameof(TestListGroupBy));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            Changed?.Invoke(this, new SettingsEventArgs(propertyName));
        }
    }
}
