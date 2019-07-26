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

using Plexdata.ArgumentParser.Attributes;
using Plexdata.ArgumentParser.Constants;
using Plexdata.QuickCopy.Extensions;
using System;
using System.Text;

namespace Plexdata.QuickCopy.Models
{
    [HelpLicense]
    [HelpUtilize]
    [HelpPreface("<description>")]
    [ParametersGroup]
    public class Arguments
    {
        #region Construction

        public Arguments()
        {
            this.Source = String.Empty;
            this.Target = String.Empty;
            this.Pattern = "*.*";
            this.IsMove = false;
            this.IsVerify = false;
            this.IsOverwrite = false;
            this.IsRecursive = false;
            this.IsConsole = false;
            this.IsVersion = false;
            this.IsHelp = false;
            this.Files = new String[0];
        }

        #endregion

        #region Public properties

        [HelpSummary("The source folder path. This parameter optionally depends on parameter \"pattern\".")]
        [OptionParameter(SolidLabel = "source", BriefLabel = "s", DependencyList = "Pattern", DependencyType = DependencyType.Optional)]
        public String Source { get; set; }

        [HelpSummary("The target folder path.")]
        [OptionParameter(SolidLabel = "target", BriefLabel = "t")]
        public String Target { get; set; }

        [HelpSummary("Search pattern, such as \"*.*\". All files are used by default.")]
        [OptionParameter(SolidLabel = "pattern", BriefLabel = "p")]
        public String Pattern { get; set; }

        [HelpSummary("Move all files instead of copying. Default is OFF.")]
        [SwitchParameter(SolidLabel = "move", BriefLabel = "m")]
        public Boolean IsMove { get; set; }

        [HelpSummary("Perform target file verification. Default is OFF.")]
        [SwitchParameter(SolidLabel = "verify", BriefLabel = "v")]
        public Boolean IsVerify { get; set; }

        [HelpSummary("Overwrite existing files at target folder. Default is OFF.")]
        [SwitchParameter(SolidLabel = "overwrite", BriefLabel = "o")]
        public Boolean IsOverwrite { get; set; }

        [HelpSummary("Process source folder recursively. Default is OFF.")]
        [SwitchParameter(SolidLabel = "recursive", BriefLabel = "r")]
        public Boolean IsRecursive { get; set; }

        [HelpSummary("Redirect all logging messages onto the console window instead of writing them into the log-file. Default is OFF.")]
        [SwitchParameter(SolidLabel = "console", BriefLabel = "c")]
        public Boolean IsConsole { get; set; }

        [HelpSummary("Print program version and other attributes. This argument cannot be used together with other options.")]
        [SwitchParameter(SolidLabel = "version", IsExclusive = true)]
        public Boolean IsVersion { get; set; }

        [HelpSummary("Print current program settings. This argument cannot be used together with other options.")]
        [SwitchParameter(SolidLabel = "settings", IsExclusive = true)]
        public Boolean IsSettings { get; set; }

        [HelpSummary("Print this little help screen.")]
        [SwitchParameter(SolidLabel = "help", BriefLabel = "?")]
        public Boolean IsHelp { get; set; }

        [HelpSummary("Provide a distinct list of source files. This source file list cannot be used together with parameter \"source\".", Options = "<files>")]
        [VerbalParameter]
        public String[] Files { get; set; }

        #endregion

        #region Public methods

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(512);

            builder.Append($"{nameof(this.Source)}=[{this.Source.ToSafeString()}], ");
            builder.Append($"{nameof(this.Target)}=[{this.Target.ToSafeString()}], ");
            builder.Append($"{nameof(this.Pattern)}=[{this.Pattern.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsMove)}=[{this.IsMove.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsVerify)}=[{this.IsVerify.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsOverwrite)}=[{this.IsOverwrite.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsRecursive)}=[{this.IsRecursive.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsConsole)}=[{this.IsConsole.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsVersion)}=[{this.IsVersion.ToSafeString()}], ");
            builder.Append($"{nameof(this.IsHelp)}=[{this.IsHelp.ToSafeString()}], ");
            builder.Append($"{nameof(this.Files)}=[{this.Files.ToSafeString()}]");

            return builder.ToString();
        }

        #endregion
    }
}

