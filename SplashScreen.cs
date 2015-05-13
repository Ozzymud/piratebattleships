using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DoubleBufferedUserControls;

namespace Battleships
{
    public partial class SplashScreen : Form_DoubleBuffered
    {
        public SplashScreen()
        {
            InitializeComponent();
            this.Visible = false;
            this.Opacity = 0;
        }

        /// <summary>
        /// Blendet die Form langsam ein
        /// </summary>
        public void showForm()
        {
            double i;

            this.Opacity = 0.1;
            this.Visible = true;

            // Intervall
            i = 0.03;
            // Sound asynchron laden und wiedergeben
            SoundClass soundPlayer = new SoundClass();
            soundPlayer.playSoundAsync(soundPlayer.currentSoundDir + "\\yaarrr.wav");

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
