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
using Plexdata.QuickCopy.Exceptions;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace QuickCopyTests.Exceptions
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(NativeException))]
    public class NativeExceptionTests
    {
        [Test]
        public void NativeException_EmptyConstructor_MessageAsExpected()
        {
            NativeException instance = new NativeException();

            Assert.That(instance.HResult, Is.Zero);
            Assert.That(instance.Message, Is.EqualTo(this.GetExpectedMessage(null, 0)));
            Assert.That(instance.SystemMessage, Is.EqualTo(this.GetExpectedSystemMessage(0)));
        }

        [Test]
        public void NativeException_MessageConstructor_MessageAsExpected()
        {
            NativeException instance = new NativeException("exception");

            Assert.That(instance.HResult, Is.Zero);
            Assert.That(instance.Message, Is.EqualTo(this.GetExpectedMessage("exception", 0)));
            Assert.That(instance.SystemMessage, Is.EqualTo(this.GetExpectedSystemMessage(0)));
        }

        [Test]
        public void NativeException_MessageHResultConstructor_MessageAsExpected()
        {
            NativeException instance = new NativeException("exception", -2147024809);

            Assert.That(instance.HResult, Is.EqualTo(-2147024809));
            Assert.That(instance.Message, Is.EqualTo(this.GetExpectedMessage("exception", -2147024809)));
            Assert.That(instance.SystemMessage, Is.EqualTo(this.GetExpectedSystemMessage(-2147024809)));
        }

        private String GetExpectedMessage(String message, Int32 error)
        {
            if (message == null)
            {
                message = (new Exception()).Message.Replace("System.Exception", typeof(NativeException).FullName);
            }

            message += $" [HResult: 0x{error.ToString("X8")} ({(error & 0x0000FFFF)}, \"{this.GetExpectedSystemMessage(error)}\")]";

            return message;
        }

        private String GetExpectedSystemMessage(Int32 error)
        {
            return (new Win32Exception(error & 0x0000FFFF)).Message;
        }
    }
}
