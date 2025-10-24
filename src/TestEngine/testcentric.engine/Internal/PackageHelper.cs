// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Common;
using NUnit.Engine;

using NUnitPackage = NUnit.Engine.TestPackage;

namespace TestCentric.Engine.Internal
{
    public static class PackageHelper
    {
        public static bool IsAssemblyPackage(this NUnitPackage package)
        {
            return package.FullName is not null && PathUtils.IsAssemblyFileType(package.FullName);
        }

        public static bool HasSubPackages(this NUnitPackage package)
        {
            return package.SubPackages.Count > 0;
        }

        public static IList<NUnitPackage> Select(this NUnitPackage package, TestPackageSelectorDelegate selector)
        {
            var selection = new List<NUnitPackage>();

            AccumulatePackages(package, selection, selector);

            return selection;
        }

        private static void AccumulatePackages(NUnitPackage package, IList<NUnitPackage> selection, TestPackageSelectorDelegate selector)
        {
            if (selector(package))
                selection.Add(package);

            foreach (var subPackage in package.SubPackages)
                AccumulatePackages(subPackage, selection, selector);
        }

        public static NUnitPackage MakeNUnitPackage(this TestPackage package)
        {
            var nunitPackage = new NUnitPackage();

            foreach (var subPackage in package.SubPackages)
                nunitPackage.AddSubPackage(MakeNUnitPackage(subPackage));

            foreach (var setting in package.Settings)
            {
                var name = setting.Name;
                var value = setting.Value;

                if (value is int)
                    nunitPackage.AddSetting(setting.Name, (int)value);
                else if (value is bool)
                    nunitPackage.AddSetting(setting.Name, (bool)value);
                else if (value is string)
                    nunitPackage.AddSetting(setting.Name, (string)value);
            }

            // HACK: We still use TargetRuntimeFramework, of Type RuntimeFramework.
            // NUnit uses TargetFrameworkName. This is needed until our API is updated.
            var targetRuntime = package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework);
            nunitPackage.AddSetting("TargetFrameworkName", RuntimeFramework.Parse(targetRuntime).FrameworkName.ToString());

            return nunitPackage;
        }
    }
}
