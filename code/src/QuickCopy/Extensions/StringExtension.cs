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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plexdata.QuickCopy.Extensions
{
    internal static class StringExtension
    {
        #region Public methods

        public static String ToSafeString(this String value)
        {
            if (value == null)
            {
                return "<null>";
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                return "<empty>";
            }

            return value.Trim();
        }

        public static String ToSafeString(this IEnumerable<String> value)
        {
            if (value == null)
            {
                return StringExtension.ToSafeString((String)null);
            }

            if (!value.Any())
            {
                return StringExtension.ToSafeString(String.Empty);
            }

            return String.Join(",", value.Select(x => x.Trim()));
        }

        public static String ToSafeHexString(this Byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder(2 * value.Length);

            foreach (Byte current in value)
            {
                builder.Append(current.ToString("X2"));
            }

            return builder.ToString();
        }

        #endregion
    }
}
