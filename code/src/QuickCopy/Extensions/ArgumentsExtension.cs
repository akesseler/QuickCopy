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

using Plexdata.QuickCopy.Models;
using System;

namespace Plexdata.QuickCopy.Extensions
{
    internal static class ArgumentsExtension
    {
        #region Public methods

        /// <summary>
        /// Rule check for properties source and files of command line 
        /// arguments. Only source or files is allowed.
        /// </summary>
        /// <param name="value">
        /// The instance of command line arguments to be checked.
        /// </param>
        public static void ValidateSourceOrThrow(this Arguments value)
        {
            if (value == null)
            {
                throw new ArgumentException("Arguments instance is null.");
            }

            if (String.IsNullOrWhiteSpace(value.Source))
            {
                if (value.Files == null || value.Files.Length <= 0)
                {
                    throw new ArgumentException("List of files must be used if source folder is not set.");
                }
            }
            else
            {
                if (value.Files != null && value.Files.Length > 0)
                {
                    throw new ArgumentException("List of files cannot be used if source folder is set.");
                }
            }
        }

        /// <summary>
        /// Rule check for property target of command line arguments.
        /// </summary>
        /// <param name="value">
        /// The instance of command line arguments to be checked.
        /// </param>
        public static void ValidateTargetOrThrow(this Arguments value)
        {
            if (value == null)
            {
                throw new ArgumentException("Arguments instance is null.");
            }

            if (String.IsNullOrWhiteSpace(value.Target))
            {
                throw new ArgumentException("Target folder must be set.");
            }
        }

        #endregion
    }
}
