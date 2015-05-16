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
// <project>Battleships Pirate Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
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

/// <summary>
/// Splash screen for the program.
/// </summary>
public partial class SplashScreen : Battleships.DoubleBufferedForm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen" /> class.
        /// </summary>
        public SplashScreen()
        {
            // Start with an invisible window.
            this.InitializeComponent();
            this.Visible = false;
            this.Opacity = 0;
        }

        /// <summary>
        /// Fade the form in slowly and play a sound.
        /// </summary>
        public void ShowForm()
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("yaarrr.wav");
            double i;
            this.Opacity = 0.1;
            this.Visible = true;

            // Amount to increase opacity each iteration.
            i = 0.03;

            // Loop until Opacity = 1 (100%).
            while (this.Opacity < 1)
            {
                System.Threading.Thread.Sleep(25);

                // Increase opacity 
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
