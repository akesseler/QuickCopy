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

using System;
using System.Reflection;

namespace Plexdata.QuickCopy.Helpers
{
    // See URL below for more information:
    // http://csharphelper.com/blog/2018/03/get-assembly-information-in-c/

    internal static class AssemblyAttributes
    {
        #region Private fields

        private static Assembly assembly = null;

        #endregion

        #region Construction

        static AssemblyAttributes()
        {
            AssemblyAttributes.assembly = Assembly.GetExecutingAssembly();
        }

        #endregion

        #region Public properties

        public static String Title
        {
            get
            {
                AssemblyTitleAttribute attribute = AssemblyAttributes.GetAssemblyAttribute<AssemblyTitleAttribute>();
                return attribute != null ? attribute.Title : String.Empty;
            }
        }

        public static String Version
        {
            get
            {
                AssemblyFileVersionAttribute attribute = AssemblyAttributes.GetAssemblyAttribute<AssemblyFileVersionAttribute>();
                return attribute != null ? attribute.Version : String.Empty;
            }
        }

        public static String Company
        {
            get
            {
                AssemblyCompanyAttribute attribute = AssemblyAttributes.GetAssemblyAttribute<AssemblyCompanyAttribute>();
                return attribute != null ? attribute.Company : String.Empty;
            }
        }

        public static String Copyright
        {
            get
            {
                AssemblyCopyrightAttribute attribute = AssemblyAttributes.GetAssemblyAttribute<AssemblyCopyrightAttribute>();
                return attribute != null ? attribute.Copyright : String.Empty;
            }
        }

        public static String Description
        {
            get
            {
                AssemblyDescriptionAttribute attribute = AssemblyAttributes.GetAssemblyAttribute<AssemblyDescriptionAttribute>();
                return attribute != null ? attribute.Description : String.Empty;
            }
        }

        #endregion

        #region Private properties

        private static TAttribute GetAssemblyAttribute<TAttribute>() where TAttribute : Attribute
        {
            Object[] attributes = AssemblyAttributes.assembly.GetCustomAttributes(typeof(TAttribute), true);

            if ((attributes == null) || (attributes.Length == 0))
            {
                return null;
            }

            return (TAttribute)attributes[0];
        }

        #endregion
    }
}
