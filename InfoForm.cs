//-----------------------------------------------------------------------
// <copyright file="InfoForm.cs" company="Team 17">
// Copyright 2005 Team 17
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <project>Battleships Pirate Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
//-----------------------------------------------------------------------

namespace Battleships
{
#region directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
#endregion

/// <summary>
/// The help about window.
/// </summary>
public partial class InfoForm : Battleships.DoubleBufferedForm
    {
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="InfoForm" /> class. 
    /// </summary>
    public InfoForm()
    {
        this.InitializeComponent();
        this.Text = string.Format(CultureInfo.InvariantCulture, "About {0}", this.AssemblyTitle);
        this.labelProductName.Text = this.AssemblyProduct;
        this.labelVersion.Text = string.Format(CultureInfo.InvariantCulture, "Version {0}", this.AssemblyVersion);
        this.labelCopyright.Text = this.AssemblyCopyright;
        this.labelCompanyName.Text = this.AssemblyCompany;
        this.textBoxDescription.Text = this.AssemblyDescription;
    }
    #endregion

    #region Assembly attribute accessors

    /// <summary>
    /// Gets and returns the long program name stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyTitle
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                {
                    return titleAttribute.Title;
                }
            }

            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }
    }

    /// <summary>
    /// Gets and returns the version of the program stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyVersion
    {
        get
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }

    /// <summary>
    /// Gets and returns the description of the program stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyDescription
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    /// <summary>
    /// Gets and returns the short product name stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyProduct
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyProductAttribute)attributes[0]).Product;
        }
    }

    /// <summary>
    /// Gets and returns the copyright info stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyCopyright
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    /// <summary>
    /// Gets and returns the company name stored in AssemblyInfo.cs
    /// </summary>
    private string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    #endregion
    }
}
