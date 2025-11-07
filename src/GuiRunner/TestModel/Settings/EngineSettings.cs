// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface IEngineSettings
    {
        bool ShadowCopyFiles { get; set; }

        int Agents { get; set; }

        bool RerunOnChange { get; set; }

        bool SetPrincipalPolicy { get; set; }

        string PrincipalPolicy { get; set; }
    }


    /// <summary>
    /// We store settings used by the engine using the same
    /// settings path that the console runner uses. We may
    /// want to change this in the future.
    /// </summary>
    public class EngineSettings : ApplicationSettingsBase, IEngineSettings
    {
        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool ShadowCopyFiles
        {
            get { return (bool)this[nameof(ShadowCopyFiles)]; }
            set { this[nameof(ShadowCopyFiles)] = value;  }
        }

        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int Agents
        {
            get { return (int)this[nameof(Agents)]; }
            set { this[nameof(Agents)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool RerunOnChange
        {
            get { return (bool)this[nameof(RerunOnChange)]; }
            set { this[nameof(RerunOnChange)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool SetPrincipalPolicy
        {
            get { return (bool)this[nameof(SetPrincipalPolicy)]; }
            set { this[nameof(SetPrincipalPolicy)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue(nameof(System.Security.Principal.PrincipalPolicy.UnauthenticatedPrincipal))]
        public string PrincipalPolicy
        {
            get { return (string)this[nameof(PrincipalPolicy)]; }
            set { this[nameof(PrincipalPolicy)] = value; }
        }
    }
}
