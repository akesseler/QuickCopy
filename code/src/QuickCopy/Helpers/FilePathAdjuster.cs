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

namespace Plexdata.QuickCopy.Helpers
{
    internal static class FilePathAdjuster
    {
        #region Private constants

        private const String StdLongPathPrefix = @"\\?\";

        private const String UncLongPathPrefix = @"\\?\UNC\";

        #endregion

        #region Public methods

        /// <summary>
        /// Converts the <paramref name="path"/> into its long path representation.
        /// </summary>
        /// <remarks>
        /// A standard path like <c>c:\path\file</c> is converted into <c>\\?\c:\path\file</c> and a UNC 
        /// path like <c>\\server\share\path\file</c> is converted into <c>\\?\UNC\server\share\path\file</c>.
        /// </remarks>
        /// <param name="path">
        /// The path to convert.
        /// </param>
        /// <returns>
        /// The long path representation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="path"/> is <c>null</c>, or <c>empty</c>, 
        /// or consists only of whitespaces.
        /// </exception>
        public static String ToLongPath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path must not be null or empty or consists only of whitespaces.");
            }

            if (path.Length <= 2)
            {
                return path;
            }

            if (FilePathAdjuster.IsUncPath(path))
            {
                if (!FilePathAdjuster.IsLongPathPrefix(path))
                {
                    // Remove all leading backslashes and prepend UNC long path prefix.
                    path = path.TrimStart('\\').Insert(0, FilePathAdjuster.UncLongPathPrefix);
                }
            }
            else
            {
                if (!FilePathAdjuster.IsLongPathPrefix(path))
                {
                    // Just prepend standard long path prefix.
                    path = path.Insert(0, FilePathAdjuster.StdLongPathPrefix);
                }
            }

            return path;
        }

        /// <summary>
        /// Checks if <paramref name="path"/> is formatted as UNC path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A UNC path looks like <c>\\server\share\path\file</c> or like 
        /// <c>\\?\UNC\server\share\path\file</c>. In these cases the method 
        /// should return <c>true</c>. Additionally, refer to link below for 
        /// more information.
        /// </para>
        /// <para>
        /// MSDN: https://docs.microsoft.com/de-de/windows/desktop/FileIO/naming-a-file
        /// </para>
        /// </remarks>
        /// <example>
        /// Below find some examples with their expected results.
        /// <code>
        /// null                   => exception
        /// ""                     => exception
        /// "    "                 => exception
        /// "\\"                   => false
        /// "\\server\share"       => true
        /// "\\?\unc\server\share" => true
        /// "\\?\c:\path\file"     => false
        /// "c:\path\file"         => false
        /// </code>
        /// </example>
        /// <param name="path">
        /// The path to check.
        /// </param>
        /// <returns>
        /// True, if <paramref name="path"/> starts either with <c>\\</c> or 
        /// with <c>\\?\UNC\</c>. Otherwise, false is returned.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="path"/> is <c>null</c>, or <c>empty</c>, 
        /// or consists only of whitespaces.
        /// </exception>
        public static Boolean IsUncPath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path must not be null or empty or consists only of whitespaces.");
            }

            if (!path.StartsWith(@"\\"))
            {
                return false;
            }

            // Check for UNC long path prefix.
            if (path.StartsWith(FilePathAdjuster.UncLongPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return path.Length > 2 && !path.StartsWith(FilePathAdjuster.StdLongPathPrefix, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Checks if <paramref name="path"/> is prefixed by a long path prefix.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A long path prefix can be either <c>\\?\</c> or <c>\\?\UNC\</c>. In 
        /// these cases the method should return <c>true</c>. Additionally, refer 
        /// to link below for more information.
        /// </para>
        /// <para>
        /// MSDN: https://docs.microsoft.com/de-de/windows/desktop/FileIO/naming-a-file
        /// </para>
        /// </remarks>
        /// <example>
        /// Below find some examples with their expected results.
        /// <code>
        /// null                   => exception
        /// ""                     => exception
        /// "    "                 => exception
        /// "\\"                   => false
        /// "\\server\share"       => false
        /// "\\?\unc\server\share" => true
        /// "\\?\c:\path\file"     => true
        /// "c:\path\file"         => false
        /// </code>
        /// </example>
        /// <param name="path">
        /// The path to check.
        /// </param>
        /// <returns>
        /// True, if <paramref name="path"/> starts either with <c>\\?\</c> or 
        /// with <c>\\?\UNC\</c>. Otherwise, false is returned.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="path"/> is <c>null</c>, or <c>empty</c>, 
        /// or consists only of whitespaces.
        /// </exception>
        public static Boolean IsLongPathPrefix(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path must not be null or empty or consists only of whitespaces.");
            }

            return
                path.StartsWith(FilePathAdjuster.UncLongPathPrefix, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(FilePathAdjuster.StdLongPathPrefix, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
