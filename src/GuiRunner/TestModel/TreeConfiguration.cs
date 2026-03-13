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

        string DisplayFormat { get; set; }

        // Properties used by all strategies
        bool ShowCheckBoxes { get; set; }

        // Properties used by NUnitTreeDisplayStrategy
        bool NUnitTreeShowTestDuration { get; set; }
        bool NUnitTreeShowAssemblies { get; set; }
        bool NUnitTreeShowNamespaces { get; set; }
        bool NUnitTreeShowFixtures { get; set; }

        // Properties used by TestListDisplayStrategy
        bool TestListShowAssemblies { get; set; }
        bool TestListShowFixtures { get; set; }
        string TestListGroupBy { get; set; }
    }

    /// <summary>
    /// This class represents the visual configuration of the tree
    /// </summary>
    public class TreeConfiguration : ITreeConfiguration
    {
        public event SettingsEventHandler Changed;

        private string _displayFormat = "NUNIT_TREE";
        public string DisplayFormat
        {
            get => _displayFormat;
            set
            {
                _displayFormat = value;
                OnPropertyChanged(nameof(DisplayFormat));
            }
        }

        // Properties used by all strategies

        private bool _showCheckBoxes = false;
        public bool ShowCheckBoxes
        {
            get => _showCheckBoxes;
            set
            {
                _showCheckBoxes = value;
                OnPropertyChanged(nameof(ShowCheckBoxes));
            }
        }

        private bool _nunitTreeShowTestDuration = false;
        public bool NUnitTreeShowTestDuration
        {
            get => _nunitTreeShowTestDuration;
            set
            {
                _nunitTreeShowTestDuration = value;
                OnPropertyChanged(nameof(NUnitTreeShowTestDuration));
            }
        }

        // Properties used by NUnitTreeDisplayStrategy

        private bool _nunitTreeShowAssemblies = true;
        public bool NUnitTreeShowAssemblies
        {
            get => _nunitTreeShowAssemblies;
            set
            {
                _nunitTreeShowAssemblies = value;
                OnPropertyChanged(nameof(NUnitTreeShowAssemblies));
            }
        }

        private bool _nunitTreeShowNamespaces = true;
        public bool NUnitTreeShowNamespaces
        {
            get => _nunitTreeShowNamespaces;
            set
            {
                _nunitTreeShowNamespaces = value;
                OnPropertyChanged(nameof(NUnitTreeShowNamespaces));
            }
        }

        private bool _nunitTreeShowFixtures = true;
        public bool NUnitTreeShowFixtures
        {
            get => _nunitTreeShowFixtures;
            set
            {
                _nunitTreeShowFixtures = value;
                OnPropertyChanged(nameof(NUnitTreeShowFixtures));
            }
        }

        // Properties used by TestListDisplayStrategy

        private bool _testListShowAssemblies = false;
        public bool TestListShowAssemblies
        {
            get => _testListShowAssemblies;
            set
            {
                _testListShowAssemblies = value;
                OnPropertyChanged(nameof(TestListShowAssemblies));
            }
        }

        private bool _testListShowFixtures = false;
        public bool TestListShowFixtures
        {
            get => _testListShowFixtures;
            set
            {
                _testListShowFixtures = value;
                OnPropertyChanged(nameof(TestListShowFixtures));
            }
        }

        private string _testListGroupBy = "UNGROUPED";
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
