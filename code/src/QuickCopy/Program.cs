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
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THEs
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Plexdata.ArgumentParser.Extensions;
using Plexdata.LogWriter.Abstraction;
using Plexdata.LogWriter.Extensions;
using Plexdata.LogWriter.Logging;
using Plexdata.LogWriter.Logging.Standard;
using Plexdata.LogWriter.Settings;
using Plexdata.QuickCopy.Builders;
using Plexdata.QuickCopy.Events;
using Plexdata.QuickCopy.Extensions;
using Plexdata.QuickCopy.Handlers;
using Plexdata.QuickCopy.Helpers;
using Plexdata.QuickCopy.Models;
using Plexdata.QuickCopy.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Plexdata.QuickCopy
{
    class Program
    {
        private ILogger logger = null;
        private ConsoleHandler handler;
        private readonly Arguments arguments = null;

        private readonly CancellationTokenSource cancellation = null;

        [STAThread]
        internal static Int32 Main(String[] options)
        {
            Program instance = new Program();
            return instance.Run(options);
        }

        #region Construction

        private Program()
            : base()
        {
            this.arguments = new Arguments();
            this.cancellation = new CancellationTokenSource();
        }

        ~Program()
        {
            this.cancellation.Dispose();

            if (this.handler != null)
            {
                this.handler.Dispose();
                this.handler = null;
            }
        }

        #endregion

        #region Internal methods

        internal Int32 Run(String[] options)
        {
            if (!this.TryParse(options))
            {
                this.ShowHelp();
                return -1;
            }

            this.ApplyLogger();

            if (this.arguments.IsHelp)
            {
                this.ShowHelp();
                return 0;
            }

            if (this.arguments.IsVersion)
            {
                this.ShowInfo();
                return 0;
            }

            this.handler = new ConsoleHandler(this.logger);
            this.handler.ConsoleEvent += this.OnHandlerConsoleEvent;

            // Track command line arguments.
            this.logger.Verbose(
                MethodBase.GetCurrentMethod(),
                this.arguments.ToString());

            try
            {
                this.arguments.ValidateSourceOrThrow();
                this.arguments.ValidateTargetOrThrow();
            }
            catch (Exception exception)
            {
                this.logger.Critical(
                    MethodBase.GetCurrentMethod(),
                    exception);

                this.HitAnyKey();
                return -1;
            }

            this.HitAnyKey();

            this.Process();

            this.HitAnyKey();

            return 0;
        }

        #endregion

        #region Private methods

        private void Process()
        {
            using (new ProgramProcessExecutionTimer(this.logger))
            {
                try
                {
                    Int32 cores = Environment.ProcessorCount;
                    CancellationToken token = this.cancellation.Token;

                    PlaylistBuilder builder = new PlaylistBuilder(this.logger, token);

                    ParallelOptions options = new ParallelOptions
                    {
                        CancellationToken = token,
                        MaxDegreeOfParallelism = cores
                    };

                    List<PlaylistEntryHandler> playlist = builder.Build(this.arguments);

                    this.logger.Verbose(
                        MethodBase.GetCurrentMethod(),
                        $"Playlist contains {playlist.Count} entries to process.");

                    List<PlaylistEntryHandler> handlers = new List<PlaylistEntryHandler>();

                    while (playlist.Count > 0)
                    {
                        handlers.Clear();

                        while (handlers.Count < cores && playlist.Count > 0)
                        {
                            handlers.Add(playlist[0]);
                            playlist.RemoveAt(0);
                        }

                        Parallel.ForEach(handlers, options, (handler) =>
                        {
                            handler.Execute();
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    this.logger.Warning(
                        MethodBase.GetCurrentMethod(),
                        "Operation of file processing canceled.");
                }
                catch (Exception exception)
                {
                    this.logger.Critical(
                        MethodBase.GetCurrentMethod(),
                        exception);
                }
            }
        }

        private Boolean TryParse(String[] options)
        {
            try
            {
                if (options != null && options.Length > 0)
                {
                    this.arguments.Process(options);
                    this.arguments.Validate();
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                ILogger helper = this.logger;

                if (helper == null)
                {
                    try
                    {
                        // This is a fallback to handle exception before the real logger exists. 
                        ILoggerSettingsBuilder builder = new LoggerSettingsBuilder();
                        builder.SetFilename(Path.Combine(Directory.GetCurrentDirectory(), "logsettings.json"));
                        helper = new PersistentLogger(new PersistentLoggerSettings(builder.Build()));
                    }
                    catch { }
                }

                helper.Critical(
                    MethodBase.GetCurrentMethod(),
                    exception);
            }

            return false;
        }

        private void ApplyLogger()
        {
            ILoggerSettingsBuilder builder = new LoggerSettingsBuilder();
            builder.SetFilename(Path.Combine(Directory.GetCurrentDirectory(), "logsettings.json"));

            if (this.arguments.IsConsole)
            {
                this.logger = new ConsoleLogger(new ConsoleLoggerSettings(builder.Build()));
            }
            else
            {
                this.logger = new PersistentLogger(new PersistentLoggerSettings(builder.Build()));
            }
        }

        private void ShowHelp()
        {
            try
            {
                Console.WriteLine(this.arguments.Generate());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            this.HitAnyKey();
        }

        private void ShowInfo()
        {
            Console.WriteLine($"{nameof(AssemblyAttributes.Title)}:       {AssemblyAttributes.Title}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Version)}:     {AssemblyAttributes.Version}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Company)}:     {AssemblyAttributes.Company}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Copyright)}:   {AssemblyAttributes.Copyright}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Description)}: {AssemblyAttributes.Description}");

            this.HitAnyKey();
        }

        [Conditional("DEBUG")]
        private void HitAnyKey()
        {
            Console.Write("{0}Hit any key to continue... ", Environment.NewLine);
            Console.ReadKey();
            Console.WriteLine();
        }

        private void OnHandlerConsoleEvent(Object sender, ConsoleHandlerEventArgs args)
        {
            this.cancellation.Cancel();
        }

        #endregion
    }
}
