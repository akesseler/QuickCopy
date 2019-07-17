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

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security;

namespace Plexdata.QuickCopy.Exceptions
{
    [Serializable]
    public class NativeException : Exception
    {
        public NativeException()
            : base()
        {
            base.HResult = 0;
        }

        public NativeException(String message)
            : base(message)
        {
            base.HResult = 0;
        }

        public NativeException(String message, Int32 hResult)
            : base(message)
        {
            base.HResult = hResult;
        }

        public NativeException(String message, Exception innerException)
            : base(message, innerException)
        {
            base.HResult = 0;
        }

        [SecuritySafeCritical]
        protected NativeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override String Message
        {
            get
            {
                return $"{base.Message} [{nameof(base.HResult)}: 0x{base.HResult.ToString("X8")} ({this.GetWin32ErrorCode()}, \"{this.SystemMessage}\")]";
            }
        }

        public String SystemMessage
        {
            get
            {
                return (new Win32Exception(this.GetWin32ErrorCode())).Message;
            }
        }

        private Int32 GetWin32ErrorCode()
        {
            // Cut off FACILITY_WIN32 and SEVERITY_ERROR (or whatever is inside the first two byte).
            return (Int32)((UInt32)base.HResult & 0x0000FFFF);
        }
    }
}
