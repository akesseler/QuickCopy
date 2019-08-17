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
using Plexdata.LogWriter.Definitions;
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
        #region Private fields

        private static Program instance = null;

        private ILogger logger = null;

        private ConsoleHandler handler = null;

        private readonly Arguments arguments = null;

        private readonly CancellationTokenSource cancellation = null;

        #endregion

        #region Entry point

        [STAThread]
        internal static Int32 Main(String[] options)
        {
            Program.instance = new Program();
            return Program.instance.Run();
        }

        #endregion

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

        #region Private methods

        private Int32 Run()
        {
            if (!this.TryParse())
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
                this.ShowVersion();
                return 0;
            }

            if (this.arguments.IsSettings)
            {
                this.ShowSettings();
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

        private Boolean TryParse()
        {
            try
            {
                List<String> options = new List<String>(Environment.CommandLine.Extract());

                if (options != null && options.Count > 1)
                {
                    // Remove name of executable...
                    options.RemoveAt(0);

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
                        helper = new PersistentLogger(new PersistentLoggerSettings(this.GetLoggerSettings()));
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
            ILoggerSettingsSection section = this.GetLoggerSettings();
            ICompositeLogger helper = new CompositeLogger(new CompositeLoggerSettings(section));

            helper.AddLogger(new ConsoleLogger(new ConsoleLoggerSettings(section)));
            helper.AddLogger(new PersistentLogger(new PersistentLoggerSettings(section)));

            this.logger = helper;
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

        private void ShowVersion()
        {
            Console.WriteLine($"{nameof(AssemblyAttributes.Title)}:       {AssemblyAttributes.Title}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Version)}:     {AssemblyAttributes.Version}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Platform)}:    {AssemblyAttributes.Platform}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Company)}:     {AssemblyAttributes.Company}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Copyright)}:   {AssemblyAttributes.Copyright}");
            Console.WriteLine($"{nameof(AssemblyAttributes.Description)}: {AssemblyAttributes.Description}");

            this.HitAnyKey();
        }

        private void ShowSettings()
        {
            IConsoleLoggerSettings cls = new ConsoleLoggerSettings(this.GetLoggerSettings());
            IPersistentLoggerSettings pls = new PersistentLoggerSettings(this.GetLoggerSettings());

            Console.WriteLine($"- Logging");
            Console.WriteLine($"  - General");
            Console.WriteLine($"    - {nameof(cls.LogLevel)}:    {cls.LogLevel.ToDisplayText()}");
            Console.WriteLine($"    - {nameof(cls.LogType)}:     {cls.LogType.ToString().ToLower()}");
            Console.WriteLine($"    - {nameof(cls.LogTime)}:     {cls.LogTime.ToString().ToLower()}");
            Console.WriteLine($"    - {nameof(cls.ShowTime)}:    {(cls.ShowTime ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(cls.TimeFormat)}:  {(cls.TimeFormat == null ? "<null>" : (String.IsNullOrWhiteSpace(cls.TimeFormat) ? "<empty>" : cls.TimeFormat))}");
            Console.WriteLine($"    - {nameof(cls.PartSplit)}:   {cls.PartSplit}");
            Console.WriteLine($"    - {nameof(cls.FullName)}:    {(cls.FullName ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(cls.Culture)}:     {(cls.Culture == null ? "<null>" : cls.Culture.Name)}");
            Console.WriteLine($"  - Console");
            Console.WriteLine($"    - {nameof(cls.UseColors)}:   {(cls.UseColors ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(cls.WindowTitle)}: {(cls.WindowTitle == null ? "<null>" : (String.IsNullOrWhiteSpace(cls.WindowTitle) ? "<empty>" : cls.WindowTitle))}");
            Console.WriteLine($"    - {nameof(cls.QuickEdit)}:   {(cls.QuickEdit ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(cls.BufferSize)}:  {(cls.BufferSize == null ? "<null>" : $"{nameof(cls.BufferSize.Width)}={cls.BufferSize.Width.ToSafeString()}, {nameof(cls.BufferSize.Lines)}={cls.BufferSize.Lines.ToSafeString()}")}");
            Console.WriteLine($"    - {nameof(cls.Coloring)}:    {cls.Coloring.Count}");

            foreach (KeyValuePair<LogLevel, Coloring> value in cls.Coloring)
            {
                Console.WriteLine($"                   {value.Key.ToDisplayText().PadRight(8, ' ')} : {value.Value.Foreground}, {value.Value.Background}");
            }

            Console.WriteLine($"  - Persistent");
            Console.WriteLine($"    - {nameof(pls.Filename)}:    {(pls.Filename == null ? "<null>" : (String.IsNullOrWhiteSpace(pls.Filename) ? "<empty>" : pls.Filename))}");
            Console.WriteLine($"    - {nameof(pls.IsRolling)}:   {(pls.IsRolling ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(pls.IsQueuing)}:   {(pls.IsQueuing ? "yes" : "no")}");
            Console.WriteLine($"    - {nameof(pls.Threshold)}:   {pls.Threshold.ToSafeString("KiB")}");
            Console.WriteLine($"    - {nameof(pls.Encoding)}:    {(pls.Encoding == null ? "<null>" : pls.Encoding.BodyName)}");

            this.HitAnyKey();
        }

        private ILoggerSettingsSection GetLoggerSettings()
        {
            ILoggerSettingsBuilder builder = new LoggerSettingsBuilder();
            builder.SetFilename(Path.Combine(Directory.GetCurrentDirectory(), "logsettings.json"));
            return builder.Build();
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
