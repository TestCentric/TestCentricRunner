// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface IEngineSettings
    {
        bool RerunOnChange { get; set; }
    }


    /// <summary>
    /// We store settings used by the engine using the same
    /// settings path that the console runner uses. We may
    /// want to change this in the future.
    /// </summary>
    public class EngineSettings : ApplicationSettingsBase, IEngineSettings
    {
        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool RerunOnChange
        {
            get { return (bool)this[nameof(RerunOnChange)]; }
            set { this[nameof(RerunOnChange)] = value; }
        }
    }
}
