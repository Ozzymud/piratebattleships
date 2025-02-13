﻿//-----------------------------------------------------------------------
// <copyright file="DoubleBufferedPanel.cs" company="Ozzymud">
// Copyright 2015 Ozzymud
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
// <author>Ozzymud</author>
//-----------------------------------------------------------------------

namespace Battleships
{
#region directives
using System.Drawing;
using System.Windows.Forms;
#endregion

/// <summary>
/// Sets control styles to use double buffering.
/// </summary>
public class DoubleBufferedPanel : Panel
    {
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleBufferedPanel" /> class.
    /// </summary>
    public DoubleBufferedPanel()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      this.UpdateStyles();
    }
    #endregion constructor
  }
}