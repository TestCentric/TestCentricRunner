// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.Filter
{
    using System.Collections.Generic;
    using TestCentric.Gui.Model;

    internal static class TestFilterData
    {
        internal static TestNode GetSampleTestProject()
        {
            return new TestNode(CreateTestRunXml(
                                    CreateTestSuiteXml("3-1000", "LibraryA",
                                        CreateTestSuiteXml("3-1001", "NamespaceA",
                                            CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>(),
                                                CreateTestcaseXml("3-1011", "TestA", ""),
                                                CreateTestcaseXml("3-1012", "TestB", ""))),
                                        CreateTestSuiteXml("3-2001", "NamespaceB", 
                                            CreateTestFixtureXml("3-2010", "Fixture_2", new List<string>(),
                                                CreateTestcaseXml("3-2011", "TestA", ""),
                                                CreateTestcaseXml("3-2012", "TestB", "")),
                                            CreateExplicitTestFixtureXml("3-3010", "ExplicitFixture", new List<string>(),
                                                CreateTestcaseXml("3-3011", "TestA", ""),
                                                CreateTestcaseXml("3-3012", "TestB", "")),
                                            CreateTestFixtureXml("3-4010", "Fixture_3", new List<string>(),
                                                CreateTestcaseXml("3-4011", "TestA", ""),
                                                CreateExplicitTestcaseXml("3-4012", "TestB", ""))))));
        }

        internal static TestNode GetTestById(TestNode node, string id)
        {
            if (node.Id == id)
                return node;

            foreach (TestNode child in node.Children)
            {
                var foundNode = GetTestById(child, id);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }

        private static string CreateTestcaseXml(string testId, string testName)
        {
            return CreateTestcaseXml(testId, testName, new List<string>());
        }

        private static string CreateTestcaseXml(string testId, string testName, string category)
        {
            return CreateTestcaseXml(testId, testName, new List<string>() { category });
        }

        private static string CreateExplicitTestcaseXml(string testId, string testName, string category)
        {
            return CreateTestcaseXml(testId, testName, new List<string>() { category }, true);
        }

        private static string CreateTestcaseXml(string testId, string testName, IList<string> categories, bool explicitTest = false)
        {
            string str = $"<test-case id='{testId}' name='{testName}' ";
            if (explicitTest)
                str += "runstate='Explicit' ";
            str += "> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            str += "</test-case> ";

            return str;
        }

        private static string CreateTestFixtureXml(string testId, string testName, IEnumerable<string> categories, params string[] testCases)
        {
            string str = $"<test-suite type='TestFixture' id='{testId}'  name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            foreach (string testCase in testCases)
                str += testCase;

            str += "</test-suite>";

            return str;
        }

        private static string CreateExplicitTestFixtureXml(string testId, string testName, IEnumerable<string> categories, params string[] testCases)
        {
            string str = $"<test-suite type='TestFixture' id='{testId}'  name='{testName}' runstate='Explicit'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            foreach (string testCase in testCases)
                str += testCase;

            str += "</test-suite>";

            return str;
        }

        private static string CreateTestSuiteXml(string testId, string testName, params string[] testSuites)
        {
            string str = $"<test-suite type='TestSuite' id='{testId}' name='{testName}'> ";
            foreach (string testSuite in testSuites)
                str += testSuite;

            str += "</test-suite>";

            return str;
        }

        private static string CreateTestRunXml(params string[] testSuites)
        {
            string str = $"<test-run type='TestRun'> ";
            foreach (string testSuite in testSuites)
                str += testSuite;

            str += "</test-run>";

            return str;
        }
    }
}
