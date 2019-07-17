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

using Plexdata.LogWriter.Abstraction;
using Plexdata.LogWriter.Extensions;
using Plexdata.QuickCopy.Exceptions;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Plexdata.QuickCopy.Native
{
    /// <summary>
    /// A Win32 API wrapper for reading and writing file operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class represents a wrapper for native reading and writing file operations. 
    /// All of the supported functionality uses the native the 
    /// Win32 API.
    /// </para>
    /// <para>
    /// The class has some constraints that has to be pointed out. These constraints are 
    /// especially related to reading and writing operations against the background of 
    /// increasing performance. See below for some of those constraints.
    /// </para>
    /// <para>
    /// <b>Sequential file scan mode:</b> Files are opened with flag FILE_FLAG_SEQUENTIAL_SCAN, 
    /// which in turn is a promise that all reading and/or writing operations are sequential.
    /// By using this flag the cache manager of the system IO control is able to optimize file 
    /// access operations. For more information about this flag please refer to 
    /// <see cref="https://devblogs.microsoft.com/oldnewthing/20120120-00/?p=8493"/>.
    /// </para>
    /// <para>
    /// <b>Disabling buffering mode:</b>
    /// By disabling the file buffering of the system IO control requires that all reading and/or 
    /// writing operations are performed with a block size of a multiple of 512 byte. This includes 
    /// the file size as well! Otherwise the system is neither able to read nor to write data onto 
    /// the local storage. This in turn implies that every reading and/or writing operation on any 
    /// kind of remote device must be performed with enabled buffering! For more information about 
    /// file buffering please refer to 
    /// <see cref="https://docs.microsoft.com/de-de/windows/win32/fileio/file-buffering"/> and to 
    /// <see cref="https://msdn.microsoft.com/en-us/windows/compatibility/advanced-format-disk-compatibility-update"/>.
    /// </para>
    /// </remarks>
    internal class AccessHandler : IDisposable
    {
        #region Private fields and constant values

        private static readonly IntPtr InvalidHandleValue = new IntPtr(unchecked(-1L));

        private IntPtr handle = IntPtr.Zero;

        private String filename = String.Empty;

        private readonly ILogger logger = null;

        #endregion

        #region Construction

        public AccessHandler(ILogger logger)
            : base()
        {
            this.logger = logger ?? throw new ArgumentException("The logger must not be null.");
            this.IsDisposed = false;
            this.IsWritable = false;
        }

        ~AccessHandler()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public properties

        public Boolean IsDisposed { get; private set; }

        public Boolean IsWritable { get; private set; }

        #endregion

        #region Public static methods

        /// <summary>
        /// Tries to create the file for provided name.
        /// </summary>
        /// <remarks>
        /// This method tries to create the file for provided name. The file creation 
        /// takes place according to additional parameters. A possibly existing file 
        /// will be overwritten if parameter <paramref name="overwrite"/> is <c>true</c>. 
        /// The length of the new file is adjusted if parameter <paramref name="length"/> 
        /// is greater than zero.
        /// </remarks>
        /// <param name="filename">
        /// The fully qualified name of the file.
        /// </param>
        /// <param name="overwrite">
        /// True to force an overwrite of an existing file and false otherwise.
        /// The method throws an exception if parameter <paramref name="overwrite"/> 
        /// is <c>false</c> and provided file already exists.
        /// </param>
        /// <param name="length">
        /// The length of the new file to be adjusted.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Either the provided filename is invalid or the provided length is less 
        /// than zero.
        /// </exception>
        /// <exception cref="NativeException">
        /// Something went wrong while creating the new file. The property 
        /// <see cref="NativeException.HResult"/> of the exception contains 
        /// the detailed error code.
        /// </exception>
        public static void Create(String filename, Boolean overwrite, Int64 length)
        {
            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("File name must not be null or empty or only consists of white spaces.");
            }

            if (length < 0)
            {
                throw new ArgumentException($"Requested length for file \"{filename}\" must be greater than zero.");
            }

            Int32 nMode = (Int32)(overwrite ? FileMode.CREATE_ALWAYS : FileMode.CREATE_NEW);
            Int32 nAccess = (Int32)FileAccess.GENERIC_WRITE;
            Int32 nShare = (Int32)FileShare.FILE_SHARE_WRITE;
            Int32 nFlags = (Int32)FileFlags.FILE_FLAG_NONE | (Int32)FileAttributes.FILE_ATTRIBUTE_NORMAL;

            IntPtr handle = AccessHandler.CreateFileW(filename, nAccess, nShare, IntPtr.Zero, nMode, nFlags, IntPtr.Zero);

            try
            {
                if (!AccessHandler.IsHandle(handle))
                {
                    throw new NativeException($"Creating of file \"{filename}\" has failed.", Marshal.GetHRForLastWin32Error());
                }

                if (length > 0)
                {
                    // Resize file up to requested file size.
                    if (!AccessHandler.SetFilePointerEx(handle, length, out Int64 result, MoveMethod.FILE_BEGIN))
                    {
                        throw new NativeException($"Changing length of file \"{filename}\" has failed.", Marshal.GetHRForLastWin32Error());
                    }

                    // According to docs this is required for changing current file size.
                    if (!AccessHandler.SetEndOfFile(handle))
                    {
                        throw new NativeException($"Writing length of file \"{filename}\" has failed.", Marshal.GetHRForLastWin32Error());
                    }
                }
            }
            finally
            {
                if (AccessHandler.IsHandle(handle))
                {
                    AccessHandler.CloseHandle(handle);
                }
            }
        }

        /// <summary>
        /// Tries to obtain the length in byte of provided file.
        /// </summary>
        /// <remarks>
        /// This method tries to obtain the length of provided file and returns 
        /// it if successful. The length returned by this method represents the 
        /// number of bytes of that file.
        /// </remarks>
        /// <param name="filename">
        /// The fully qualified name of the file.
        /// </param>
        /// <returns>
        /// The length in byte of the file.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The provided filename is invalid.
        /// </exception>
        /// <exception cref="NativeException">
        /// Something went wrong while obtaining the length of the file. The property 
        /// <see cref="NativeException.HResult"/> of the exception contains 
        /// the detailed error code.
        /// </exception>
        public static Int64 GetLength(String filename)
        {
            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("File name must not be null or empty or only consists of white spaces.");
            }

            Int32 nMode = (Int32)FileMode.OPEN_EXISTING;
            Int32 nAccess = (Int32)FileAccess.GENERIC_READ;
            Int32 nShare = (Int32)FileShare.FILE_SHARE_READ;
            Int32 nFlags = (Int32)FileFlags.FILE_FLAG_NONE | (Int32)FileAttributes.FILE_ATTRIBUTE_NORMAL;

            IntPtr handle = AccessHandler.CreateFileW(filename, nAccess, nShare, IntPtr.Zero, nMode, nFlags, IntPtr.Zero);

            try
            {
                if (!AccessHandler.IsHandle(handle))
                {
                    throw new NativeException($"Accessing file \"{filename}\" for read has failed.", Marshal.GetHRForLastWin32Error());
                }

                if (!AccessHandler.GetFileSizeEx(handle, out Int64 length))
                {
                    throw new NativeException($"Obtaining length of file \"{filename}\" has failed.", Marshal.GetHRForLastWin32Error());
                }

                return length;
            }
            finally
            {
                if (AccessHandler.IsHandle(handle))
                {
                    AccessHandler.CloseHandle(handle);
                }
            }
        }

        #endregion

        #region Public methods

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Opens provided file for sequential read access only.
        /// </summary>
        /// <remarks>
        /// This method tires to open the file for provided filename 
        /// for read access. In this case file buffering of the system 
        /// IO control is enabled.
        /// </remarks>
        /// <param name="filename">
        /// The fully qualified name of the file.
        /// </param>
        public void OpenRead(String filename)
        {
            this.OpenRead(filename, true);
        }

        /// <summary>
        /// Opens provided file for sequential read access only but with buffering 
        /// mode as requested. Recommendation: Always call this method with enabled 
        /// buffering!
        /// </summary>
        /// <param name="filename">
        /// The fully qualified name of the file.
        /// </param>
        /// <param name="buffering">
        /// True to enable buffering used by system IO control and false to 
        /// disable buffering mode. Be aware, disabling system IO control 
        /// buffering is not possible in every case. For example, reading 
        /// and/or writing operations will fails on each remote file.
        /// </param>
        public void OpenRead(String filename, Boolean buffering)
        {
            if (this.IsValid)
            {
                return;
            }

            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("File name must not be null or empty or only consists of white spaces.");
            }

            FileMode mode = FileMode.OPEN_EXISTING;
            FileAccess access = FileAccess.GENERIC_READ;
            FileShare share = FileShare.FILE_SHARE_READ;
            FileFlags flags = (buffering ? FileFlags.FILE_FLAG_NONE : FileFlags.FILE_FLAG_NO_BUFFERING) | FileFlags.FILE_FLAG_SEQUENTIAL_SCAN;
            FileAttributes attributes = FileAttributes.FILE_ATTRIBUTE_NORMAL;

            if (!this.OpenFile(filename, mode, access, share, flags, attributes))
            {
                throw new NativeException($"Open file \"{filename}\" for read access has failed.", Marshal.GetHRForLastWin32Error());
            }
        }

        public void OpenWrite(String filename)
        {
            this.OpenWrite(filename, true);
        }

        // Recommendation: Always call this method with enabled buffering!
        public void OpenWrite(String filename, Boolean buffering)
        {
            if (this.IsValid)
            {
                return;
            }

            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("File name must not be null or empty or only consists of white spaces.");
            }

            FileMode mode = FileMode.OPEN_EXISTING;
            FileAccess access = FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE;
            FileShare share = FileShare.FILE_SHARE_READ | FileShare.FILE_SHARE_WRITE;
            FileFlags flags = (buffering ? FileFlags.FILE_FLAG_NONE : FileFlags.FILE_FLAG_NO_BUFFERING) | FileFlags.FILE_FLAG_SEQUENTIAL_SCAN;
            FileAttributes attributes = FileAttributes.FILE_ATTRIBUTE_NORMAL;

            if (!this.OpenFile(filename, mode, access, share, flags, attributes))
            {
                throw new NativeException($"Open file \"{filename}\" for write access has failed.", Marshal.GetHRForLastWin32Error());
            }
        }

        public Boolean ReadChunk(Byte[] buffer)
        {
            return this.ReadChunk(buffer, out Int32 count);
        }

        public Boolean ReadChunk(Byte[] buffer, out Int32 count)
        {
            count = 0;

            if (!this.IsValid)
            {
                throw new ArgumentException("Cannot read from file with an invalid handle value.");
            }

            if (buffer == null || buffer.Length < 1)
            {
                throw new ArgumentException("The result buffer must have a minimum length of one byte.");
            }

            if (!AccessHandler.ReadFile(this.handle, buffer, (UInt32)buffer.Length, out UInt32 bytes, IntPtr.Zero))
            {
                throw new NativeException($"Reading from file \"{this.filename}\" has failed.", Marshal.GetHRForLastWin32Error());
            }

            // Test for end of the file.
            if (bytes == 0)
            {
                return false;
            }

            count = Convert.ToInt32(bytes);

            return true;
        }

        public Boolean WriteChunk(Byte[] buffer, Int32 length)
        {
            return this.WriteChunk(buffer, length, out Int32 written);
        }

        public Boolean WriteChunk(Byte[] buffer, Int32 length, out Int32 written)
        {
            written = 0;

            if (!this.IsValid)
            {
                throw new ArgumentException("Cannot write into file with an invalid handle value.");
            }

            if (buffer == null || buffer.Length < 1)
            {
                throw new ArgumentException("The input buffer must have a minimum length of one byte.");
            }

            if (length > buffer.Length)
            {
                throw new ArgumentException("Parameter \"length\" must be less than or equal to the length of provided buffer.");
            }

            if (!AccessHandler.WriteFile(this.handle, buffer, (UInt32)length, out UInt32 bytes, IntPtr.Zero))
            {
                throw new NativeException($"Writing into file \"{this.filename}\" has failed.", Marshal.GetHRForLastWin32Error());
            }

            written = Convert.ToInt32(bytes);

            return written > 0 && length == written;
        }

        public Boolean Flush()
        {
            Boolean result = true;

            if (this.IsValid)
            {
                try
                {
                    if (this.IsWritable && !AccessHandler.FlushFileBuffers(this.handle))
                    {
                        throw new NativeException("Flushing file buffers has failed.", Marshal.GetHRForLastWin32Error());
                    }
                }
                catch (Exception exception)
                {
                    result = false;
                    this.logger.Warning(MethodBase.GetCurrentMethod(), exception, this.GetDetail(nameof(this.filename), this.filename));
                }
            }

            return result;
        }

        public Boolean Close()
        {
            Boolean result = true;

            if (this.IsValid)
            {
                try
                {
                    if (!AccessHandler.CloseHandle(this.handle))
                    {
                        throw new NativeException("Closing file handle has failed.", Marshal.GetHRForLastWin32Error());
                    }

                    this.handle = IntPtr.Zero;
                }
                catch (Exception exception)
                {
                    result = false;
                    this.logger.Warning(MethodBase.GetCurrentMethod(), exception, this.GetDetail(nameof(this.filename), this.filename));
                }
            }

            return result;
        }

        #endregion

        #region Protected methods

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing) { /* Dispose managed resources. */ }

                this.Flush();
                this.Close();
                this.filename = String.Empty;

                this.IsDisposed = true;
            }
        }

        #endregion

        #region Private properties

        private Boolean IsValid
        {
            get
            {
                return AccessHandler.IsHandle(this.handle);
            }
        }

        #endregion

        #region Private static methods

        private static Boolean IsHandle(IntPtr value)
        {
            return value != IntPtr.Zero && value != AccessHandler.InvalidHandleValue;
        }

        #endregion

        #region Private methods

        private Boolean OpenFile(String filename, FileMode mode, FileAccess access, FileShare share, FileFlags flags, FileAttributes attributes)
        {
            Int32 nMode = (Int32)mode;
            Int32 nAccess = (Int32)access;
            Int32 nShare = (Int32)share;
            Int32 nFlags = (Int32)flags | (Int32)attributes;

            IntPtr handle = AccessHandler.CreateFileW(filename, nAccess, nShare, IntPtr.Zero, nMode, nFlags, IntPtr.Zero);

            if (!AccessHandler.IsHandle(handle))
            {
                return false;
            }

            this.IsWritable = (access & FileAccess.GENERIC_WRITE) == FileAccess.GENERIC_WRITE;
            this.handle = handle;
            this.filename = filename;

            return true;
        }

        #endregion

        #region Logging helpers

        private (String Label, Object Value) GetDetail(String label, String value)
        {
            return (Label: label, Value: value);
        }

        #endregion

        #region Native access methods

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

        // https://docs.microsoft.com/de-de/windows/desktop/api/handleapi/nf-handleapi-closehandle
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean CloseHandle(
            IntPtr hObject
        );

        // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-flushfilebuffers
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean FlushFileBuffers(
            IntPtr hFile
        );

        // https://docs.microsoft.com/de-de/windows/win32/api/fileapi/nf-fileapi-getfilesizeex
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean GetFileSizeEx(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.U8)] out Int64 lpFileSize
        );

        // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfilepointerex
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean SetFilePointerEx(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.I8)] Int64 liDistanceToMove,
            [MarshalAs(UnmanagedType.I8)] out Int64 lpNewFilePointer,
            [MarshalAs(UnmanagedType.U4)] MoveMethod dwMoveMethod
        );

        // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setendoffile
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean SetEndOfFile(
            IntPtr hFile
        );

        // https://docs.microsoft.com/de-de/windows/desktop/api/fileapi/nf-fileapi-readfile
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean ReadFile(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] [Out] Byte[] lpBuffer,
            [MarshalAs(UnmanagedType.U4)] UInt32 nNumberOfBytesToRead,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpNumberOfBytesRead,
            IntPtr lpOverlapped
        );

        // https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-writefile
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean WriteFile(
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPArray)] Byte[] lpBuffer,
            [MarshalAs(UnmanagedType.U4)] UInt32 nNumberOfBytesToWrite,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpNumberOfBytesWritten,
            IntPtr lpOverlapped
        );

        #endregion

        #region Native access types

        [Flags]
        private enum FileAccess : Int32
        {
            GENERIC_NONE = 0,               // 0x00000000
            GENERIC_READ = -2147483648,     // 0x80000000
            GENERIC_WRITE = 1073741824,     // 0x40000000
            GENERIC_EXECUTE = 536870912,    // 0x20000000
            GENERIC_ALL = 268435456,        // 0x10000000
        }

        [Flags]
        private enum FileShare : Int32
        {
            FILE_SHARE_NONE = 0,    // 0x00000000
            FILE_SHARE_READ = 1,    // 0x00000001
            FILE_SHARE_WRITE = 2,   // 0x00000002
            FILE_SHARE_DELETE = 4,  // 0x00000004
        }

        private enum FileMode : Int32
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5,
        }

        [Flags]
        private enum FileFlags : Int32
        {
            // For the moment: Flags only for create file
            FILE_FLAG_NONE = 0,                         // 0x00000000
            FILE_FLAG_BACKUP_SEMANTICS = 33554432,      // 0x02000000
            FILE_FLAG_DELETE_ON_CLOSE = 67108864,       // 0x04000000
            FILE_FLAG_NO_BUFFERING = 536870912,         // 0x20000000
            FILE_FLAG_OPEN_NO_RECALL = 1048576,         // 0x00100000
            FILE_FLAG_OPEN_REPARSE_POINT = 2097152,     // 0x00200000
            FILE_FLAG_OVERLAPPED = 1073741824,          // 0x40000000
            FILE_FLAG_POSIX_SEMANTICS = 1048576,        // 0x00100000
            FILE_FLAG_RANDOM_ACCESS = 268435456,        // 0x10000000
            FILE_FLAG_SESSION_AWARE = 8388608,          // 0x00800000
            FILE_FLAG_SEQUENTIAL_SCAN = 134217728,      // 0x08000000
            FILE_FLAG_WRITE_THROUGH = -2147483648,      // 0x80000000
        }

        [Flags]
        private enum FileAttributes : Int32
        {
            // For the moment: Attributes only for create file
            FILE_ATTRIBUTE_NONE = 0,            // 0x0000
            FILE_ATTRIBUTE_READONLY = 1,        // 0x0001
            FILE_ATTRIBUTE_HIDDEN = 2,          // 0x0002
            FILE_ATTRIBUTE_SYSTEM = 4,          // 0x0004
            FILE_ATTRIBUTE_ARCHIVE = 32,        // 0x0020
            FILE_ATTRIBUTE_NORMAL = 128,        // 0x0080
            FILE_ATTRIBUTE_TEMPORARY = 256,     // 0x0100
            FILE_ATTRIBUTE_OFFLINE = 4096,      // 0x1000
            FILE_ATTRIBUTE_ENCRYPTED = 16384,   // 0x4000
        }

        private enum MoveMethod
        {
            FILE_BEGIN = 0,
            FILE_CURRENT = 1,
            FILE_END = 2
        }

        #endregion
    }
}
