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
using Plexdata.QuickCopy.Models;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Models
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(Arguments))]
    public class ArgumentsTests
    {
        [Test]
        public void ArgumentsTests_DefaultsValidation_ResultIsDefaultsAsExpected()
        {
            Arguments instance = new Arguments();

            Assert.That(instance.Source, Is.Empty);
            Assert.That(instance.Target, Is.Empty);
            Assert.That(instance.Pattern, Is.EqualTo("*.*"));
            Assert.That(instance.IsMove, Is.False);
            Assert.That(instance.IsVerify, Is.False);
            Assert.That(instance.IsOverwrite, Is.False);
            Assert.That(instance.IsRecursive, Is.False);
            Assert.That(instance.IsVersion, Is.False);
            Assert.That(instance.IsHelp, Is.False);
            Assert.That(instance.Files, Is.Empty);
        }
    }
}
