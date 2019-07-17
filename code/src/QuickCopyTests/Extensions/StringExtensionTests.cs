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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(StringExtension))]
    public class StringExtensionTests
    {
        [Test]
        [TestCase(null, "<null>")]
        [TestCase("", "<empty>")]
        [TestCase("  ", "<empty>")]
        [TestCase("string", "string")]
        [TestCase(" string", "string")]
        [TestCase("string ", "string")]
        [TestCase(" string ", "string")]
        public void ToSafeString_StringOnly_ResultAsExpected(String actual, String expected)
        {
            Assert.That(actual.ToSafeString(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(null, "<null>")]
        [TestCase("", "<empty>")]
        [TestCase("  ", "<empty>")]
        [TestCase("string", "string")]
        [TestCase(" string", "string")]
        [TestCase("string ", "string")]
        [TestCase(" string ", "string")]
        [TestCase("string1;string2", "string1,string2")]
        [TestCase(" string1; string2", "string1,string2")]
        [TestCase("string1 ;string2 ", "string1,string2")]
        [TestCase(" string1 ; string2 ", "string1,string2")]
        [TestCase("  ; string2 ", ",string2")]
        [TestCase(" string1 ; ", "string1,")]
        public void ToSafeString_StringList_ResultAsExpected(String actual, String expected)
        {
            IEnumerable<String> items = null;

            if (actual == null)
            {
                items = null;
            }
            else if (actual.Trim() == String.Empty)
            {
                items = new String[0];
            }
            else
            {
                items = actual.Split(';');
            }

            Assert.That(items.ToSafeString(), Is.EqualTo(expected));
        }

        [Test]
        public void ToSafeHexString_BufferIsNull_ResultIsEmptyString()
        {
            Byte[] actual = null;
            Assert.That(actual.ToSafeHexString(), Is.EqualTo(String.Empty));
        }

        [Test]
        public void ToSafeHexString_BufferIsEmpty_ResultIsEmptyString()
        {
            Byte[] actual = new Byte[0];
            Assert.That(actual.ToSafeHexString(), Is.EqualTo(String.Empty));
        }

        [Test]
        public void ToSafeHexString_BufferIsValid_ResultAsExpected()
        {
            Byte[] actual = new Byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            Assert.That(actual.ToSafeHexString(), Is.EqualTo("000102030405060708090A0B0C0D0E0F"));
        }
    }
}
