//-----------------------------------------------------------------------
// <copyright file="SplashScreen.cs" company="Team 17">
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
//-----------------------------------------------------------------------
namespace Battleships
{
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class SplashScreen : DoubleBuffered.FormDoubleBuffered
    {
        public SplashScreen()
        {
            this.InitializeComponent();
            this.Visible = false;
            this.Opacity = 0;
        }

        /// <summary>
        /// Blendet die Form langsam ein
        /// </summary>
        public void showForm()
        {
            // load and play sound
            BattleshipsForm.soundPlayer.PlaySoundAsync("yaarrr.wav");
            double i;

            this.Opacity = 0.1;
            this.Visible = true;

            // Intervall
            i = 0.03;

            // Solange durchlaufen bis Opacity = 1 (100%)
            while (this.Opacity < 1)
            {
                System.Threading.Thread.Sleep(25);

                // Opacity erhöhen
                this.Opacity += i;
                this.TopMost = true;
                Application.DoEvents();
            }

            this.TopMost = false;
            this.BringToFront();
            Application.DoEvents();
        }
    }
}
