//-----------------------------------------------------------------------
// <copyright file="DoubleBuffered.cs" company="Ozzymud">
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

namespace DoubleBuffered
{
using System.Drawing;
using System.Windows.Forms;

public class FormDoubleBuffered : Form
        {
        public FormDoubleBuffered()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      this.UpdateStyles();
    }
  }

    public class PanelDoubleBuffered : Panel
    {
        public PanelDoubleBuffered()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      this.UpdateStyles();
    }
  }

    public class PictureBoxDoubleBuffered : PictureBox
    {
        public PictureBoxDoubleBuffered()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
      this.UpdateStyles();
    }
  }

    public class TransparentListBox : ListBox
    {
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams createParams = base.CreateParams;
        createParams.ExStyle |= 32;
        return createParams;
      }
    }

    public TransparentListBox()
    {
      this.SetStyle(ControlStyles.Opaque | ControlStyles.SupportsTransparentBackColor, true);
      this.BackColor = Color.Transparent;
    }
  }
}