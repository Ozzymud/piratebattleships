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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public class BattlefieldOpponent : Battleships.DoubleBufferedPanel
    {
        private struct IconInfo
        {
            internal bool ParameterfIcon;
            internal int HotspotX;
            internal int HotspotY;
            private IntPtr hbmMask;
            private IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        private Battleships.DoubleBufferedPanel[,] pb = new Battleships.DoubleBufferedPanel[10, 10];

        private delegate void AddControlCallback(Control contr);

        private delegate void ShowDestroyedShipsCallback(int[] args, bool horizontal);

        //// private System.IO.MemoryStream ms = new System.IO.MemoryStream(Battleships.Properties.Resources.MyCursor);

        /// <summary>
        /// Position color (When moving mouse over box).
        /// </summary>
        private Color positionColor;

        public BattlefieldOpponent(int x, int y)
        {
            this.positionColor = new Color();
            this.positionColor = Color.FromArgb(120, 30, 151, 255); // Ein helles Blau
            // Get cursor from an embedded image
            Bitmap bitmap = new Bitmap(Battleships.Properties.Resources.aim);

            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //// this.BackgroundImageLayout = ImageLayout.Stretch;
            //// this.BackgroundImage = Battleships.Properties.Resources.sea_enemy;
            this.BackColor = Color.Transparent;
            //// this.BorderStyle = BorderStyle.FixedSingle;
            //// this.BorderStyle = BorderStyle.None;

            // enemies grid
            for (int i = 0; i < this.pb.GetLength(0); i++)
            {
                for (int j = 0; j < this.pb.GetLength(1); j++)
                {
                    Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.MouseClick += new MouseEventHandler(this.PlayerClicked);
                    p.MouseEnter += new EventHandler(this.PlayerMouseEnter);
                    p.MouseLeave += new EventHandler(this.PlayerMouseLeave);
                    p.Cursor = CreateCursor(bitmap, 16, 16);
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.pb[i, j] = p;
                    this.Controls.Add(p);
                }
            }
        }

        /// <summary>
        /// Displays a destroyed boat on the enemies playing field.
        /// </summary>
        /// <param name="args">Contains the coordinates of the vessel.</param>
        /// <param name="horizontal">Specifies whether the ship was used horizontally or vertically.</param>
        public void ShowDestroyedBoat(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedBoat);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // remove explosion picture at the specified position (remove--> PictureBox control)
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
        ///  Displays a ruined cruiser on the enemy field.
        /// </summary>
        /// <param name="args">The co-ordinates of the vessel.</param>
        /// <param name="horizontal">Specifies whether the ship was placed horizontally or vertically.</param>
        public void ShowDestroyedCruiser(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedCruiser);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                // TODO: show destroyed boat
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // At the entered point remove explosion image (remove --> PictureBox control)
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
        /// Sets a hit on the specified field.
        /// </summary>
        /// <param name="x">X Coordinate of the hit.</param>
        /// <param name="y">Y Coordinate of the hit.</param>
        public void SetImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion2.wav");
                this.DrawExplosion(x, y);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Sets a missed shot on the specified field.
        /// </summary>
        /// <param name="x">X coordinate of the missed shot.</param>
        /// <param name="y">Y coordinate of the missed shot.</param>
        public void SetMiss(int x, int y)
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("splash.wav");
            this.DrawMiss(x, y);
        }

        private void DrawMiss(int x, int y)
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

            this.AddControl(missPicture);
        }

        /// <summary>
        /// Decide based on the stored image in the panel which explosion should be presented.
        /// </summary>
        /// <param name="x">X Coordinate of the hit.</param>
        /// <param name="y">Y Coordinate of the hit.</param>
        public void DrawExplosion(int x, int y)
        {
            PictureBox explPicture = new PictureBox();
            explPicture.Name = "expl_" + x.ToString() + ":" + y.ToString();
            explPicture.Location = new Point(x * 30, y * 30);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explo6;
            
            // Explosion PictureBox - Add to the panel where the impact is
            this.AddControl(explPicture);
        }
        
        public void AddControl(Control contr)
        {
            if (this.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(this.AddControl);
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
        public void PlayerClicked(object sender, MouseEventArgs e)
        {
            // Only start if both players are ready
            if (BattleshipsForm.OpponentReadyToPlay && BattleshipsForm.PlayerReadyToPlay)
            {
                // can only shoot if it is your turn
                if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.player)
                {
                    Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;
                    switch (e.Button)
                    {
                        case System.Windows.Forms.MouseButtons.Left:
                            // has a game been joined?
                            if (BattleshipsForm.ClientGameForm != null)
                            {
                                if (BattleshipsForm.ClientGameForm.MainClientSocket != null)
                                {
                                    try
                                    {
                                        object sendData = tmp.Name;
                                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                        // Send coordinates of the current field to server
                                        BattleshipsForm.ClientGameForm.MainClientSocket.Send(byteData);

                                        // Wait for a response (HIT\MISS\WIN\LOSE)
                                        BattleshipsForm.ClientGameForm.WaitForData();
                                    }
                                    catch (Exception ex)
                                    {
                                        BattleshipsForm.ClientGameForm.SetText(ex.ToString());
                                    }
                                }
                            }
                            else if (BattleshipsForm.HostGameForm != null)
                            {
                            // Or do I host a game myself?
                                try
                                {
                                    if (BattleshipsForm.HostGameForm.MainWorkerSocket != null)
                                    {
                                        try
                                        {
                                            object sendData = tmp.Name;
                                            byte[] byteData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                            // Send coordinates of the current field to Server
                                            BattleshipsForm.HostGameForm.MainWorkerSocket.Send(byteData);

                                            // wait for an answer (HIT\MISS\WIN\LOSE)
                                            BattleshipsForm.HostGameForm.WaitForData(BattleshipsForm.HostGameForm.MainWorkerSocket);
                                        }
                                        catch (Exception ex)
                                        {
                                            BattleshipsForm.HostGameForm.SetText(ex.ToString());
                                            BattleshipsForm.HostGameForm.MainClientCount--;
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

        public void PlayerMouseEnter(object sender, EventArgs e)
        {
            if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.player)
            {
                Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

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

        public void PlayerMouseLeave(object sender, EventArgs e)
        {
            if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.player)
            {
                Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

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
        /// Creates a cursor from a bitmap (PNG, JPEG is also good).
        /// </summary>
        /// <param name="bmp">The image file to be displayed as the cursor.</param>
        /// <param name="hotSpotX">X value of the hotspot.</param>
        /// <param name="hotSpotY">Y value of the hotspot.</param>
        /// <returns>The created cursor.</returns>
        public static Cursor CreateCursor(Bitmap bmp, int hotSpotX, int hotSpotY)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.HotspotX = hotSpotX;
            tmp.HotspotY = hotSpotY;
            tmp.ParameterfIcon = false;
            return new Cursor(CreateIconIndirect(ref tmp));
        }
    }
}