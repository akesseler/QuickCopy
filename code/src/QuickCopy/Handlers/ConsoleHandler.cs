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
using Plexdata.QuickCopy.Events;
using System;
using System.Runtime.InteropServices;

namespace Plexdata.QuickCopy.Handlers
{
    // SEE: https://social.msdn.microsoft.com/Forums/vstudio/en-US/707e9ae1-a53f-4918-8ac4-62a1eddb3c4a/detecting-console-application-exit-in-c?forum=csharpgeneral
    // For the howto of catching closing events!

    internal class ConsoleHandler : IDisposable
    {
        #region Public events

        public event ConsoleHandlerEventDelegate ConsoleEvent;

        #endregion

        #region Private fields

        private ConsoleEventDelegate handler = null;

        private readonly ILogger logger = null;

        #endregion

        #region Construction

        public ConsoleHandler(ILogger logger)
            : base()
        {
            this.logger = logger ?? throw new ArgumentException("The logger must not be null.");
            this.handler = new ConsoleEventDelegate(this.ConsoleEventCallback);

            if (!ConsoleHandler.SetConsoleCtrlHandler(handler, true))
            {
                this.handler = null;
            }
        }

        ~ConsoleHandler()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public properties

        public Boolean IsDisposed { get; private set; }

        #endregion

        #region Public methods

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected methods

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing) { /* Dispose managed resources. */ }

                if (this.handler != null)
                {
                    ConsoleHandler.SetConsoleCtrlHandler(handler, false);
                    this.handler = null;
                }

                this.IsDisposed = true;
            }
        }

        #endregion

        #region Private methods

        private Boolean ConsoleEventCallback(ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.CTRL_C_EVENT:

                    this.logger.Verbose("CTRL+C received!");
                    this.ConsoleEvent?.Invoke(this, new ConsoleHandlerEventArgs(ConsoleEventType.AbortEvent));
                    break;

                case ControlType.CTRL_BREAK_EVENT:

                    this.logger.Verbose("CTRL+BREAK received!");
                    this.ConsoleEvent?.Invoke(this, new ConsoleHandlerEventArgs(ConsoleEventType.BreakEvent));
                    break;

                case ControlType.CTRL_CLOSE_EVENT:

                    this.logger.Verbose("Program is being closed!");
                    this.ConsoleEvent?.Invoke(this, new ConsoleHandlerEventArgs(ConsoleEventType.CloseEvent));
                    break;

                case ControlType.CTRL_LOGOFF_EVENT:

                    this.logger.Verbose("User is logging off!");
                    this.ConsoleEvent?.Invoke(this, new ConsoleHandlerEventArgs(ConsoleEventType.LogoffEvent));
                    break;

                case ControlType.CTRL_SHUTDOWN_EVENT:
                    this.logger.Verbose("System is shutting down!");
                    this.ConsoleEvent?.Invoke(this, new ConsoleHandlerEventArgs(ConsoleEventType.ShutdownEvent));
                    break;

            }

            return true;
        }

        #endregion

        #region Native access

        private delegate Boolean ConsoleEventDelegate(ControlType controlType);

        private enum ControlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_RESERVED_3 = 3,
            CTRL_RESERVED_4 = 4,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Boolean SetConsoleCtrlHandler(ConsoleEventDelegate handler, Boolean add);

        #endregion
    }
}
