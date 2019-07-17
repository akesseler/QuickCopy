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
using System;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(IntegerExtension))]
    public class IntegerExtensionTests
    {
        [Test]
        [TestCase(1,"1")]
        [TestCase(10, "10")]
        [TestCase(100, "100")]
        [TestCase(1000, "1,000")]
        [TestCase(10000, "10,000")]
        [TestCase(100000, "100,000")]
        [TestCase(1000000, "1,000,000")]
        [TestCase(10000000, "10,000,000")]
        [TestCase(100000000, "100,000,000")]
        [TestCase(1000000000, "1,000,000,000")]
        [SetUICulture("en-US")]
        public void ToSafeString_ValueInt32Only_ResultAsExpected(Int32 actual, String expected)
        {
            Assert.That(actual.ToSafeString(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(1, " unit ", "1 unit")]
        [TestCase(10, " unit ", "10 unit")]
        [TestCase(100, " unit ", "100 unit")]
        [TestCase(1000, " unit ", "1,000 unit")]
        [TestCase(10000, " unit ", "10,000 unit")]
        [TestCase(100000, " unit ", "100,000 unit")]
        [TestCase(1000000, " unit ", "1,000,000 unit")]
        [TestCase(10000000, " unit ", "10,000,000 unit")]
        [TestCase(100000000, " unit ", "100,000,000 unit")]
        [TestCase(1000000000, " unit ", "1,000,000,000 unit")]
        [SetUICulture("en-US")]
        public void ToSafeString_ValueInt32Unit_ResultAsExpected(Int32 actual, String unit, String expected)
        {
            Assert.That(actual.ToSafeString(unit), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(1, "1")]
        [TestCase(10, "10")]
        [TestCase(100, "100")]
        [TestCase(1000, "1,000")]
        [TestCase(10000, "10,000")]
        [TestCase(100000, "100,000")]
        [TestCase(1000000, "1,000,000")]
        [TestCase(10000000, "10,000,000")]
        [TestCase(100000000, "100,000,000")]
        [TestCase(1000000000, "1,000,000,000")]
        [SetUICulture("en-US")]
        public void ToSafeString_ValueInt64Only_ResultAsExpected(Int64 actual, String expected)
        {
            Assert.That(actual.ToSafeString(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(1, " unit ", "1 unit")]
        [TestCase(10, " unit ", "10 unit")]
        [TestCase(100, " unit ", "100 unit")]
        [TestCase(1000, " unit ", "1,000 unit")]
        [TestCase(10000, " unit ", "10,000 unit")]
        [TestCase(100000, " unit ", "100,000 unit")]
        [TestCase(1000000, " unit ", "1,000,000 unit")]
        [TestCase(10000000, " unit ", "10,000,000 unit")]
        [TestCase(100000000, " unit ", "100,000,000 unit")]
        [TestCase(1000000000, " unit ", "1,000,000,000 unit")]
        [SetUICulture("en-US")]
        public void ToSafeString_ValueInt64Unit_ResultAsExpected(Int64 actual, String unit, String expected)
        {
            Assert.That(actual.ToSafeString(unit), Is.EqualTo(expected));
        }
    }
}
