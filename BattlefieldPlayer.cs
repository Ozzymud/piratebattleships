//-----------------------------------------------------------------------
// <copyright file="BattlefieldPlayer.cs" company="Team 17">
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
using System.Text;
using System.Windows.Forms;
#endregion

/// <summary>
/// The players battlefield.
/// </summary>
public class BattlefieldPlayer : DoubleBufferedPanel
    {
    #region fields
    /// <summary>
    /// Public access to ShipModel.
    /// </summary>
    private ShipModel ships;

    /// <summary>
    /// Contains the playing field and all ships.
    /// </summary>
    private Battleships.DoubleBufferedPanel[,] playField = new Battleships.DoubleBufferedPanel[10, 10];

    /// <summary>
    /// Contains a shadow copy of the playing field.
    /// </summary>
    private Battleships.DoubleBufferedPanel[,] playfieldStore = new Battleships.DoubleBufferedPanel[10, 10];

    /// <summary>
    /// Color value will be displayed, if a collision at the ships detected.
    /// </summary>
    private Color collisionColor;

    /// <summary>
    /// Flag that indicates whether a ship is to be used horizontally or vertically.
    /// </summary>
    private bool horizontal;

    private int counterGalley = 0;
    private int counterBattleship = 0;
    private int counterCruiser = 0;
    private int counterBoat = 0;

    /// <summary>
    /// Manages the position of the boats and the state.
    /// </summary>
    private Ships.Boat[] boatReference = new Ships.Boat[3];                        // 3 Boat
    private Ships.Cruiser[] cruiserReference = new Ships.Cruiser[3];               // 3 Cruiser
    private Ships.Galley galleyReference = new Ships.Galley();                     // 1 Galley
    private Ships.Battleship battleshipReference = new Ships.Battleship();         // 1 Battleship
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="BattlefieldPlayer" /> class.
    /// by default the ships are placed horizontally.
    /// with a right click this changes to vertical
    /// </summary>
    /// <param name="gridX">X coordinate on the grid.</param>
    /// <param name="gridY">Y  coordinate on the grid.</param>
    public BattlefieldPlayer(int gridX, int gridY)
        {
            this.horizontal = true;
            this.collisionColor = new Color();
            this.collisionColor = Color.FromArgb(90, 210, 0, 0); // Bright red, A/R/G/B.
            this.Location = new Point(gridX, gridY);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            this.BackColor = Color.Transparent;

            // Players grid.
            for (int i = 0; i < this.playField.GetLength(0); i++)
            {
                for (int j = 0; j < this.playField.GetLength(1); j++)
                {
                    Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Padding = new Padding(0);
                    p.Name = "pf_" + i.ToString(CultureInfo.InvariantCulture) + ":" + j.ToString(CultureInfo.InvariantCulture);
                    p.Size = new Size(30, 30);
                    p.MouseClick += new MouseEventHandler(this.PlayerMouse_Click);
                    p.MouseEnter += new EventHandler(this.PlayerMouseEnter);
                    p.MouseLeave += new EventHandler(this.PlayerMouseLeave);
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.playField[i, j] = p;
                    this.Controls.Add(p);
                    this.playfieldStore[i, j] = new Battleships.DoubleBufferedPanel();
                }
            }
        }
    #endregion

    #region delegate
    private delegate void AddControlCallback(Control contr, int x, int y);

    private delegate void SetTextCallback(string text);

    private delegate void ShowDestroyedShipsCallback(int[] args, bool horizontal);
    #endregion

    #region enum
    /// <summary>
    /// Collection of ship models.
    /// </summary>
    public enum ShipModel
        {
            /// <summary>
            /// No ship.
            /// </summary>
            NoShip = 0,

            /// <summary>
            /// A galley.
            /// </summary>
            Galley = 1,

            /// <summary>
            /// A battleship.
            /// </summary>
            Battleship = 2,

            /// <summary>
            /// A cruiser.
            /// </summary>
            Cruiser = 3,

            /// <summary>
            /// A boat.
            /// </summary>
            Boat = 4
        }
    #endregion

    #region properties
    public ShipModel Ships
        {
            get { return this.ships; }
            set { this.ships = value; }
        }

    public int CounterGalley
    {
        get { return this.counterGalley; }
        set { this.counterGalley = value; } 
    }

    public int CounterBattleship
        {
            get { return this.counterBattleship; }
            set { this.counterBattleship = value; } 
        }

    public int CounterCruiser
        {
            get { return this.counterCruiser; }
            set { this.counterCruiser = value; } 
        }

    public int CounterBoat
                {
            get { return this.counterBoat; }
            set { this.counterBoat = value; } 
        }
    #endregion

    #region methods
    #region public
    /// <summary>
    /// Shows a destroyed boat on the enemies playing field.
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
    /// Display on the enemy field a ruined cruiser.
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
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // Remove explosion picture at the specified position (remove--> PictureBox control)
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
    public void ShowDestroyedGalley(int[] args, bool horizontal)
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
    public void ShowDestroyedBattleship(int[] args, bool horizontal)
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
    /// Decides which explosion is to be displayed in the panel.
    /// </summary>
    /// <param name="explosionX">X coordinate of the hit.</param>
    /// <param name="explosionY">Y coordinate of the hit.</param>
    public void DrawExplosion(int explosionX, int explosionY)
        {
            // PictureBox_DoubleBuffered explPicture = new PictureBox_DoubleBuffered();
            PictureBox explPicture = new PictureBox();
            explPicture.Name = "expl_" + explosionX.ToString(CultureInfo.InvariantCulture) + ":" + explosionY.ToString(CultureInfo.InvariantCulture);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explosion2_img;
            this.AddControl(explPicture, explosionX, explosionY); // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
        }

    /// <summary>
    /// Checks whether the opponent has hit something or not.
    /// </summary>
    /// <param name="shotX">X-Coordinate of the shot.</param>
    /// <param name="shotY">Y-Coordinate of the shot.</param>
    /// <returns>False if missed, true if hit.</returns>
    public bool HitOrMiss(int shotX, int shotY)
        {
            if (this.playField[shotX, shotY].BackgroundImage == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    /// <summary>
    /// Sets a hit on the specified field.
    /// </summary>
    /// <param name="hitX">X-coordinate of the hit.</param>
    /// <param name="hitY">Y-coordinate of the hit.</param>
    /// <returns>true or false if enemy won or not.</returns>
    public bool SetImpact(int hitX, int hitY)
        {
            try
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion2.wav");
                this.DrawExplosion(hitX, hitY); // Draw an explosion on the field
                this.CheckShips(hitX, hitY); // Check which ship was hit where

                // Check the status of the game.
                if (this.CheckGameStatus())
                {
                    return true; // Opponents won, because all ships were destroyed
                }

                return false; // No one has won up there...
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

    public void SetMiss(int missX, int missY)
    {
        BattleshipsForm.SoundPlayer.PlaySoundAsync("splash.wav");
        this.DrawMiss(missX, missY); // Draw a miss on the playing field
    }
    #endregion

    #region protected methods
    protected virtual void SetTextLabelStatus(string text)
        {
            if (BattleshipsForm.LabelStatus.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(this.SetTextLabelStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                BattleshipsForm.LabelStatus.Text += text;
                BattleshipsForm.PanelStatus.VerticalScroll.Value += BattleshipsForm.PanelStatus.VerticalScroll.SmallChange;
                BattleshipsForm.PanelStatus.Refresh();
            }
        }

    protected virtual void AddControl(Control control, int explosionX, int explosionY)
        {
            if (this.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(this.AddControl);
                this.Invoke(d, new object[] { control, explosionX, explosionY });
            }
            else
            {
                // Assign the explosion to the panel in which the hit was (The explosion is therefore displayed before the panel image)
                this.playField[explosionX, explosionY].Controls.Add(control);
            }
        }
    #endregion
    
    #region private methods
    #region Mouse-Events
    private void PlayerMouse_Click(object sender, MouseEventArgs e)
        {
            // Get the Panel which has thrown the MouseClick event.
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

            // Left mouse button pressed.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Set the ship to clicked position.
                this.SetShips(ref tmp);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Loop through the fields (PictureBox).
                for (int i = 0; i < this.playField.GetLength(0); i++)
                {
                    for (int j = 0; j < this.playField.GetLength(1); j++)
                    {
                        // Check if field contains a part of the ship (Tag = 1).
                        if ((int)this.playField[i, j].Tag != (int)1)
                        {
                            // If no, then delete image box.
                            this.playField[i, j].BackgroundImage = null;
                        }
                    }
                }

                this.horizontal = !this.horizontal; // Negate Value.
                this.DrawShips(ref tmp); // Draw ship.
            }
        }

    private void PlayerMouseEnter(object sender, EventArgs e)
        {
            // Event was triggered by a DoubleBufferedPanel.
            // Get sender object--> Panel which triggered the event.
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;
            this.DrawShips(ref tmp); // Draw ship.
        }

    private void PlayerMouseLeave(object sender, EventArgs e)
        {
            // Cycle through all the panels.
            for (int x = 0; x < this.playField.GetLength(0); x++)
            {
                for (int y = 0; y < this.playField.GetLength(1); y++)
                {
                    // If no image is saved set current panel
                    if ((int)this.playField[x, y].Tag != (int)1)
                    {
                        this.playField[x, y].BackgroundImage = null; // Then delete the image
                        this.playField[x, y].Tag = 0; // Image flag to false
                    }
                }
            }
        }
    #endregion

    private void DrawMiss(int x, int y)
        {
            // PictureBox missPicture = new PictureBox();
            // missPicture.Name = "miss_" + x.ToString() + ":" + y.ToString();
            // missPicture.Location = new Point(x * 30, y * 30);
            // missPicture.Size = new Size(30, 30);
            // missPicture.Margin = new Padding(0);
            // missPicture.Padding = new Padding(0);
            // missPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            // missPicture.BackColor = Color.Transparent;
            // missPicture.Image = Properties.Resources.platsch;

            // AddControl(missPicture, x, y);
            this.playField[x, y].BackgroundImage = Properties.Resources.splash2;
            this.playField[x, y].Tag = 1;
        }

    /// <summary>
    /// Checks the game status (If all my ships are destroyed, the opponent has won).
    /// </summary>
    /// <returns>
    /// True if all my ships have been destroyed.
    /// False if at least one of my ships exists.
    /// </returns>
    private bool CheckGameStatus()
        {
            // Check all boats.
            for (int i = 0; i < this.boatReference.Length; i++)
            {
                // Check if all boats are destroyed.
                if (!this.boatReference[i].ShipDestroyed)
                {
                    // If only a single boat remains, then the game is not over.
                    return false;
                }
            }
            
            // Check cruisers.
            for (int i = 0; i < this.cruiserReference.Length; i++)
            {
                // Check if all cruisers are destroyed
                if (!this.cruiserReference[i].ShipDestroyed)
                { 
                    // If there is a cruiser left, then the game is not over.
                    return false;
                }
            }

             // Check galleys.
            if (!this.galleyReference.ShipDestroyed)
            {
                // If there is a galley left, then the game is not over.
                return false;
            }

            // Check battleships.
            if (!this.battleshipReference.ShipDestroyed)
            {
                // If there is a battleship left, then the game is not over.
                return false;
            }

            return true; // Are all ships destroyed? If so, then the game is over and you lose!!
        }

    /// <summary>
    /// Find out what ship was hit, on which part, and if a ship was completely destroyed.
    /// </summary>
    /// <param name="x">X-Coordinate of the hit.</param>
    /// <param name="y">Y-Coordinate of the hit.</param>
    private void CheckShips(int x, int y)
        {
            // Find out what ship was hit.
            // Was the boat hit?
            if (this.playfieldStore[x, y].Name.StartsWith("Boat_", StringComparison.Ordinal))
            {
                // Find out the number of the boat (from 1-3)
                string[] shipBoat = this.playfieldStore[x, y].Name.Split('_');
                int boatNr = int.Parse(shipBoat[1], CultureInfo.InvariantCulture);
                string boatPart = shipBoat[2];

                // Which part of the boat was hit?
                switch (boatPart)
                {
                    case "Rear":
                        // Rear was hit (horizontal)
                        this.boatReference[boatNr].Rear = true;
                        this.SetTextLabelStatus("Boat  " + boatNr.ToString(CultureInfo.InvariantCulture) + " rear was hit!\n");
                        break;
                    case "Front":
                        // Front was hit (horizontal)
                        this.boatReference[boatNr].Front = true;
                        this.SetTextLabelStatus("Boat " + boatNr.ToString(CultureInfo.InvariantCulture) + " front was hit!\n");
                        break;
                }

                // Was the boat completely destroyed?
                if (this.boatReference[boatNr].Front && this.boatReference[boatNr].Rear)
                {
                    this.boatReference[boatNr].ShipDestroyed = true;
                    this.ShowDestroyedBoat( // Show the destroyed boat on the playfield
                        new int[4]
                        {
                        this.boatReference[boatNr].PosRearX, this.boatReference[boatNr].PosRearY,
                        this.boatReference[boatNr].PosFrontX, this.boatReference[boatNr].PosFrontY
                        },
                        this.boatReference[boatNr].IsHorizontal);
                    ////BattleshipsForm.BattlefieldOpponent.ShowDestroyedBoat(
                    ////    new int[4]
                    ////    {
                    ////    this.boatReference[boatNr].PosRearX, this.boatReference[boatNr].PosRearY,
                    ////    this.boatReference[boatNr].PosFrontX, this.boatReference[boatNr].PosFrontY
                    ////    },
                    ////    this.boatReference[boatNr].IsHorizontal);
                    this.SetTextLabelStatus("Boat " + boatNr.ToString(CultureInfo.InvariantCulture) + " destroyed!\n");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Cruiser_", StringComparison.Ordinal))
            {
                // Is a cruiser hit?
                string[] shipCruiser = this.playfieldStore[x, y].Name.Split('_');
                int cruiserNr = int.Parse(shipCruiser[1]);
                string cruiserPart = shipCruiser[2];

                // Which part of the cruiser was hit?
                switch (cruiserPart)
                {
                    case "Rear":
                        // Rear was hit (horizontal).
                        this.cruiserReference[cruiserNr].Rear = true;
                        this.SetTextLabelStatus("Cruiser " + cruiserNr.ToString() + " rear was hit!\n");
                        break;
                    case "Middle":
                        // Middle was hit.
                        this.cruiserReference[cruiserNr].Middle = true;
                        this.SetTextLabelStatus("Cruiser " + cruiserNr.ToString() + " middle was hit!\n");
                        break;
                    case "Front":
                        // Front was hit (horizontal).
                        this.cruiserReference[cruiserNr].Front = true;
                        this.SetTextLabelStatus("Cruiser " + cruiserNr.ToString() + " front was hit!\n");
                        break;
                }

                // Was the cruiser completely destroyed?
                if (this.cruiserReference[cruiserNr].Rear && this.cruiserReference[cruiserNr].Middle && this.cruiserReference[cruiserNr].Front)
                {
                    this.cruiserReference[cruiserNr].ShipDestroyed = true;
                    
                    this.ShowDestroyedCruiser( // Show the destroyed cruiser on the playfield.
                        new int[6]
                        {
                        this.cruiserReference[cruiserNr].PosRearX, this.cruiserReference[cruiserNr].PosRearY,
                        this.cruiserReference[cruiserNr].PosMiddleX, this.cruiserReference[cruiserNr].PosMiddleY,
                        this.cruiserReference[cruiserNr].PosFrontX, this.cruiserReference[cruiserNr].PosFrontY
                        },
                        this.cruiserReference[cruiserNr].IsHorizontal);
                    ////BattleshipsForm.BattlefieldOpponent.ShowDestroyedCruiser(
                    ////    new int[6]
                    ////    {
                    ////    this.cruiserReference[cruiserNr].PosRearX, this.cruiserReference[cruiserNr].PosRearY,
                    ////    this.cruiserReference[cruiserNr].PosMiddleX, this.cruiserReference[cruiserNr].PosMiddleY,
                    ////    this.cruiserReference[cruiserNr].PosFrontX, this.cruiserReference[cruiserNr].PosFrontY
                    ////    },
                    ////    this.cruiserReference[cruiserNr].IsHorizontal);
                    this.SetTextLabelStatus("Cruiser " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Galley_", StringComparison.Ordinal))
            {
                // Was the galley hit?
                string[] shipGalley = this.playfieldStore[x, y].Name.Split('_');
                string galleyPart = shipGalley[1];

                // Which part of the galley was hit?
                switch (galleyPart)
                {
                    case "Rear":
                        this.galleyReference.Rear = true;
                        this.SetTextLabelStatus("Galley's rear was hit!\n");
                        break;
                    case "Middle1":
                        this.galleyReference.MiddleFirstPart = true;
                        this.SetTextLabelStatus("Galley's rear middle was hit!\n");
                        break;
                    case "Middle2":
                        this.galleyReference.MiddleSecondPart = true;
                        this.SetTextLabelStatus("Galley's front middle was hit!\n");
                        break;
                    case "Front":
                        this.galleyReference.Front = true;
                        this.SetTextLabelStatus("Galley's front was hit!\n");
                        break;
                }

                // Was the galley completely destroyed?
                if (this.galleyReference.Rear && this.galleyReference.MiddleFirstPart && this.galleyReference.MiddleSecondPart && this.galleyReference.Front)
                {
                    this.galleyReference.ShipDestroyed = true;
                    this.ShowDestroyedGalley( // Show the destroyed galley on the playfield.
                        new int[8]
                        {
                        this.galleyReference.PosRearX, this.galleyReference.PosRearY,
                        this.galleyReference.PosMiddleFirstX, this.galleyReference.PosMiddleFirstY,
                        this.galleyReference.PosMiddleSecondX, this.galleyReference.PosMiddleSecondY,
                        this.galleyReference.PosFrontX, this.galleyReference.PosFrontY
                        },
                        this.galleyReference.IsHorizontal);
                    ////BattleshipsForm.BattlefieldOpponent.ShowDestroyedGalley(
                    ////    new int[8]
                    ////    {
                    ////    this.galleyReference.PosRearX, this.galleyReference.PosRearY,
                    ////    this.galleyReference.PosMiddleSecondX, this.galleyReference.PosMiddleFirstY,
                    ////    this.galleyReference.PosMiddleSecondX, this.galleyReference.PosMiddleSecondY,
                    ////    this.galleyReference.PosFrontX, this.galleyReference.PosFrontY
                    ////    },
                    ////    this.galleyReference.IsHorizontal);
                    this.SetTextLabelStatus("Galley destroyed!\n");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Battleship_", StringComparison.Ordinal))
            {
            // Was the battleship hit?
                string[] shipBattleship = this.playfieldStore[x, y].Name.Split('_');
                string battleshipPart = shipBattleship[1];

                // What part of the battleship was hit?
                switch (battleshipPart)
                {
                    case "Rear":
                        this.battleshipReference.Rear = true;
                        this.SetTextLabelStatus("Battleship was hit in the rear!\n");
                        break;
                    case "Middle1":
                        this.battleshipReference.MiddleFirstPart = true;
                        this.SetTextLabelStatus("Battleship was hit in the rear middle part!\n");
                        break;
                    case "Middle2":
                        this.battleshipReference.MiddleSecondPart = true;
                        this.SetTextLabelStatus("Battleship was hit in the front middle part!\n");
                        break;
                    case "Front":
                        this.battleshipReference.Front = true;
                        this.SetTextLabelStatus("Battleship was hit in the front!\n");
                        break;
                }

                // Was the battleship completely destroyed?
                if (this.battleshipReference.Rear && this.battleshipReference.MiddleFirstPart && this.battleshipReference.MiddleSecondPart && this.battleshipReference.Front)
                {
                    this.battleshipReference.ShipDestroyed = true;
                    this.ShowDestroyedBattleship( // Show the destroyed battleship on the playfield.
                        new int[8]
                        {
                        this.battleshipReference.PosRearX, this.battleshipReference.PosRearY,
                        this.battleshipReference.PosMiddleFirstX, this.battleshipReference.PosMiddleFirstY,
                        this.battleshipReference.PosMiddleSecondX, this.battleshipReference.PosMiddleSecondY,
                        this.battleshipReference.PosFrontX, this.battleshipReference.PosFrontY
                        },
                        this.battleshipReference.IsHorizontal);
                    ////BattleshipsForm.BattlefieldOpponent.ShowDestroyedBattleship(
                    ////    new int[8]
                    ////    {
                    ////    this.battleshipReference.PosRearX, this.battleshipReference.PosRearY,
                    ////    this.battleshipReference.PosMiddleSecondX, this.battleshipReference.PosMiddleFirstY,
                    ////    this.battleshipReference.PosMiddleSecondX, this.battleshipReference.PosMiddleSecondY,
                    ////    this.battleshipReference.PosFrontX, this.battleshipReference.PosFrontY
                    ////    },
                    ////    this.battleshipReference.IsHorizontal);
                    this.SetTextLabelStatus("Battleship destroyed!\n");
                }
            }
        }

    /// <summary>
    /// Sets the ship parts to the respective positions.
    /// (For a Mouse Click event).
    /// </summary>
    /// <param name="tmp">The Panel, which has thrown the MouseClick event (as a reference).</param>
    private void SetShips(ref Battleships.DoubleBufferedPanel tmp)
        {
            string positionString = tmp.Name;
            positionString = positionString.Remove(0, 3); // remove "pf_" from the string.
            string[] position = positionString.Split(':'); // x and y position.
            int x = int.Parse(position[0], CultureInfo.InvariantCulture);
            int y = int.Parse(position[1], CultureInfo.InvariantCulture);

            switch (this.Ships)
            {
                // galley
                case ShipModel.Galley:
                    if (this.horizontal)
                    {
                        // horizontal
                        // Galley is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != (int)1 && (int)this.playField[x - 1, y].Tag != (int)1 && (int)this.playField[x - 2, y].Tag != (int)1 && (int)this.playField[x - 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.playField[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                                tmp.Tag = 1;
                                this.playField[x - 1, y].Tag = 1;
                                this.playField[x - 2, y].Tag = 1;
                                this.playField[x - 3, y].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosFrontX = x;
                                this.galleyReference.PosFrontY = y;
                                this.galleyReference.PosMiddleFirstX = x - 1;
                                this.galleyReference.PosMiddleFirstY = y;
                                this.galleyReference.PosMiddleSecondX = x - 2;
                                this.galleyReference.PosMiddleSecondY = y;
                                this.galleyReference.PosRearX = x - 3;
                                this.galleyReference.PosRearY = y;
                                this.galleyReference.ShipDestroyed = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x - 2, y].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x - 3, y].Name = this.galleyReference.ShipName + "_" + "Rear";

                                // Set ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x - 1, y].BackColor = this.collisionColor;
                                this.playField[x - 2, y].BackColor = this.collisionColor;
                                this.playField[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x - 1, y].BackColor = Color.Transparent;
                                this.playField[x - 2, y].BackColor = Color.Transparent;
                                this.playField[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Otherwise draw the galley in the normal direction (4 fields).
                            if ((int)tmp.Tag != (int)1 && (int)this.playField[x + 1, y].Tag != (int)1 && (int)this.playField[x + 2, y].Tag != (int)1 && (int)this.playField[x + 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.playField[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                                tmp.Tag = 1;
                                this.playField[x + 1, y].Tag = 1;
                                this.playField[x + 2, y].Tag = 1;
                                this.playField[x + 3, y].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosRearX = x;
                                this.galleyReference.PosRearY = y;
                                this.galleyReference.PosMiddleSecondX = x + 1;
                                this.galleyReference.PosMiddleSecondY = y;
                                this.galleyReference.PosMiddleFirstX = x + 2;
                                this.galleyReference.PosMiddleFirstY = y;
                                this.galleyReference.PosFrontX = x + 3;
                                this.galleyReference.PosFrontY = y;
                                this.galleyReference.ShipDestroyed = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x + 2, y].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x + 3, y].Name = this.galleyReference.ShipName + "_" + "Front";

                                // Set ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x + 1, y].BackColor = this.collisionColor;
                                this.playField[x + 2, y].BackColor = this.collisionColor;
                                this.playField[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x + 1, y].BackColor = Color.Transparent;
                                this.playField[x + 2, y].BackColor = Color.Transparent;
                                this.playField[x + 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Galley is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1 && (int)playField[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                this.playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                this.playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                this.playField[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                                tmp.Tag = 1;
                                this.playField[x, y - 1].Tag = 1;
                                this.playField[x, y - 2].Tag = 1;
                                this.playField[x, y - 3].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosFrontX = x;
                                this.galleyReference.PosFrontY = y;
                                this.galleyReference.PosMiddleFirstX = x;
                                this.galleyReference.PosMiddleFirstY = y - 1;
                                this.galleyReference.PosMiddleSecondX = x;
                                this.galleyReference.PosMiddleSecondY = y - 2;
                                this.galleyReference.PosRearX = x;
                                this.galleyReference.PosRearY = y - 3;
                                this.galleyReference.ShipDestroyed = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Front";
                                this.playfieldStore[x, y - 1].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x, y - 2].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x, y - 3].Name = this.galleyReference.ShipName + "_" + "Rear";

                                // Set ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                playField[x, y - 1].BackColor = collisionColor;
                                playField[x, y - 2].BackColor = collisionColor;
                                playField[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                playField[x, y - 1].BackColor = Color.Transparent;
                                playField[x, y - 2].BackColor = Color.Transparent;
                                playField[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Otherwise draw the galley in the normal direction (4 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1 && (int)playField[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                this.playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                this.playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                this.playField[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                                tmp.Tag = 1;
                                this.playField[x, y + 1].Tag = 1;
                                this.playField[x, y + 2].Tag = 1;
                                this.playField[x, y + 3].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosRearX = x;
                                this.galleyReference.PosRearY = y;
                                this.galleyReference.PosMiddleSecondX = x;
                                this.galleyReference.PosMiddleSecondY = y + 1;
                                this.galleyReference.PosMiddleFirstX = x;
                                this.galleyReference.PosMiddleFirstY = y + 2;
                                this.galleyReference.PosFrontX = x;
                                this.galleyReference.PosFrontY = y + 3;
                                this.galleyReference.ShipDestroyed = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x, y + 1].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x, y + 2].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x, y + 3].Name = this.galleyReference.ShipName + "_" + "Front";

                                // Set the ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y + 1].BackColor = this.collisionColor;
                                this.playField[x, y + 2].BackColor = this.collisionColor;
                                this.playField[x, y + 3].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y + 1].BackColor = Color.Transparent;
                                this.playField[x, y + 2].BackColor = Color.Transparent;
                                this.playField[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModel.Battleship: // battleship
                    if (this.horizontal)
                    {
                        // horizontal
                        // Battleship is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1 && (int)this.playField[x - 2, y].Tag != 1 && (int)this.playField[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.playField[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                                tmp.Tag = 1;
                                this.playField[x - 1, y].Tag = 1;
                                this.playField[x - 2, y].Tag = 1;
                                this.playField[x - 3, y].Tag = 1;

                                // Set the ship selection to nothing.
                                this.Ships = ShipModel.NoShip;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosFrontX = x;
                                this.battleshipReference.PosFrontY = y;
                                this.battleshipReference.PosMiddleFirstX = x - 1;
                                this.battleshipReference.PosMiddleFirstY = y;
                                this.battleshipReference.PosMiddleSecondX = x - 2;
                                this.battleshipReference.PosMiddleSecondY = y;
                                this.battleshipReference.PosRearX = x - 3;
                                this.battleshipReference.PosRearY = y;
                                this.battleshipReference.ShipDestroyed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x - 2, y].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x - 3, y].Name = this.battleshipReference.ShipName + "_" + "Rear";
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x - 1, y].BackColor = this.collisionColor;
                                this.playField[x - 2, y].BackColor = this.collisionColor;
                                this.playField[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x - 1, y].BackColor = Color.Transparent;
                                this.playField[x - 2, y].BackColor = Color.Transparent;
                                this.playField[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Otherwise draw the battleship in the normal direction (5 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1 && (int)this.playField[x + 2, y].Tag != 1 && (int)this.playField[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.playField[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                                tmp.Tag = 1;
                                this.playField[x + 1, y].Tag = 1;
                                this.playField[x + 2, y].Tag = 1;
                                this.playField[x + 3, y].Tag = 1;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosRearX = x;
                                this.battleshipReference.PosRearY = y;
                                this.battleshipReference.PosMiddleSecondX = x + 1;
                                this.battleshipReference.PosMiddleSecondY = y;
                                this.battleshipReference.PosMiddleFirstX = x + 2;
                                this.battleshipReference.PosMiddleFirstY = y;
                                this.battleshipReference.PosFrontX = x + 3;
                                this.battleshipReference.PosFrontY = y;
                                this.battleshipReference.ShipDestroyed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x + 2, y].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x + 3, y].Name = this.battleshipReference.ShipName + "_" + "Front";

                                // Set the ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x + 1, y].BackColor = this.collisionColor;
                                this.playField[x + 2, y].BackColor = this.collisionColor;
                                this.playField[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x + 1, y].BackColor = Color.Transparent;
                                this.playField[x + 2, y].BackColor = Color.Transparent;
                                this.playField[x + 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Battleship is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1 && (int)playField[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                this.playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                this.playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                this.playField[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                                tmp.Tag = 1;
                                this.playField[x, y - 1].Tag = 1;
                                this.playField[x, y - 2].Tag = 1;
                                this.playField[x, y - 3].Tag = 1;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosFrontX = x;
                                this.battleshipReference.PosFrontY = y;
                                this.battleshipReference.PosMiddleFirstX = x;
                                this.battleshipReference.PosMiddleFirstY = y - 1;
                                this.battleshipReference.PosMiddleSecondX = x;
                                this.battleshipReference.PosMiddleSecondY = y - 2;
                                this.battleshipReference.PosRearX = x;
                                this.battleshipReference.PosRearY = y - 3;
                                this.battleshipReference.ShipDestroyed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Front";
                                this.playfieldStore[x, y - 1].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x, y - 2].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x, y - 3].Name = this.battleshipReference.ShipName + "_" + "Rear";

                                // Set the ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y - 1].BackColor = this.collisionColor;
                                this.playField[x, y - 2].BackColor = this.collisionColor;
                                this.playField[x, y - 3].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y - 1].BackColor = Color.Transparent;
                                this.playField[x, y - 2].BackColor = Color.Transparent;
                                this.playField[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Otherwise draw the battleship in the normal direction (5 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1 && (int)playField[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                this.playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                this.playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                this.playField[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                                tmp.Tag = 1;
                                this.playField[x, y + 1].Tag = 1;
                                this.playField[x, y + 2].Tag = 1;
                                this.playField[x, y + 3].Tag = 1;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosRearX = x;
                                this.battleshipReference.PosRearY = y;
                                this.battleshipReference.PosMiddleSecondX = x;
                                this.battleshipReference.PosMiddleSecondY = y + 1;
                                this.battleshipReference.PosMiddleFirstX = x;
                                this.battleshipReference.PosMiddleFirstY = y + 2;
                                this.battleshipReference.PosFrontX = x;
                                this.battleshipReference.PosFrontY = y + 3;
                                this.battleshipReference.ShipDestroyed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x, y + 1].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x, y + 2].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x, y + 3].Name = this.battleshipReference.ShipName + "_" + "Front";

                                // Set the ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y + 1].BackColor = this.collisionColor;
                                this.playField[x, y + 2].BackColor = this.collisionColor;
                                this.playField[x, y + 3].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y + 1].BackColor = Color.Transparent;
                                this.playField[x, y + 2].BackColor = Color.Transparent;
                                this.playField[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModel.Cruiser: // Cruiser
                    if (this.horizontal)
                    {
                        // horizontal
                        // Cruiser is 3 fields long, if field 8 is reached, draw the ship in the opposite direction.
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1 && (int)this.playField[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                tmp.Tag = 1;
                                this.playField[x - 1, y].Tag = 1;
                                this.playField[x - 2, y].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString(CultureInfo.InvariantCulture);
                                this.cruiserReference[this.CounterCruiser].PosFrontX = x;
                                this.cruiserReference[this.CounterCruiser].PosFrontY = y;
                                this.cruiserReference[this.CounterCruiser].PosMiddleX = x - 1;
                                this.cruiserReference[this.CounterCruiser].PosMiddleY = y;
                                this.cruiserReference[this.CounterCruiser].PosRearX = x - 2;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y;
                                this.cruiserReference[this.CounterCruiser].ShipDestroyed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x - 2, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";

                                // Set the ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x - 1, y].BackColor = this.collisionColor;
                                this.playField[x - 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x - 1, y].BackColor = Color.Transparent;
                                this.playField[x - 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Otherwise draw the cruiser in the normal direction (3 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1 && (int)this.playField[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                tmp.Tag = 1;
                                this.playField[x + 1, y].Tag = 1;
                                this.playField[x + 2, y].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString(CultureInfo.InvariantCulture);
                                this.cruiserReference[this.CounterCruiser].PosRearX = x;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y;
                                this.cruiserReference[this.CounterCruiser].PosMiddleX = x + 1;
                                this.cruiserReference[this.CounterCruiser].PosMiddleY = y;
                                this.cruiserReference[this.CounterCruiser].PosFrontX = x + 2;
                                this.cruiserReference[this.CounterCruiser].PosFrontY = y;
                                this.cruiserReference[this.CounterCruiser].ShipDestroyed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x + 2, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Front";

                                // Set the ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x + 1, y].BackColor = this.collisionColor;
                                this.playField[x + 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x + 1, y].BackColor = Color.Transparent;
                                this.playField[x + 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Cruiser is 3 fields long, if field 8 is reached, draw the ship in the opposite direction.
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                this.playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                this.playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                tmp.Tag = 1;
                                this.playField[x, y - 1].Tag = 1;
                                this.playField[x, y - 2].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString(CultureInfo.InvariantCulture);
                                this.cruiserReference[this.CounterCruiser].PosFrontX = x;
                                this.cruiserReference[this.CounterCruiser].PosFrontY = y;
                                this.cruiserReference[this.CounterCruiser].PosMiddleX = x;
                                this.cruiserReference[this.CounterCruiser].PosMiddleY = y - 1;
                                this.cruiserReference[this.CounterCruiser].PosRearX = x;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y - 2;
                                this.cruiserReference[this.CounterCruiser].ShipDestroyed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Front";
                                this.playfieldStore[x, y - 1].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x, y - 2].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Rear";

                                // Set the ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x, y - 1].BackColor = this.collisionColor;
                                this.playField[x, y - 2].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y - 1].BackColor = Color.Transparent;
                                this.playField[x, y - 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Otherwise draw the cruiser in the normal direction. (3 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                this.playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                this.playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                tmp.Tag = 1;
                                this.playField[x, y + 1].Tag = 1;
                                this.playField[x, y + 2].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString(CultureInfo.InvariantCulture);
                                this.cruiserReference[this.CounterCruiser].PosRearX = x;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y;
                                this.cruiserReference[this.CounterCruiser].PosMiddleX = x;
                                this.cruiserReference[this.CounterCruiser].PosMiddleY = y + 1;
                                this.cruiserReference[this.CounterCruiser].PosFrontX = x;
                                this.cruiserReference[this.CounterCruiser].PosFrontY = y + 2;
                                this.cruiserReference[this.CounterCruiser].ShipDestroyed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Rear";
                                this.playfieldStore[x, y + 1].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x, y + 2].Name = this.cruiserReference[CounterCruiser].ShipName + "_" + "Front";

                                // Set the ship selection to nothing.
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y + 1].BackColor = this.collisionColor;
                                this.playField[x, y + 2].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y + 1].BackColor = Color.Transparent;
                                this.playField[x, y + 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModel.Boat: // boat
                    if (this.horizontal)
                    {
                        // horizontal
                        // Boat is 2 fields long, if field 9 is reached, draw the ship in the opposite direction.
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                                tmp.Tag = 1;
                                this.playField[x - 1, y].Tag = 1;

                                // Save position and name of the ship
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString(CultureInfo.InvariantCulture);
                                this.boatReference[this.CounterBoat].PosFrontX = x;
                                this.boatReference[this.CounterBoat].PosFrontY = y;
                                this.boatReference[this.CounterBoat].PosRearX = x - 1;
                                this.boatReference[this.CounterBoat].PosRearY = y;
                                this.boatReference[this.CounterBoat].ShipDestroyed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].IsHorizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Rear";

                                // Set the ship selection to nothing.
                                this.Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x - 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x - 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1)
                            {
                                // Otherwise assemble the boat in the normal direction (2 fields)
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                                tmp.Tag = 1;
                                this.playField[x + 1, y].Tag = 1;
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString(CultureInfo.InvariantCulture); // Position sowie name des schiffes speichern
                                this.boatReference[this.CounterBoat].PosRearX = x;
                                this.boatReference[this.CounterBoat].PosRearY = y;
                                this.boatReference[this.CounterBoat].PosFrontX = x + 1;
                                this.boatReference[this.CounterBoat].PosFrontY = y;
                                this.boatReference[this.CounterBoat].ShipDestroyed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].IsHorizontal = this.horizontal;
                                this.playfieldStore[x, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.Ships = ShipModel.NoShip; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = this.collisionColor;
                                this.playField[x + 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x + 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Boat is 2 fields long, if field 9 is reached, draw the ship in the opposite direction.
                        if (y >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                this.playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                                tmp.Tag = 1;
                                this.playField[x, y - 1].Tag = 1;
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + CounterBoat.ToString(CultureInfo.InvariantCulture); // Position sowie name des schiffes speichern
                                this.boatReference[this.CounterBoat].PosFrontX = x;
                                this.boatReference[this.CounterBoat].PosFrontY = y;
                                this.boatReference[this.CounterBoat].PosRearX = x;
                                this.boatReference[this.CounterBoat].PosRearY = y - 1;
                                this.boatReference[this.CounterBoat].ShipDestroyed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].IsHorizontal = horizontal;
                                this.playfieldStore[x, y].Name = this.boatReference[CounterBoat].ShipName + "_" + "Front";
                                this.playfieldStore[x, y - 1].Name = this.boatReference[CounterBoat].ShipName + "_" + "Rear";
                                this.Ships = ShipModel.NoShip; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Show that no ship can be placed here (flash red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y - 1].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y - 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Otherwise assemble boat in the normal direction (2 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                this.playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                                tmp.Tag = 1;
                                this.playField[x, y + 1].Tag = 1;

                                // Store location and name of the vessel
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + CounterBoat.ToString(CultureInfo.InvariantCulture);
                                this.boatReference[this.CounterBoat].PosRearX = x;
                                this.boatReference[this.CounterBoat].PosRearY = y;
                                this.boatReference[this.CounterBoat].PosFrontX = x;
                                this.boatReference[this.CounterBoat].PosFrontY = y + 1;
                                this.boatReference[this.CounterBoat].ShipDestroyed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].IsHorizontal = horizontal;

                                this.playfieldStore[x, y].Name = this.boatReference[CounterBoat].ShipName + "_" + "Rear";
                                this.playfieldStore[x, y + 1].Name = this.boatReference[CounterBoat].ShipName + "_" + "Front";

                                // Ship selection set to nothing
                                Ships = ShipModel.NoShip;
                            }
                            else
                            {
                                // Show that no ship can be placed here  (blink red the corresponding fields)
                                tmp.BackColor = collisionColor;
                                this.playField[x, y + 1].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.playField[x, y + 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
            }
        }

    /// <summary>
    /// Draws the ships at the appropriate place.
    /// From a MouseEnter event on a Panel.
    /// </summary>
    /// <param name="tmp">The panel which has thrown the MouseEnter event (as a reference).</param>
    private void DrawShips(ref Battleships.DoubleBufferedPanel tmp)
        {
            string positionString = tmp.Name;
            positionString = positionString.Remove(0, 3); // Remove "pf_" from the string.
            string[] position = positionString.Split(':'); // x and y position
            int x = int.Parse(position[0], CultureInfo.InvariantCulture);
            int y = int.Parse(position[1], CultureInfo.InvariantCulture);

            switch (this.Ships)
            {
                case ShipModel.Galley: // galley
                    if (this.horizontal)
                    {
                        // horizontal
                        // Galley is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1 && (int)this.playField[x - 2, y].Tag != 1 && (int)this.playField[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.playField[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                            }
                        }
                        else
                        {
                        // Otherwise assemble the galley in the normal direction (4 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1 && (int)this.playField[x + 2, y].Tag != 1 && (int)this.playField[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.playField[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Galley is 4 fields long, if field 7 is reached, draw the ship in the opposite direction.
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1 && (int)playField[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                playField[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                            }
                        }
                        else // Otherwise assemble the galley in the normal direction (4 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1 && (int)playField[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                playField[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                            }
                        }
                    }

                    break;
                case ShipModel.Battleship: // battleship
                    if (this.horizontal)
                    {
                        // horizontal
                        // Battleship is 4 fields long, if field 6 is reached, draw the ship in the opposite direction.
                        if (x >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1 && (int)this.playField[x - 2, y].Tag != 1 && (int)this.playField[x - 3, y].Tag != 1 && (int)this.playField[x - 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.playField[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                            }
                        }
                        else
                        {
                        // Otherwise assemble the battleship in the normal direction (5 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1 && (int)this.playField[x + 2, y].Tag != 1 && (int)this.playField[x + 3, y].Tag != 1 && (int)this.playField[x + 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.playField[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Battleship is 4 fields long, if field 6 is reached, draw the ship in the opposite direction.
                        if (y >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1 && (int)playField[x, y - 3].Tag != 1 && (int)playField[x, y - 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                playField[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                            }
                        }
                        else // Otherwise assemble the battleship in the normal direction (5 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1 && (int)playField[x, y + 3].Tag != 1 && (int)playField[x, y + 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                playField[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                            }
                        }
                    }

                    break;
                case ShipModel.Cruiser: // Cruiser
                    if (this.horizontal)
                    {
                        // horizontal
                        // Cruiser is 3 fields long, if field 8 is reached, draw the ship in the opposite direction.
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1 && (int)this.playField[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.playField[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                            }
                        }
                        else
                        {
                        // Otherwise assemble the cruiser in the normal direction (3 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1 && (int)this.playField[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.playField[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Cruiser is 3 fields long, if field 8 is reached, draw the ship in the opposite direction.
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1 && (int)playField[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                playField[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                            }
                        }
                        else // Otherwise assemble the cruiser in the normal direction (3 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1 && (int)playField[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                playField[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                            }
                        }
                    }

                    break;
                case ShipModel.Boat: // boat
                    if (this.horizontal)
                    {
                        // horizontal
                        // Boat is 2 fields long, if field 9 is reached, draw the ship in the opposite direction.
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.playField[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.playField[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                            }
                        }
                        else
                        {
                        // Otherwise assemble the boat in the normal direction (2 fields)
                            if ((int)tmp.Tag != 1 && (int)this.playField[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.playField[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        // Boat is 2 fields long, if field 9 is reached, draw the ship in the opposite direction.
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                playField[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                            }
                        }
                        else // Otherwise assemble the boat in the normal direction (2 fields)
                        {
                            if ((int)tmp.Tag != 1 && (int)playField[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                playField[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                            }
                        }
                    }

                    break;
            }
        }
    #endregion
    #endregion
    }
}