// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public interface IEngineSettings
    {
        bool ShadowCopyFiles { get; set; }

        int Agents { get; set; }

        bool RerunOnChange { get; set; }

        bool SetPrincipalPolicy { get; set; }

        string PrincipalPolicy { get; set; }
    }
}
