//-----------------------------------------------------------------------
// <copyright file="BattlefieldOpponent.cs" company="Team 17">
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
// <project>Schiffeversenken Pirat Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
//-----------------------------------------------------------------------

namespace Battleships
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class BattlefieldOpponent : DoubleBuffered.PanelDoubleBuffered
    {
        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        public DoubleBuffered.PanelDoubleBuffered[,] pb = new DoubleBuffered.PanelDoubleBuffered[10, 10];

        private delegate void AddControlCallback(Control contr);

        delegate void showDestroyedShipsCallback(int[] args, bool horizontal);

        // private System.IO.MemoryStream ms = new System.IO.MemoryStream(Battleships.Properties.Resources.MyCursor);

        /// <summary>
        /// Positionsfarbe (Wenn mit Maus über Feld gefahren wird)
        /// </summary>
        private Color positionColor;

        public BattlefieldOpponent(int x, int y)
        {
            this.positionColor = new Color();
            this.positionColor = Color.FromArgb(120, 30, 151, 255); // Ein helles Blau
            // Cursor aus PNG-File laden
            // Edit: use embedded resource
            //// Bitmap bitmap = new Bitmap(System.IO.Directory.GetCurrentDirectory() + "\\aim.png");
            Bitmap bitmap = new Bitmap(Battleships.Properties.Resources.aim);

            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //// this.BackgroundImageLayout = ImageLayout.Stretch;
            //// this.BackgroundImage = Battleships.Properties.Resources.meer_opponent;
            this.BackColor = Color.Transparent;
            //// this.BorderStyle = BorderStyle.FixedSingle;
            //// this.BorderStyle = BorderStyle.None;

            // Matrix Gegner
            for (int i = 0; i < this.pb.GetLength(0); i++)
            {
                for (int j = 0; j < this.pb.GetLength(1); j++)
                {
                    DoubleBuffered.PanelDoubleBuffered p = new DoubleBuffered.PanelDoubleBuffered();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.MouseClick += new MouseEventHandler(this.p_Clicked);
                    p.MouseEnter += new EventHandler(this.p_MouseEnter);
                    p.MouseLeave += new EventHandler(this.p_MouseLeave);
                    p.Cursor = CreateCursor(bitmap, 16, 16);
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.pb[i, j] = p;
                    this.Controls.Add(p);
                }
            }
        }

        /// <summary>
        /// Zeigt auf dem Gegnerspielfeld ein zerstörtes Boot an
        /// </summary>
        /// <param name="args">Enthält die Coordinaten des Schiffes</param>
        /// <param name="horizontal">Gibt an ob das Schiff horizontal oder vertikal gesetzt wurde</param>
        public void showDestroyedBoat(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                showDestroyedShipsCallback d = new showDestroyedShipsCallback(this.showDestroyedBoat);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                BattleshipsForm.soundPlayer.PlaySoundAsync("explosion1.wav");

                // Explosionsbild an der angegeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                if (horizontal)
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h2;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                }
                else
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                }
            }
        }

        /// <summary>
        ///  Displays on the opponent field a ruined cruiser
        /// </summary>
        /// <param name="args">the co-ordinates of the vessel</param>
        /// <param name="horizontal">Specifies whether the ship was placed horizontally or vertically</param>
        public void showDestroyedCruiser(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                showDestroyedShipsCallback d = new showDestroyedShipsCallback(this.showDestroyedCruiser);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                // TODO: Siehe showDestroyedBoat
                BattleshipsForm.soundPlayer.PlaySoundAsync("explosion1.wav");

                // Explosionsbild an der angegebeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.pb[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

                if (horizontal)
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_h1;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_h2;
                    this.pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_h3;
                }
                else
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_v1;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_v2;
                    this.pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_v3;
                }
            }
        }

        /// <summary>
        /// Setzt einen Treffer auf das angegebene Feld
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        public void setImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.soundPlayer.PlaySoundAsync("explosion2.wav");
                this.drawExplosion(x, y);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// sets a missed shot to the specified field
        /// </summary>
        /// <param name="x">x coordinate of the missed shot</param>
        /// <param name="y">y coordinate of the missed shot</param>
        public void setMiss(int x, int y)
        {
            BattleshipsForm.soundPlayer.PlaySoundAsync("splash.wav");
            this.drawMiss(x, y);
        }

        private void drawMiss(int x, int y)
        {
            PictureBox missPicture = new PictureBox();
            missPicture.Name = "miss_" + x.ToString() + ":" + y.ToString();
            missPicture.Location = new Point(x * 30, y * 30);
            missPicture.Size = new Size(30, 30);
            missPicture.Margin = new Padding(0);
            missPicture.Padding = new Padding(0);
            missPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            missPicture.BackColor = Color.Transparent;
            missPicture.Image = Properties.Resources.splash;

            this.addControl(missPicture);
        }

        /// <summary>
        /// Entscheidet anhand des bereits im Panel gespeicherten Bildes welche Explosion dargestellt werden soll
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        public void drawExplosion(int x, int y)
        {
            // PictureBoxDoubleBuffered explPicture = new PictureBoxDoubleBuffered();
            PictureBox explPicture = new PictureBox();
            explPicture.Name = "expl_" + x.ToString() + ":" + y.ToString();
            explPicture.Location = new Point(x * 30, y * 30);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explo6;
            
            // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
            this.addControl(explPicture);
        }
        
        public void addControl(Control contr)
        {
            if (this.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(this.addControl);
                this.Invoke(d, new object[] { contr });
            }
            else
            {
                this.Controls.Add(contr);
                Control[] s = this.Controls.Find(contr.Name, false);
                s[0].BringToFront();
            }
        }

        #region Mouse-Events
        public void p_Clicked(object sender, MouseEventArgs e)
        {
            // Erst loslegen, wenn beide Spieler bereit sind
            if (BattleshipsForm.opponendReady2Play && BattleshipsForm.playerReady2Play)
            {
                // Es darf nur geschossen werden, wenn man auch an der reihe ist!
                if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
                {
                    DoubleBuffered.PanelDoubleBuffered tmp = (DoubleBuffered.PanelDoubleBuffered)sender;
                    switch (e.Button)
                    {
                        case System.Windows.Forms.MouseButtons.Left:
                            // Bin ich einem Spiel beigetreten?
                            if (BattleshipsForm.clientGameForm != null)
                            {
                                if (BattleshipsForm.clientGameForm.m_clientSocket != null)
                                {
                                    try
                                    {
                                        object sendData = tmp.Name;
                                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                        // Koordinaten des aktuellen Feldes an Server schicken
                                        BattleshipsForm.clientGameForm.m_clientSocket.Send(byData);

                                        // Auf Antwort warten (HIT\MISS\WIN\LOSE)
                                        BattleshipsForm.clientGameForm.WaitForData();
                                    }
                                    catch (Exception ex)
                                    {
                                        BattleshipsForm.clientGameForm.SetText(ex.ToString());
                                    }
                                }
                            }
                            else if (BattleshipsForm.hostGameForm != null)
                            {
                            // Oder Hoste ich selber ein Spiel?
                                try
                                {
                                    if (BattleshipsForm.hostGameForm.m_workerSocket != null)
                                    {
                                        try
                                        {
                                            object sendData = tmp.Name;
                                            byte[] byData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                            // Koordinaten des aktuellen Feldes an Server schicken
                                            BattleshipsForm.hostGameForm.m_workerSocket.Send(byData);

                                            // Auf Antwort warten (HIT\MISS\WIN\LOSE)
                                            BattleshipsForm.hostGameForm.WaitForData(BattleshipsForm.hostGameForm.m_workerSocket);
                                        }
                                        catch (Exception ex)
                                        {
                                            BattleshipsForm.hostGameForm.SetText(ex.ToString());
                                            BattleshipsForm.hostGameForm.m_clientCount--;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            break;
                    }
                }
            }
        }

        public void p_MouseEnter(object sender, EventArgs e)
        {
            if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
            {
                DoubleBuffered.PanelDoubleBuffered tmp = (DoubleBuffered.PanelDoubleBuffered)sender;

                for (int i = 0; i < this.pb.GetLength(0); i++)
                {
                    for (int j = 0; j < this.pb.GetLength(1); j++)
                    {
                        if (tmp.Name.ToString() == this.pb[i, j].Name.ToString())
                        {
                            this.pb[i, j].BackColor = this.positionColor;
                        }
                    }
                }
            }
        }

        public void p_MouseLeave(object sender, EventArgs e)
        {
            if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
            {
                DoubleBuffered.PanelDoubleBuffered tmp = (DoubleBuffered.PanelDoubleBuffered)sender;

                for (int i = 0; i < this.pb.GetLength(0); i++)
                {
                    for (int j = 0; j < this.pb.GetLength(1); j++)
                    {
                        if ((int)this.pb[i, j].Tag != (int)1)
                        {
                            this.pb[i, j].BackColor = Color.Transparent;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates a cursor from a bitmap (PNG, JPEG is also good)
        /// </summary>
        /// <param name="bmp">The image file to be displayed as the cursor</param>
        /// <param name="xHotSpot">x value of the hotspot</param>
        /// <param name="yHotSpot">y value of the hotspot</param>
        /// <returns>the created cursor</returns>
        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            return new Cursor(CreateIconIndirect(ref tmp));
        }
    }
}