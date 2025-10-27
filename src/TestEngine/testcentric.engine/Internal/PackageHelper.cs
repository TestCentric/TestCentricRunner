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

            return nunitPackage;
        }
    }
}
