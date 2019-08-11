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

using Plexdata.QuickCopy.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plexdata.QuickCopy.Native
{
    /// <summary>
    /// A Win32 API wrapper to resolve the final path name for reparse points.
    /// </summary>
    /// <remarks>
    /// This class represents a wrapper to be able to resolve the physical path 
    /// of files with attribute "reparse point".
    /// </remarks>
    internal static class ReparseResolver
    {
        #region Private fields and constant values

        private static readonly IntPtr InvalidHandleValue = new IntPtr(unchecked(-1L));

        // File Access
        private const Int32 FILE_READ_EA = 8; // 0x0008 

        // File Share
        private const Int32 FILE_SHARE_NONE = 0;    // 0x00000000
        private const Int32 FILE_SHARE_READ = 1;    // 0x00000001
        private const Int32 FILE_SHARE_WRITE = 2;   // 0x00000002
        private const Int32 FILE_SHARE_DELETE = 4;  // 0x00000004

        // File Mode
        private const Int32 OPEN_EXISTING = 3;

        // File Flags
        private const Int32 FILE_FLAG_BACKUP_SEMANTICS = 33554432; // 0x02000000

        // Final Path Name Flags
        private const Int32 FILE_NAME_NORMALIZED = 0;

        #endregion

        #region Public static methods

        /// <summary>
        /// Tries to resolve the physical file path.
        /// </summary>
        /// <remarks>
        /// This method tries to resolve the physical file path of 
        /// provided <paramref name="filename"/>. The full name is 
        /// returned if parameter is not <c>null</c> or <c>empty</c>, 
        /// no matter if the file needs to be resolved or not.
        /// </remarks>
        /// <param name="filename">
        /// The file name to resolve.
        /// </param>
        /// <param name="resolved">
        /// True if file name could be resolved and false otherwise.
        /// </param>
        /// <returns>
        /// The full path of the resolved file name.
        /// </returns>
        /// <exception cref="NativeException">
        /// This exception is thrown in any error case. For example 
        /// the requested file name could not be opened for read access.
        /// </exception>
        public static String Resolve(String filename, out Boolean resolved)
        {
            resolved = false;

            if (String.IsNullOrWhiteSpace(filename))
            {
                return filename;
            }

            FileInfo info = new FileInfo(Path.GetFullPath(filename));

            if ((info.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
            {
                return info.FullName;
            }

            filename = ReparseResolver.GetFinalPathName(info.FullName);
            resolved = true;
            return filename;
        }

        #endregion

        #region Private static methods

        private static String GetFinalPathName(String path)
        {
            Int32 nAccess = ReparseResolver.FILE_READ_EA;
            Int32 nShare = ReparseResolver.FILE_SHARE_READ | ReparseResolver.FILE_SHARE_WRITE | ReparseResolver.FILE_SHARE_DELETE;
            Int32 nMode = ReparseResolver.OPEN_EXISTING;
            Int32 nFlags = ReparseResolver.FILE_FLAG_BACKUP_SEMANTICS;

            IntPtr handle = ReparseResolver.CreateFileW(path, nAccess, nShare, IntPtr.Zero, nMode, nFlags, IntPtr.Zero);

            if (handle == ReparseResolver.InvalidHandleValue)
            {
                throw new NativeException($"Could not resolve final path name for \"{path}\".", Marshal.GetHRForLastWin32Error());
            }

            try
            {
                nFlags = ReparseResolver.FILE_NAME_NORMALIZED;

                // Obtain required result size.
                Int32 length = ReparseResolver.GetFinalPathNameByHandleW(handle, null, 0, nFlags);

                if (length < 1)
                {
                    throw new NativeException($"Could not obtain required buffer size to resolve final path name for \"{path}\".", Marshal.GetHRForLastWin32Error());
                }

                StringBuilder builder = new StringBuilder(length);

                Int32 result = ReparseResolver.GetFinalPathNameByHandleW(handle, builder, length, nFlags);

                if (result < 1)
                {
                    throw new NativeException($"Resolving final path name for \"{path}\" has failed.", Marshal.GetHRForLastWin32Error());
                }

                return builder.ToString();
            }
            finally
            {
                ReparseResolver.CloseHandle(handle);
            }
        }

        #endregion

        #region Native methods

        // https://docs.microsoft.com/de-de/windows/desktop/api/fileapi/nf-fileapi-createfilew
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] String lpFileName,
            [MarshalAs(UnmanagedType.U4)] Int32 dwDesiredAccess,        // FileAccess
            [MarshalAs(UnmanagedType.U4)] Int32 dwShareMode,            // FileShare
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] Int32 dwCreationDisposition,  // FileMode
            [MarshalAs(UnmanagedType.U4)] Int32 dwFlagsAndAttributes,   // FileFlags & FileAttributes
            IntPtr hTemplateFile
        );

        // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern Int32 GetFinalPathNameByHandleW(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszFilePath,
            [MarshalAs(UnmanagedType.U4)]     Int32 cchFilePath,
            [MarshalAs(UnmanagedType.U4)]     Int32 dwFlags
        );

        // https://docs.microsoft.com/de-de/windows/desktop/api/handleapi/nf-handleapi-closehandle
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean CloseHandle(
            IntPtr hObject
        );

        #endregion
    }
}
