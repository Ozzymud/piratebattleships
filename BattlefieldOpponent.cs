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
// <project>Battleships Pirate Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
//-----------------------------------------------------------------------

namespace Battleships
{
#region directives
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
#endregion

/// <summary>
/// The opponents battlefield.
/// </summary>
public class BattlefieldOpponent : DoubleBufferedPanel
    {
    #region fields
    /// <summary>
    /// Position color (When moving mouse over box).
    /// </summary>
    private Color positionColor;

    private Battleships.DoubleBufferedPanel[,] playField = new Battleships.DoubleBufferedPanel[10, 10];
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="BattlefieldOpponent" /> class.
    /// </summary>
    /// <param name="x">X parameter on the opponent playfield.</param>
    /// <param name="y">Y parameter on the opponent playfield.</param>
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
        this.BackColor = Color.Transparent;

        // Enemies grid.
        for (int i = 0; i < this.playField.GetLength(0); i++)
        {
            for (int j = 0; j < this.playField.GetLength(1); j++)
            {
                Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                p.Location = new Point(i * 30, j * 30);
                p.Tag = 0;
                p.Margin = new Padding(0);
                p.Name = "pf_" + i.ToString() + ":" + j.ToString();
                p.Size = new Size(30, 30);
                p.BorderStyle = BorderStyle.FixedSingle;
                p.MouseClick += new MouseEventHandler(this.PlayerMouse_Click);
                p.MouseEnter += new EventHandler(this.PlayerMouseEnter);
                p.MouseLeave += new EventHandler(this.PlayerMouseLeave);
                p.Cursor = CreateCursor(bitmap, 16, 16);
                p.BackColor = Color.Transparent;
                p.BorderStyle = BorderStyle.None;
                this.playField[i, j] = p;
                this.Controls.Add(p);
            }
        }
    }
    #endregion

    #region delegate
    private delegate void AddControlCallback(Control contr);

    private delegate void ShowDestroyedShipsCallback(int[] args, bool horizontal);
    #endregion

    #region methods
    #region public static
    /// <summary>
    /// Creates a cursor from a bitmap (PNG, JPEG is also good).
    /// </summary>
    /// <param name="bmp">The image file to be displayed as the cursor.</param>
    /// <param name="hotspotX">X value of the hotspot.</param>
    /// <param name="hotspotY">Y value of the hotspot.</param>
    /// <returns>The created cursor.</returns>
    public static Cursor CreateCursor(Bitmap bmp, int hotspotX, int hotspotY)
        {
            IconInfo tmp = new IconInfo();
            NativeMethods.GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.HotspotX = hotspotX;
            tmp.HotspotY = hotspotY;
            tmp.ParameterfIcon = false;
            return new Cursor(NativeMethods.CreateIconIndirect(ref tmp));
        }
    #endregion

    #region public
    /// <summary>
    /// Sets a hit on the specified field.
    /// </summary>
    /// <param name="hitX">X Coordinate of the hit.</param>
    /// <param name="hitY">Y Coordinate of the hit.</param>
    public void SetImpact(int hitX, int hitY)
    {
        try
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion2.wav");
            this.DrawExplosion(hitX, hitY);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    /// <summary>
    /// Sets a missed shot on the specified field.
    /// </summary>
    /// <param name="missX">X coordinate of the missed shot.</param>
    /// <param name="missY">Y coordinate of the missed shot.</param>
    public void SetMiss(int missX, int missY)
    {
        BattleshipsForm.SoundPlayer.PlaySoundAsync("splash.wav");
        this.DrawMiss(missX, missY);
    }

    /// <summary>
    /// Create an image with a name based on the coordinates of the explosion.
    /// </summary>
    /// <param name="hitX">X Coordinate of the hit.</param>
    /// <param name="hitY">Y Coordinate of the hit.</param>
    public void DrawExplosion(int hitX, int hitY)
    {
        PictureBox explPicture = new PictureBox();
        explPicture.Name = "expl_" + hitX.ToString() + ":" + hitY.ToString();
        explPicture.Location = new Point(hitX * 30, hitY * 30);
        explPicture.Size = new Size(30, 30);
        explPicture.Margin = new Padding(0);
        explPicture.Padding = new Padding(0);
        explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
        explPicture.BackColor = Color.Transparent;
        explPicture.Image = Properties.Resources.explosion_img;
        
        // Explosion PictureBox - Add to the panel where the impact is
        this.AddControl(explPicture);
    }
        
    /// <summary>
    /// Adds a control to the playfield form (miss, hit, etc).
    /// </summary>
    /// <param name="control">The resource to add.</param>
    public void AddControl(Control control)
    {
        if (this.InvokeRequired)
        {
            AddControlCallback d = new AddControlCallback(this.AddControl);
            this.Invoke(d, new object[] { control });
        }
        else
        {
            this.Controls.Add(control);
            Control[] s = this.Controls.Find(control.Name, false);
            s[0].BringToFront();
        }
    }
    #endregion

    #region private Mouse-Events
    private void PlayerMouse_Click(object sender, MouseEventArgs e)
        {
            // Only start if both players are ready
            if (BattleshipsForm.OpponentReadyToPlay && BattleshipsForm.PlayerReadyToPlay)
            {
                // can only shoot if it is your turn
                if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.Player)
                {
                    Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;
                    switch (e.Button)
                    {
                        case System.Windows.Forms.MouseButtons.Left:
                            // Has a game been joined?
                            if (BattleshipsForm.ClientGameForm != null)
                            {
                                if (BattleshipsForm.ClientGameForm.ClientSocket != null)
                                {
                                    try
                                    {
                                        object sendData = tmp.Name;
                                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                        // Send coordinates of the current field to server
                                        BattleshipsForm.ClientGameForm.ClientSocket.Send(byteData);

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
                                    if (BattleshipsForm.HostGameForm.WorkerSocket != null)
                                    {
                                        try
                                        {
                                            object sendData = tmp.Name;
                                            byte[] byteData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());

                                            // Send coordinates of the current field to Server
                                            BattleshipsForm.HostGameForm.WorkerSocket.Send(byteData);

                                            // Wait for an answer (HIT\MISS\WIN\LOSE)
                                            BattleshipsForm.HostGameForm.WaitForData(BattleshipsForm.HostGameForm.WorkerSocket);
                                        }
                                        catch (Exception ex)
                                        {
                                            BattleshipsForm.HostGameForm.SetText(ex.ToString());
                                            BattleshipsForm.HostGameForm.ClientCount--;
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

    private void PlayerMouseEnter(object sender, EventArgs e)
        {
            if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.Player)
            {
                Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

                for (int i = 0; i < this.playField.GetLength(0); i++)
                {
                    for (int j = 0; j < this.playField.GetLength(1); j++)
                    {
                        if (tmp.Name.ToString() == this.playField[i, j].Name.ToString())
                        {
                            this.playField[i, j].BackColor = this.positionColor;
                        }
                    }
                }
            }
        }

    private void PlayerMouseLeave(object sender, EventArgs e)
        {
            if (BattleshipsForm.WhosTurn == BattleshipsForm.TurnIdentifier.Player)
            {
                for (int i = 0; i < this.playField.GetLength(0); i++)
                {
                    for (int j = 0; j < this.playField.GetLength(1); j++)
                    {
                        if ((int)this.playField[i, j].Tag != (int)1)
                        {
                            this.playField[i, j].BackColor = Color.Transparent;
                        }
                    }
                }
            }
        }
    #endregion

    #region private
    /// <summary>
    /// Displays a destroyed boat on the enemies playing field.
    /// </summary>
    /// <param name="args">Contains the coordinates of the vessel.</param>
    /// <param name="horizontal">Specifies whether the ship was used horizontally or vertically.</param>
    private void ShowDestroyedBoat(int[] args, bool horizontal)
    {
        if (this.InvokeRequired)
        {
            ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedBoat);
            this.Invoke(d, new object[] { args, horizontal });
        }
        else
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

            // Remove explosion picture at the specified position (remove--> PictureBox control)
            this.playField[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
            this.playField[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
            if (horizontal)
            {
                this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h2;
            }
            else
            {
                this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
            }
        }
    }

    /// <summary>
    ///  Displays a ruined cruiser on the enemy field.
    /// </summary>
    /// <param name="args">The co-ordinates of the vessel.</param>
    /// <param name="horizontal">Specifies whether the ship was placed horizontally or vertically.</param>
    private void ShowDestroyedCruiser(int[] args, bool horizontal)
    {
        if (this.InvokeRequired)
        {
            ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedCruiser);
            this.Invoke(d, new object[] { args, horizontal });
        }
        else
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

            // At the entered point remove explosion image (remove --> PictureBox control)
            this.playField[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
            this.playField[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
            this.playField[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

            if (horizontal)
            {
                this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_h1;
                this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_h2;
                this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_h3;
            }
            else
            {
                this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_v1;
                this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_v2;
                this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_v3;
            }
        }
    }
    
    /// <summary>
    /// Display a destroyed galley on the enemy field.
    /// </summary>
    /// <param name="args">The co-ordinates of the vessel.</param>
    /// <param name="horizontal">Specifies whether the ship was placed horizontally or vertically.</param>
    private void ShowDestroyedGalley(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedGalley);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // At the entered point remove explosion image (remove--> PictureBox control)
                this.playField[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.playField[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.playField[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());
                this.playField[args[6], args[7]].Controls.RemoveByKey("expl_" + args[6].ToString() + ":" + args[7].ToString());

                if (horizontal)
                {
                    this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.galley_dmg_h1;
                    this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.galley_dmg_h2;
                    this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.galley_dmg_h3;
                    this.playField[args[6], args[7]].BackgroundImage = Properties.Resources.galley_dmg_h4;
                }
                else
                {
                    this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.galley_dmg_v1;
                    this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.galley_dmg_v2;
                    this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.galley_dmg_v3;
                    this.playField[args[6], args[7]].BackgroundImage = Properties.Resources.galley_dmg_v4;
                }
            }
        }

    /// <summary>
    /// Display a destroyed galley on the enemy field.
    /// </summary>
    /// <param name="args">The co-ordinates of the vessel.</param>
    /// <param name="horizontal">Specifies whether the ship was placed horizontally or vertically.</param>
    private void ShowDestroyedBattleship(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedBattleship);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // At the entered point remove explosion image (remove--> PictureBox control)
                this.playField[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.playField[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.playField[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());
                this.playField[args[6], args[7]].Controls.RemoveByKey("expl_" + args[6].ToString() + ":" + args[7].ToString());

                if (horizontal)
                {
                    this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.z_dmg_h1;
                    this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.z_dmg_h2;
                    this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.z_dmg_h3;
                    this.playField[args[6], args[7]].BackgroundImage = Properties.Resources.z_dmg_h4;
                }
                else
                {
                    this.playField[args[0], args[1]].BackgroundImage = Properties.Resources.z_dmg_v1;
                    this.playField[args[2], args[3]].BackgroundImage = Properties.Resources.z_dmg_v2;
                    this.playField[args[4], args[5]].BackgroundImage = Properties.Resources.z_dmg_v3;
                    this.playField[args[6], args[7]].BackgroundImage = Properties.Resources.z_dmg_v4;
                }
            }
        }

    /// <summary>
    /// Draws a missed shot on the opponent's battlefield
    /// </summary>
    /// <param name="x">X parameter of the missed shot.</param>
    /// <param name="y">Y parameter of the missed shot.</param>
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
        missPicture.Image = Properties.Resources.splash2;
        this.AddControl(missPicture);
    }
    #endregion
    #endregion

    #region struct
    /// <summary>
    /// Information for formatting icons.
    /// </summary>
    public struct IconInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Using Win32 naming for consistency.")]
        private bool fIcon;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Using Win32 naming for consistency.")]
        private int yHotspot;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Using Win32 naming for consistency.")]
        private int xHotspot;
        private IntPtr hbmMask;
        private IntPtr hbmColor;

        public bool ParameterfIcon
        {
            get { return this.fIcon; }
            set { this.fIcon = value; }
        }
        
        public int HotspotX
        {
            get { return this.xHotspot; }
            set { this.xHotspot = value; }
        }

        public int HotspotY
        {
            get { return this.yHotspot; }
            set { this.yHotspot = value; }
        }
    }
    #endregion
    }
}