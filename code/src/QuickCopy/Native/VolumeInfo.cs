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

namespace Plexdata.QuickCopy.Native
{
    internal class VolumeInfo
    {
        #region Private fields

        private readonly UInt32 sectorsPerCluster = 0;

        private readonly UInt32 bytesPerSector = 0;

        private readonly UInt32 numberOfFreeClusters = 0;

        private readonly UInt32 totalNumberOfClusters = 0;

        #endregion

        #region Construction

        public VolumeInfo(String filename)
        {
            if (String.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename must not be null or empty or consists only of whitespaces.");
            }

            this.PathRoot = Path.GetPathRoot(filename);

            if (!VolumeInfo.GetDiskFreeSpaceW(this.PathRoot, out this.sectorsPerCluster, out this.bytesPerSector, out this.numberOfFreeClusters, out this.totalNumberOfClusters))
            {
                throw new NativeException($"Obtaining volume information for {this.PathRoot} has failed.", Marshal.GetHRForLastWin32Error());
            }
        }

        #endregion

        #region Public properties

        public String PathRoot { get; private set; }

        public UInt32 SectorsPerCluster
        {
            get
            {
                return this.sectorsPerCluster;
            }
        }

        public UInt32 BytesPerSector
        {
            get
            {
                return this.bytesPerSector;
            }
        }

        public UInt32 PageSize
        {
            get
            {
                // NOTE: This page size calculation is indeed a guess!
                return this.bytesPerSector * this.sectorsPerCluster;
            }
        }

        public UInt32 NumberOfFreeClusters
        {
            get
            {
                return this.numberOfFreeClusters;
            }
        }

        public UInt32 TotalNumberOfClusters
        {
            get
            {
                return this.totalNumberOfClusters;
            }
        }

        #endregion

        #region Native access

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean GetDiskFreeSpaceW(
            String lpRootPathName,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpSectorsPerCluster,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpBytesPerSector,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpNumberOfFreeClusters,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpTotalNumberOfClusters
        );

        #endregion
    }
}
