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
using Plexdata.QuickCopy.Extensions;
using Plexdata.QuickCopy.Models;
using Plexdata.QuickCopy.Native;
using Plexdata.QuickCopy.Timers;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace Plexdata.QuickCopy.Handlers
{
    internal class PlaylistEntryHandler
    {
        #region Private fields

        private readonly ILogger logger = null;

        private readonly CancellationToken token;

        #endregion

        #region Construction

        public PlaylistEntryHandler(ILogger logger, PlaylistEntry entry, CancellationToken token)
            : base()
        {
            this.logger = logger ?? throw new ArgumentException("The logger must not be null.");
            this.Entry = entry ?? throw new ArgumentException("The playlist entry must not be null.");
            this.token = token;
        }

        #endregion

        #region Public properties

        public PlaylistEntry Entry { get; private set; }

        public Boolean IsError { get; private set; }

        public Boolean IsCancel
        {
            get
            {
                return this.token.IsCancellationRequested;
            }
        }

        #endregion

        #region Public methods

        public void Execute()
        {
            try
            {
                if (this.IsAbort) { return; }

                // Check file name equality.
                if (!this.CheckDiffer(this.Entry.Source, this.Entry.Target))
                {
                    return;
                }

                if (this.IsAbort) { return; }

                // Check and validate source file.
                if (!this.CheckSource(this.Entry.Source))
                {
                    return;
                }

                if (this.IsAbort) { return; }

                // Check and validate target file.
                if (!this.CheckTarget(this.Entry.Target))
                {
                    return;
                }

                if (this.IsAbort) { return; }

                using (new PlaylistEntryExecutionTimer(this.logger, this.GetExecutionDetails()))
                using (IncrementalHash sourceHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
                using (IncrementalHash targetHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
                {
                    UInt32 bufferLength = this.GetMinimumBufferLength(this.Entry.Source, this.Entry.Target);

                    if (this.Entry.IsVerify)
                    {
                        this.CopyFiles(this.Entry.Source, this.Entry.Target, bufferLength, this.Entry.IsOverwrite, sourceHash);
                    }
                    else
                    {
                        this.CopyFiles(this.Entry.Source, this.Entry.Target, bufferLength, this.Entry.IsOverwrite, null);
                    }

                    if (this.IsAbort) { return; }

                    if (this.Entry.IsVerify)
                    {
                        this.VerifyFiles(this.Entry.Target, bufferLength, sourceHash, targetHash);
                    }

                    if (this.IsAbort) { return; }

                    this.ApplyDetails(this.Entry.Source, this.Entry.Target);
                }
            }
            catch (Exception exception)
            {
                this.IsError = true;
                this.logger.Critical(
                    MethodBase.GetCurrentMethod(),
                    exception);
            }
            finally
            {
                // Remove source file in case of moving is enabled but not in 
                // case of an unsuccessful termination (!error && !cancel).
                this.DeleteSource(this.Entry.Source);

                // Remove target file but only in case of an unsuccessful 
                // termination (error || cancel).
                this.DeleteTarget(this.Entry.Target);
            }
        }

        #endregion

        #region Private properties

        public Boolean IsAbort
        {
            get
            {
                return this.IsError || this.IsCancel;
            }
        }

        #endregion

        #region File operations

        private UInt32 GetMinimumBufferLength(String sourceFile, String targetFile)
        {
            VolumeInfo sourceInfo = new VolumeInfo(sourceFile);
            VolumeInfo targetInfo = new VolumeInfo(targetFile);

            if (sourceInfo.PageSize == targetInfo.PageSize)
            {
                return sourceInfo.PageSize;
            }

            if (sourceInfo.BytesPerSector == targetInfo.BytesPerSector)
            {
                return sourceInfo.BytesPerSector;
            }

            return 512; // Fallback: This is (should be) the minimum sector size of each hard drive.
        }

        /// <summary>
        /// Checks if <paramref name="sourceFile"/> and <paramref name="targetFile"/> 
        /// are different.
        /// </summary>
        /// <param name="sourceFile">
        /// The source file to check.
        /// </param>
        /// <param name="targetFile">
        /// The target file to check.
        /// </param>
        /// <returns>
        /// True, if both paths are different and false if not.
        /// </returns>
        private Boolean CheckDiffer(String sourceFile, String targetFile)
        {
            if (String.Compare(sourceFile, targetFile, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.IsError = true;
                this.logger.Warning(
                    MethodBase.GetCurrentMethod(),
                    "Unable to process because of source and target are identical.",
                    this.GetSourceFileDetail(sourceFile),
                    this.GetTargetFileDetail(targetFile));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the <paramref name="sourceFile"/> file (not empty, exists, is 
        /// not folder, etc.).
        /// </summary>
        /// <param name="sourceFile">
        /// The source file to check.
        /// </param>
        /// <returns>
        /// True, if successful and false otherwise.
        /// </returns>
        private Boolean CheckSource(String sourceFile)
        {
            if (String.IsNullOrWhiteSpace(sourceFile))
            {
                this.IsError = true;
                this.logger.Error(
                    MethodBase.GetCurrentMethod(),
                    "Source file name is invalid.");
                return false;
            }

            try
            {
                FileInfo nominee = new FileInfo(sourceFile);

                if (!nominee.Exists)
                {
                    this.IsError = true;
                    this.logger.Error(
                        MethodBase.GetCurrentMethod(),
                        "Source file does not exist.",
                        this.GetSourceFileDetail(sourceFile));
                    return false;
                }

                if ((nominee.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    this.IsError = true;
                    this.logger.Error(
                        MethodBase.GetCurrentMethod(),
                        "Source path is a directory.",
                        this.GetSourceFileDetail(sourceFile));
                    return false;
                }

                // TODO: Check other unsupported file attributes!

                return true;
            }
            catch (Exception exception)
            {
                this.IsError = true;
                this.logger.Critical(
                    MethodBase.GetCurrentMethod(),
                    exception,
                    this.GetSourceFileDetail(sourceFile));
                return false;
            }
        }

        /// <summary>
        /// Validates the target file (not empty) and creates the folder structure 
        /// if necessary. Additionally, all existing file attributes are released.
        /// </summary>
        /// <param name="targetFile">
        /// The target file to check.
        /// </param>
        /// <returns>
        /// True, if successful and false otherwise.
        /// </returns>
        private Boolean CheckTarget(String targetFile)
        {
            if (String.IsNullOrWhiteSpace(targetFile))
            {
                this.IsError = true;
                this.logger.Error(
                    MethodBase.GetCurrentMethod(),
                    "Target file name is invalid.");
                return false;
            }

            try
            {
                FileInfo nominee = new FileInfo(targetFile);

                DirectoryInfo folder = nominee.Directory;

                if (!folder.Exists)
                {
                    folder.Create();
                }

                if (nominee.Exists)
                {
                    nominee.Attributes = FileAttributes.Normal; // Release all attributes.
                }

                return true;
            }
            catch (Exception exception)
            {
                this.IsError = true;
                this.logger.Critical(
                    MethodBase.GetCurrentMethod(),
                    exception,
                    this.GetTargetFileDetail(targetFile));
                return false;
            }
        }

        private void CopyFiles(String sourceFile, String targetFile, UInt32 bufferLength, Boolean overwrite, IncrementalHash sourceHash)
        {
            if (this.IsAbort) { return; }

            // Ensure the target file exists as requested and initially takes the same length as the source file has.
            AccessHandler.Create(targetFile, overwrite, AccessHandler.GetLength(sourceFile));

            using (AccessHandler reader = new AccessHandler(this.logger))
            using (AccessHandler writer = new AccessHandler(this.logger))
            {
                reader.OpenRead(sourceFile);
                writer.OpenWrite(targetFile);

                Byte[] buffer = new Byte[bufferLength];
                Int32 count = 0;
                Int32 total = 0;

                while (!this.IsAbort && reader.ReadChunk(buffer, out count))
                {
                    if (sourceHash != null)
                    {
                        sourceHash.AppendData(buffer, 0, count);
                    }

                    total += count;

                    if (this.IsAbort) { return; }

                    if (!writer.WriteChunk(buffer, count, out Int32 written))
                    {
                        this.IsError = true;
                        this.logger.Error(
                            MethodBase.GetCurrentMethod(),
                            "Buffer processing failure.",
                            this.GetDetail("source-length", count.ToSafeString(nameof(Byte))),
                            this.GetDetail("target-length", written.ToSafeString(nameof(Byte))));
                        return;
                    }
                }

                if (this.IsAbort) { return; }

                this.logger.Trace(
                    MethodBase.GetCurrentMethod(),
                    $"Processed file length: {total.ToSafeString(nameof(Byte))}.",
                    this.GetSourceFileDetail(sourceFile));
            }
        }

        private void VerifyFiles(String targetFile, UInt32 bufferLength, IncrementalHash sourceHash, IncrementalHash targetHash)
        {
            if (this.IsAbort) { return; }

            using (AccessHandler reader = new AccessHandler(this.logger))
            {
                reader.OpenRead(targetFile);

                Byte[] buffer = new Byte[bufferLength];
                Int32 total = 0;

                while (!this.IsAbort && reader.ReadChunk(buffer, out Int32 length))
                {
                    targetHash.AppendData(buffer, 0, length);

                    total += length;
                }

                if (this.IsAbort) { return; }

                String sourceResult = sourceHash.GetHashAndReset().ToSafeHexString();
                String targetResult = targetHash.GetHashAndReset().ToSafeHexString();

                if (String.Compare(sourceResult, targetResult) != 0)
                {
                    this.IsError = true;
                    this.logger.Error(
                        MethodBase.GetCurrentMethod(),
                        "File verification mismatch.",
                        this.GetSourceHashDetail(sourceResult),
                        this.GetTargetHashDetail(targetResult));
                    return;
                }

                if (this.IsAbort) { return; }

                this.logger.Trace(
                    MethodBase.GetCurrentMethod(),
                    $"Verified file length: {total.ToSafeString(nameof(Byte))}.",
                    this.GetTargetHashDetail(targetResult),
                    this.GetTargetFileDetail(targetFile));
            }
        }

        private void ApplyDetails(String source, String target)
        {
            try
            {
                if (this.IsAbort) { return; }

                // BUG: Applying target file attributes may cause trouble under some circumstances.
                //      But the reason why couldn't been figured out yet. For the moment, flushing 
                //      and closing a file (see class AccessHandler) throws and catches an exception 
                //      with last Win32 error code, in the hope that this exception shows the reason.

                FileInfo from = new FileInfo(source);

                new FileInfo(target)
                {
                    CreationTime = from.CreationTime,
                    LastWriteTime = from.LastWriteTime,
                    LastAccessTime = from.LastAccessTime,
                    Attributes = from.Attributes
                };
            }
            catch (Exception exception)
            {
                this.IsError = true;
                this.logger.Error(
                    MethodBase.GetCurrentMethod(),
                    exception,
                    this.GetSourceFileDetail(source),
                    this.GetTargetFileDetail(target));
            }
        }

        /// <summary>
        /// Tries to delete provided file, but only if no error has occurred 
        /// and operation was not canceled and file file moving is enabled.
        /// </summary>
        /// <param name="source">
        /// The fully qualified name of the file to delete.
        /// </param>
        private void DeleteSource(String source)
        {
            try
            {
                // Don't do anything in case of abort state.
                if (this.IsAbort) { return; }

                if (!this.Entry.IsMove) { return; }

                if (File.Exists(source))
                {
                    this.logger.Trace(
                        MethodBase.GetCurrentMethod(),
                        $"Deleting file \"{source}\".");

                    File.SetAttributes(source, FileAttributes.Normal);
                    File.Delete(source);
                }
            }
            catch (Exception exception)
            {
                // Intentionally, do not touch current error state!
                this.logger.Fatal(
                    MethodBase.GetCurrentMethod(),
                    exception,
                    this.GetSourceFileDetail(source));
            }
        }

        /// <summary>
        /// Tries to delete provided file, but only if an error has 
        /// occurred or the operation has been canceled at all.
        /// </summary>
        /// <param name="target">
        /// The fully qualified name of the file to delete.
        /// </param>
        private void DeleteTarget(String target)
        {
            try
            {
                // Don't do anything in case of success state.
                if (!this.IsAbort) { return; }

                if (File.Exists(target))
                {
                    this.logger.Trace(
                        MethodBase.GetCurrentMethod(),
                        $"Deleting file \"{target}\".");

                    File.SetAttributes(target, System.IO.FileAttributes.Archive);
                    File.Delete(target);
                }
            }
            catch (Exception exception)
            {
                // Intentionally, do not touch current error state!
                this.logger.Fatal(
                    MethodBase.GetCurrentMethod(),
                    exception,
                    this.GetTargetFileDetail(target));
            }
        }

        #endregion

        #region Logging helpers

        private (String Label, Object Value) GetDetail(String label, String value)
        {
            return (Label: label, Value: value);
        }

        private (String Label, Object Value)[] GetExecutionDetails()
        {
            return new (String Label, Object Value)[] {
                this.GetSourceSizeDetail(this.Entry.Source),
                this.GetSourceFileDetail(this.Entry.Source),
                this.GetTargetFileDetail(this.Entry.Target)
            };
        }

        private (String Label, Object Value) GetSourceSizeDetail(String filename)
        {
            return this.GetDetail("source-size", $"\"{new FileInfo(filename).Length.ToSafeString(nameof(Byte))}\"");
        }

        private (String Label, Object Value) GetSourceFileDetail(String filename)
        {
            return this.GetDetail("source-file", $"\"{filename}\"");
        }

        private (String Label, Object Value) GetTargetFileDetail(String filename)
        {
            return this.GetDetail("target-file", $"\"{filename}\"");
        }

        private (String Label, Object Value) GetSourceHashDetail(String value)
        {
            return this.GetDetail("source-hash", $"\"{value}\"");
        }

        private (String Label, Object Value) GetTargetHashDetail(String value)
        {
            return this.GetDetail("target-hash", $"\"{value}\"");
        }

        #endregion
    }
}
