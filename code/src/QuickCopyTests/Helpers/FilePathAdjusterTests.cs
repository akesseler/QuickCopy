/*
 * MIT License
 * 
 * Copyright (c) 2019 plexdata.de
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using NUnit.Framework;
using Plexdata.QuickCopy.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Helpers
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(FilePathAdjuster))]
    public class FilePathAdjusterTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ToLongPath_PathInvalid_ThrowsArgumentException(String path)
        {
            Assert.That(() => FilePathAdjuster.ToLongPath(path), Throws.ArgumentException);
        }

        [Test]
        [TestCase("a")]
        [TestCase("aa")]
        public void ToLongPath_PathTooShort_ResultIsPath(String path)
        {
            Assert.That(FilePathAdjuster.ToLongPath(path), Is.EqualTo(path));
        }

        [Test]
        [TestCase(@"\\?\", @"\\?\")]
        [TestCase(@"\\?\unc\", @"\\?\unc\")]
        [TestCase(@"\\server\share", @"\\?\UNC\server\share")]
        [TestCase(@"\\?\unc\server\share", @"\\?\unc\server\share")]
        [TestCase(@"\\?\c:\path\file", @"\\?\c:\path\file")]
        [TestCase(@"c:\path\file", @"\\?\c:\path\file")]
        public void ToLongPath_PathValid_ResultAsExpected(String path, String expected)
        {
            Assert.That(FilePathAdjuster.ToLongPath(path), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void IsUncPath_PathInvalid_ThrowsArgumentException(String path)
        {
            Assert.That(() => FilePathAdjuster.IsUncPath(path), Throws.ArgumentException);
        }

        [Test]
        [TestCase(@"a", false)]
        [TestCase(@"aa", false)]
        [TestCase(@"\", false)]
        [TestCase(@"\\", false)]
        [TestCase(@"\\?\", false)]
        [TestCase(@"\\?\unc\", true)]
        [TestCase(@"\\server\share", true)]
        [TestCase(@"\\?\unc\server\share", true)]
        [TestCase(@"\\?\c:\path\file", false)]
        [TestCase(@"c:\path\file", false)]
        public void IsUncPath_PathValid_ResultAsExpected(String path, Boolean expected)
        {
            Assert.That(FilePathAdjuster.IsUncPath(path), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void IsLongPathPrefix_PathInvalid_ThrowsArgumentException(String path)
        {
            Assert.That(() => FilePathAdjuster.IsLongPathPrefix(path), Throws.ArgumentException);
        }

        [Test]
        [TestCase(@"a", false)]
        [TestCase(@"aa", false)]
        [TestCase(@"\", false)]
        [TestCase(@"\\", false)]
        [TestCase(@"\\?\", true)]
        [TestCase(@"\\?\unc\", true)]
        [TestCase(@"\\server\share", false)]
        [TestCase(@"\\?\unc\server\share", true)]
        [TestCase(@"\\?\c:\path\file", true)]
        [TestCase(@"c:\path\file", false)]
        public void IsLongPathPrefix_PathValid_ResultAsExpected(String path, Boolean expected)
        {
            Assert.That(FilePathAdjuster.IsLongPathPrefix(path), Is.EqualTo(expected));
        }
    }
}
