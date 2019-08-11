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

using Plexdata.QuickCopy.Extensions;
using Plexdata.QuickCopy.Helpers;
using System;
using System.IO;
using System.Text;

namespace Plexdata.QuickCopy.Models
{
    internal class PlaylistEntry
    {
        #region Private fields

        private String source = String.Empty;

        private String origin = String.Empty;

        private String target = String.Empty;

        #endregion

        #region Construction

        public PlaylistEntry()
            : base()
        {
        }

        #endregion

        #region Public properties

        public String Source
        {
            get
            {
                return this.source;
            }
            set
            {
                this.source = FilePathAdjuster.ToLongPath(Path.GetFullPath(value));
            }
        }

        public String Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    this.origin = String.Empty;
                }
                else
                {
                    this.origin = FilePathAdjuster.ToLongPath(Path.GetFullPath(value));
                }
            }
        }

        public String Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = FilePathAdjuster.ToLongPath(Path.GetFullPath(value));
            }
        }

        public Boolean IsMove { get; set; }

        public Boolean IsVerify { get; set; }

        public Boolean IsOverwrite { get; set; }

        #endregion

        #region Public methods

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(256);

            builder.Append($"{nameof(this.IsMove)}=[{this.IsMove.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsVerify)}=[{this.IsVerify.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsOverwrite)}=[{this.IsOverwrite.ToSafeString()}], ");
            builder.Append($"{nameof(this.Target)}=[{this.Target.ToSafeString()}], ");
            builder.Append($"{nameof(this.Source)}=[{this.Source.ToSafeString()}], ");
            builder.Append($"{nameof(this.Origin)}=[{this.Origin.ToSafeString()}]");

            return builder.ToString();
        }

        #endregion
    }
}
