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
using Plexdata.LogWriter.Definitions;
using Plexdata.LogWriter.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace Plexdata.QuickCopy.Timers
{
    internal abstract class LoggingExecutionTimer : IDisposable
    {
        #region Private fields

        private readonly ILogger logger;

        private readonly Stopwatch stopwatch;

        #endregion

        #region Construction

        public LoggingExecutionTimer(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentException("The logger must not be null."); ;
            this.stopwatch = new Stopwatch();

            // TODO: Remove workaround for bug in composite logger.
            if (Program.IsEnabled(LogLevel.Verbose))
            //if (this.logger.IsEnabled(LogLevel.Verbose))
            {
                this.logger.Verbose(this.GetLaunchMessage());
                this.stopwatch.Start();
            }
        }

        ~LoggingExecutionTimer()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public properties

        public Boolean IsDisposed { get; private set; }

        #endregion

        #region Protected properties

        protected (String Label, Object Value)[] Arguments { get; set; }

        #endregion

        #region Public methods

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected methods

        protected abstract String GetLaunchMessage();

        protected abstract String GetFinishMessage(TimeSpan elapsed);

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // TODO: Remove workaround for bug in composite logger.
                    if (Program.IsEnabled(LogLevel.Verbose))
                    //if (this.logger.IsEnabled(LogLevel.Verbose))
                    {
                        this.stopwatch.Stop();

                        if (this.Arguments != null && this.Arguments.Any())
                        {
                            this.logger.Verbose(this.GetFinishMessage(this.stopwatch.Elapsed), this.Arguments);
                        }
                        else
                        {
                            this.logger.Verbose(this.GetFinishMessage(this.stopwatch.Elapsed));
                        }
                    }
                }

                this.IsDisposed = true;
            }
        }

        #endregion
    }
}
