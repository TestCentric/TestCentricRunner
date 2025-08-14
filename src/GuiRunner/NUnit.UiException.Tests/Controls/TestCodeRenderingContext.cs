// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;
using NUnit.Framework;
using NUnit.UiException.CodeFormatters;
using NUnit.UiException.Controls;

namespace NUnit.UiException.Tests.Controls
{
    [TestFixture]
    public class TestCodeRenderingContext
    {
        private CodeRenderingContext _context;

        [SetUp]
        public void SetUp()
        {
            TestingCodeBox box = new TestingCodeBox();
            _context = box.RenderingContext;
        }

        [Test]
        public void DefaultState()
        {
            Assert.That(_context, Is.Not.Null);
            Assert.That(_context.Graphics, Is.Not.Null);
            Assert.That(_context.Font, Is.Not.Null);
            Assert.That(_context.Font.Size, Is.EqualTo(8));

            Assert.That(_context.CurrentLine, Is.EqualTo(-1));

            Assert.That(_context.BackgroundColor, Is.EqualTo(Color.White));
            Assert.That(_context.CurrentLineBackColor, Is.EqualTo(Color.Red));
            Assert.That(_context.CurrentLineForeColor, Is.EqualTo(Color.White));
            Assert.That(_context.KeywordColor, Is.EqualTo(Color.Blue));
            Assert.That(_context.CommentColor, Is.EqualTo(Color.Green));
            Assert.That(_context.CodeColor, Is.EqualTo(Color.Black));
            Assert.That(_context.StringColor, Is.EqualTo(Color.Red));

            Assert.That(_context[ClassificationTag.Code], Is.EqualTo(_context.CodeColor));
            Assert.That(_context[ClassificationTag.Comment], Is.EqualTo(_context.CommentColor));
            Assert.That(_context[ClassificationTag.Keyword], Is.EqualTo(_context.KeywordColor));
            Assert.That(_context[ClassificationTag.String], Is.EqualTo(_context.StringColor));

            CheckBrushes(_context);
            CheckPens(_context);

            return;
        }

        void CheckBrushes(CodeRenderingContext context)
        {
            Assert.That(_context.BackgroundBrush, Is.Not.Null);
            Assert.That(_context.CurrentLineBackBrush, Is.Not.Null);
            Assert.That(_context.CurrentLineForeBrush, Is.Not.Null);
            Assert.That(_context.KeywordBrush, Is.Not.Null);
            Assert.That(_context.CommentBrush, Is.Not.Null);
            Assert.That(_context.CodeBrush, Is.Not.Null);
            Assert.That(_context.StringBrush, Is.Not.Null);

            Assert.That(_context.GetBrush(ClassificationTag.Code),
                Is.SameAs(_context.CodeBrush));
            Assert.That(_context.GetBrush(ClassificationTag.Comment),
                Is.SameAs(_context.CommentBrush));
            Assert.That(_context.GetBrush(ClassificationTag.Keyword),
                Is.SameAs(_context.KeywordBrush));
            Assert.That(_context.GetBrush(ClassificationTag.String),
                Is.SameAs(_context.StringBrush));

            return;
        }

        void CheckPens(CodeRenderingContext context)
        {
            Assert.That(_context.BackgroundPen, Is.Not.Null);
            Assert.That(_context.CurrentLineBackPen, Is.Not.Null);
            Assert.That(_context.CurrentLineForePen, Is.Not.Null);
            Assert.That(_context.KeywordPen, Is.Not.Null);
            Assert.That(_context.CommentPen, Is.Not.Null);
            Assert.That(_context.CodePen, Is.Not.Null);
            Assert.That(_context.StringPen, Is.Not.Null);

            Assert.That(_context.GetPen(ClassificationTag.Code),
               Is.SameAs(_context.CodePen));
            Assert.That(_context.GetPen(ClassificationTag.Comment),
                Is.SameAs(_context.CommentPen));
            Assert.That(_context.GetPen(ClassificationTag.Keyword),
                Is.SameAs(_context.KeywordPen));
            Assert.That(_context.GetPen(ClassificationTag.String),
                Is.SameAs(_context.StringPen));

            return;
        }

        [Test]
        public void Can_Change_Colors()
        {
            Brush formerBrush;
            Pen formerPen;

            formerBrush = _context.BackgroundBrush;
            formerPen = _context.BackgroundPen;
            _context.BackgroundColor = Color.Pink;
            Assert.That(_context.BackgroundColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.BackgroundPen, Is.Not.SameAs(formerPen));
            Assert.That(_context.BackgroundBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.CodeBrush;
            formerPen = _context.CodePen;
            _context.CodeColor = Color.Pink;
            Assert.That(_context.CodeColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.CodePen, Is.Not.SameAs(formerPen));
            Assert.That(_context.CodeBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.CommentBrush;
            formerPen = _context.CommentPen;
            _context.CommentColor = Color.Pink;
            Assert.That(_context.CommentColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.CommentPen, Is.Not.SameAs(formerPen));
            Assert.That(_context.CommentBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.CurrentLineBackBrush;
            formerPen = _context.CurrentLineBackPen;
            _context.CurrentLineBackColor = Color.Pink;
            Assert.That(_context.CurrentLineBackColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.CurrentLineBackPen, Is.Not.SameAs(formerPen));
            Assert.That(_context.CurrentLineBackBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.CurrentLineForeBrush;
            formerPen = _context.CurrentLineForePen;
            _context.CurrentLineForeColor = Color.Pink;
            Assert.That(_context.CurrentLineForeColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.CurrentLineForePen, Is.Not.SameAs(formerPen));
            Assert.That(_context.CurrentLineForeBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.KeywordBrush;
            formerPen = _context.KeywordPen;
            _context.KeywordColor = Color.Pink;
            Assert.That(_context.KeywordColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.KeywordPen, Is.Not.SameAs(formerPen));
            Assert.That(_context.KeywordBrush, Is.Not.SameAs(formerBrush));

            formerBrush = _context.StringBrush;
            formerPen = _context.StringPen;
            _context.StringColor = Color.Pink;
            Assert.That(_context.StringColor, Is.EqualTo(Color.Pink));
            Assert.That(_context.StringPen, Is.Not.SameAs(formerPen));
            Assert.That(_context.StringBrush, Is.Not.SameAs(formerBrush));

            CheckPens(_context);
            CheckBrushes(_context);

            return;
        }

        class TestingCodeBox : CodeBox
        {
            public CodeRenderingContext RenderingContext
            {
                get { return (_workingContext); }
            }
        }
    }
}
