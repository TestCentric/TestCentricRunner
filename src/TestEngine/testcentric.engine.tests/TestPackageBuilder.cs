// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Engine;

namespace TestCentric.Engine
{
    /// <summary>
    /// TestPackageBuilder creates test packages while allowing for
    /// differences between the NUnit 3 and NUnit 4 APIs. It does
    /// this by avoiding use of the single file constructor, which
    /// has different semantics in version 3 and 4. Currently we
    /// are using the NUnit 3 API so this class is actually 
    /// used in only a few tests. If we start building for both
    /// APIs, then we will need to use it systematically.
    /// </summary>
    public static class TestPackageBuilder
    {
        public static TestPackage MakeTopLevelPackage(string testFile)
        {
            return new TestPackage(new string[] { testFile });
        }

        public static TestPackage MakeTopLevelPackage(params string[] testFiles)
        {
            return new TestPackage(testFiles);
        }

        public static TestPackage MakeTopLevelPackage(IList<string> testFiles)
        {
            return new TestPackage(testFiles);
        }

        public static TestPackage MakeSubPackage(string testFile)
        {
            return new TestPackage(testFile).SubPackages[0];
        }
    }
}
