﻿// ***********************************************************************
// Copyright (c) 2018 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using NUnit.Framework;
using NUnit.UiException.StackTraceAnalysers;
using NUnit.UiException.StackTraceAnalyzers;

namespace NUnit.UiException.Tests.StackTraceAnalyzers
{
    [TestFixture]
    public class TestFunctionParser :
        TestIErrorParser
    {
        private IErrorParser _parser;

        [SetUp]
        public new void SetUp()
        {
            _parser = new FunctionParser();

            return;
        }

        [Test]
        public void Test_Ability_To_Parse_Regular_Function_Values()
        {
            RawError res;

            // check parse of basic method
            res = AcceptValue(_parser, "à NUnit.UiException.TraceItem.get_Text() dans C:\\TraceItem.cs:ligne 43");
            Assert.That(res.Function, Is.EqualTo("NUnit.UiException.TraceItem.get_Text()"));

            // check parse a method with parameters
            res = AcceptValue(_parser, "à System.String.InternalSubStringWithChecks(Int32 startIndex, Int32 length, Boolean fAlwaysCopy)");
            Assert.That(res.Function, Is.EqualTo("System.String.InternalSubStringWithChecks(Int32 startIndex, Int32 length, Boolean fAlwaysCopy)"));

            // check it supports C/C++ function as well
            res = AcceptValue(_parser, "à main(int argc, const char **argv) dans C:\\file1:line1");
            Assert.That(res.Function, Is.EqualTo("main(int argc, const char **argv)"));

            // check it doesn't rely upon filePath or line information
            //res = AcceptValue(_parser, "get_Text()");
            //Assert.That(res.Function, Is.EqualTo("get_Text()"));

            // a simple function name is not accepted - that is, the leading "at" is required
            // TODO: try to restore older behavior while still allowing a space before the
            // opening parenthesis.
            RejectValue(_parser, "get_Text()");

            return;
        }

        [Test]
        public void Test_Ability_To_Parse_Mono_Stack_Trace()
        {
            RawError res;

            // mono adds a space after the name
            res = AcceptValue(_parser, "à NUnit.UiException.TraceItem.get_Text () dans C:\\TraceItem.cs:ligne 43");
            Assert.That(res.Function, Is.EqualTo("NUnit.UiException.TraceItem.get_Text ()"));
        }

        [Test]
        public void Test_Fail_To_Parse_Odd_Function_Values()
        {
            // check parse relies on '(' and ')'
            RejectValue(_parser, "à get_Text dans C:\\file1:line1");
            RejectValue(_parser, "à get_Text( dans C:\\file1:line1");
            RejectValue(_parser, "à get_Text) dans C:\\file1:line1");
            RejectValue(_parser, "à get_Text)( dans C:\\file1:line1");

            // check function name cannot be empty
            RejectValue(_parser, "à (int index) dans C:\\file1:line1");

            return;
        }
    }
}
