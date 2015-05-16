//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Oz">
// Copyright 5/16/2015 Oz
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
// <author>Oz</author>
//-----------------------------------------------------------------------
namespace Battleships
{
    #region directives
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    #endregion

    /// <summary>
    /// Methods marked with $DllImport$.
    /// </summary>
    public class NativeMethods
        {
        #region private static
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref BattlefieldOpponent.IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref BattlefieldOpponent.IconInfo pIconInfo);
        #endregion
    }
}