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
using Plexdata.QuickCopy.Handlers;
using Plexdata.QuickCopy.Helpers;
using Plexdata.QuickCopy.Models;
using Plexdata.QuickCopy.Native;
using Plexdata.QuickCopy.Timers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Plexdata.QuickCopy.Builders
{
    internal class PlaylistBuilder
    {
        #region Private fields

        private readonly ILogger logger;

        private readonly CancellationToken token;

        #endregion

        #region Construction

        public PlaylistBuilder(ILogger logger, CancellationToken token)
            : base()
        {
            this.logger = logger ?? throw new ArgumentException("The logger must not be null.");
            this.token = token;
        }

        #endregion

        #region Public methods

        public List<PlaylistEntryHandler> Build(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentException("The base argument must not be null.");
            }

            List<PlaylistEntry> result = new List<PlaylistEntry>();

            using (new CollectingFilesExecutionTimer(this.logger))
            {
                if (arguments.Files != null && arguments.Files.Length > 0)
                {
                    foreach (String file in arguments.Files)
                    {
                        String source = Path.GetFullPath(file);
                        String origin = source;

                        source = this.Resolve(source, out Boolean resolved);

                        String target = Path.Combine(arguments.Target, Path.GetFileName(source));

                        result.Add(new PlaylistEntry()
                        {
                            Source = source,
                            Origin = resolved ? origin : String.Empty,
                            Target = target,
                            IsMove = arguments.IsMove,
                            IsVerify = arguments.IsVerify,
                            IsOverwrite = arguments.IsOverwrite,
                        });
                    }
                }
                else
                {
                    DirectoryInfo folder = new DirectoryInfo(FilePathAdjuster.ToLongPath(this.Resolve(arguments.Source)));
                    SearchOption option = arguments.IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    foreach (FileInfo file in folder.EnumerateFiles(arguments.Pattern, option))
                    {
                        // The new target filename must include the sub-path of the source folder.
                        String source = file.FullName;
                        String origin = source;

                        source = this.Resolve(source, out Boolean resolved);

                        String target = source.Replace(folder.FullName, String.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        result.Add(new PlaylistEntry()
                        {
                            Source = source,
                            Origin = resolved ? origin : String.Empty,
                            Target = Path.Combine(arguments.Target, target),
                            IsMove = arguments.IsMove,
                            IsVerify = arguments.IsVerify,
                            IsOverwrite = arguments.IsOverwrite,
                        });
                    }
                }
            }

            return result.Select(entry => new PlaylistEntryHandler(this.logger, entry, this.token)).ToList();
        }

        #endregion

        #region Private methods

        private String Resolve(String filename)
        {
            return this.Resolve(filename, out Boolean resolved);
        }

        private String Resolve(String filename, out Boolean resolved)
        {
            resolved = false;

            try
            {
                String result = ReparseResolver.Resolve(filename, out resolved);

                if (resolved)
                {
                    this.logger.Verbose(
                        MethodBase.GetCurrentMethod(),
                        $"Reparse point \"{filename}\" resolved to \"{result}\".");
                }

                return result;
            }
            catch (Exception exception)
            {
                this.logger.Warning(
                    MethodBase.GetCurrentMethod(),
                    $"Resolving reparse point \"{filename}\" has failed. Use original name instead.",
                    exception);

                return filename;
            }
        }

        #endregion
    }
}
