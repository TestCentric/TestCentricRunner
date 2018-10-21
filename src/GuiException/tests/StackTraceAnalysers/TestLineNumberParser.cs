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
    public class TestLineNumberParser :
        TestIErrorParser
    {
        private IErrorParser _parser;

        [SetUp]
        public new void SetUp()
        {
            _parser = new LineNumberParser();

            return;
        }

        [Test]
        public void Test_Ability_To_Parse_Regular_Line_Number_Values()
        {
            RawError res;

            // a basic test
            res = AcceptValue(_parser, "à get_Text() dans C:\\folder\\file1:line 1");
            Assert.That(res.Line, Is.EqualTo(1));

            // parser doesn't rely upon the presence of words between
            // the colon and the number
            res = AcceptValue(_parser, "à get_Text() dans C:\\folder\\file1:42");
            Assert.That(res.Line, Is.EqualTo(42));

            // parser doesn't rely on the existence of
            // a method name or filePath value
            res = AcceptValue(_parser, ":43");
            Assert.That(res.Line, Is.EqualTo(43));

            // Works for German
            // NOTE: German provides a period at the end of the line
            res = AcceptValue(_parser, @"bei CT.Business.BusinessObjectXmlSerializer.Deserialize(String serializedObject) in D:\Source\CT5\BASE\CT.Business\BusinessObjectXmlSerializer.cs:Zeile 86.");
            Assert.That(res.Line, Is.EqualTo(86));

            // Russian works too
            // в Samples.ExceptionBrowserTest.Worker.DoSomething() в C:\psgdev\Projects\NUnit\Tests\Samples\ExceptionBrowserTest.cs:строка 16
            // в Samples.ExceptionBrowserTest.Test() в C:\psgdev\Projects\NUnit\Tests\Samples\ExceptionBrowserTest.cs:строка 24
            res = AcceptValue(_parser, @"в Samples.ExceptionBrowserTest.Worker.DoSomething() в C:\psgdev\Projects\NUnit\Tests\Samples\ExceptionBrowserTest.cs:строка 16");
            Assert.That(res.Line, Is.EqualTo(16));
            return;
        }

        [Test]
        public void Test_Ability_To_Reject_Odd_Line_Number_Values()
        {
            // after the terminal ':' parser expects to have only one integer value            
            RejectValue(_parser, "à get_Text() dans C:\\folder\\file1 line 42");
            RejectValue(_parser, "à get_Text() dans C:\\folder\\file42");

            // check it fails to parse int values that are part of a word
            RejectValue(_parser, "à get_Text() dans C:\\folder\\file1:line43");

            // a line number should not be zero
            RejectValue(_parser, "à get_Text() dans C:\\folder\\file1:line 0");

            // a line number should not be negative
            RejectValue(_parser, "à get_Text() dans C:\\folder\\file1:line -42");

            return;
        }
    }
}
