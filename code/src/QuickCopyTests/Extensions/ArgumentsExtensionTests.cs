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
using Plexdata.QuickCopy.Extensions;
using Plexdata.QuickCopy.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(ArgumentsExtension))]
    public class ArgumentsExtensionTests
    {
        [Test]
        public void ValidateSourceOrThrow_ArgumentsAreNull_ThrowsArgumentException()
        {
            Arguments instance = null;
            Assert.That(() => instance.ValidateSourceOrThrow(), Throws.ArgumentException);
        }

        [Test]
        public void ValidateSourceOrThrow_SourceRuleCheck_ThrowsArgumentException()
        {
            Arguments instance = new Arguments
            {
                Source = "source",
                Files = new String[] { "file1" }
            };

            Assert.That(() => instance.ValidateSourceOrThrow(), Throws.ArgumentException);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("  ", null)]
        [TestCase(null, 0)]
        [TestCase("", 0)]
        [TestCase("  ", 0)]
        public void ValidateSourceOrThrow_FilesRuleCheck_ThrowsArgumentException(String source, Int32? files)
        {
            Arguments instance = new Arguments
            {
                Source = source,
                Files = files != null ? new String[files.Value] : null
            };

            Assert.That(() => instance.ValidateSourceOrThrow(), Throws.ArgumentException);
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        public void ValidateSourceOrThrow_SourceRuleCheck_ThrowsNothing(Int32? files)
        {
            Arguments instance = new Arguments
            {
                Source = "source",
                Files = files != null ? new String[files.Value] : null
            };

            Assert.That(() => instance.ValidateSourceOrThrow(), Throws.Nothing);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ValidateSourceOrThrow_FilesRuleCheck_ThrowsNothing(String source)
        {
            Arguments instance = new Arguments
            {
                Source = source,
                Files = new String[] { "file1" }
            };

            Assert.That(() => instance.ValidateSourceOrThrow(), Throws.Nothing);
        }

        [Test]
        public void ValidateTargetOrThrow_ArgumentsAreNull_ThrowsArgumentException()
        {
            Arguments instance = null;
            Assert.That(() => instance.ValidateTargetOrThrow(), Throws.ArgumentException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ValidateTargetOrThrow_FilesRuleCheck_ThrowsArgumentException(String target)
        {
            Arguments instance = new Arguments
            {
                Target = target
            };

            Assert.That(() => instance.ValidateTargetOrThrow(), Throws.ArgumentException);
        }

        [Test]
        public void ValidateTargetOrThrow_FilesRuleCheck_ThrowsNothing()
        {
            Arguments instance = new Arguments
            {
                Target = "target"
            };

            Assert.That(() => instance.ValidateTargetOrThrow(), Throws.Nothing);
        }
    }
}
