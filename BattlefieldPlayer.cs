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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class BattlefieldPlayer : Battleships.DoubleBufferedPanel
    {
        private delegate void AddControlCallback(Control contr, int x, int y);

        private delegate void SetTextCallback(string text);

        private delegate void ShowDestroyedShipsCallback(int[] args, bool horizontal);

        // Auflistung der Schiffsmodelle
        public enum ShipModels
        {
            nothing = 0,
            galley = 1,
            battleship = 2,
            cruiser = 3,
            boat = 4
        }

        /// <summary>
        /// Manages the position of the boats and the state.
        /// </summary>
        private Ships.Boat[] boatReference = new Ships.Boat[3];                        // 3 Boote
        private Ships.Cruiser[] cruiserReference = new Ships.Cruiser[3];               // 3 Cruiser
        private Ships.Galley galleyReference = new Ships.Galley();                     // 1 Galley
        private Ships.Battleship battleshipReference = new Ships.Battleship();         // 1 Schlachtschiff

        private int counterGalley = 0;

        public int CounterGalley
        {
            get { return this.counterGalley; }
            set { this.counterGalley = value; } 
        }

        private int counterBattleship = 0;

        public int CounterBattleship
        {
            get { return this.counterBattleship; }
            set { this.counterBattleship = value; } 
        }

        private int counterCruiser = 0;

        public int CounterCruiser
        {
            get { return this.counterCruiser; }
            set { this.counterCruiser = value; } 
        }

        private int counterBoat = 0;

        public int CounterBoat
                {
            get { return this.counterBoat; }
            set { this.counterBoat = value; } 
        }

        /// <summary>
        /// Contains a collection of ship models.
        /// </summary>
        public ShipModels Ships;

        /// <summary>
        /// Contains the playing field and all ships.
        /// </summary>
        private Battleships.DoubleBufferedPanel[,] pf = new Battleships.DoubleBufferedPanel[10, 10];

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

        public BattlefieldPlayer(int x, int y)
        {
            // Ships are used as standard horizontal
            // with a click on the right mouse button this can be changed
            this.horizontal = true;
            this.collisionColor = new Color();
            this.collisionColor = Color.FromArgb(90, 210, 0, 0); // Ein helles Rot
            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //// this.BackgroundImageLayout = ImageLayout.Stretch;
            //// this.BackgroundImage = Battleships.Properties.Resources.sea_player;
            this.BackColor = Color.Transparent;
            //// this.BorderStyle = BorderStyle.FixedSingle;
            //// this.BorderStyle = BorderStyle.None;

            // Matrix Spieler
            for (int i = 0; i < this.pf.GetLength(0); i++)
            {
                for (int j = 0; j < this.pf.GetLength(1); j++)
                {
                    Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Padding = new Padding(0);
                    p.Name = "pf_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.MouseClick += new MouseEventHandler(this.PlayerMouse_Click);
                    p.MouseEnter += new EventHandler(this.PlayerMouseEnter);
                    p.MouseLeave += new EventHandler(this.PlayerMouseLeave);
                    //// p.Visible = false;
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.pf[i, j] = p;
                    this.Controls.Add(p);

                    this.playfieldStore[i, j] = new Battleships.DoubleBufferedPanel();
                }
            }
        }

        #region Mouse-Events
        private void PlayerMouse_Click(object sender, MouseEventArgs e)
        {
            // Das Panel holen, welches das MouseClick-Event ausgelöst hat
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

            // Linke Maustaste gedrückt
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Schiff an angeklickte Position setzen
                this.SetShips(ref tmp);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Loop through the fields (PictureBox)
                for (int i = 0; i < this.pf.GetLength(0); i++)
                {
                    for (int j = 0; j < this.pf.GetLength(1); j++)
                    {
                        // Prüfen ob Feld ein Schiffsteil beihnaltet (Tag = 1)
                        if ((int)this.pf[i, j].Tag != (int)1)
                        {
                            // Wenn nein, dann Bild im Feld löschen
                            this.pf[i, j].BackgroundImage = null;
                        }
                    }
                }

                this.horizontal = !this.horizontal; // Wert negieren
                this.DrawShips(ref tmp); // Schiff zeichnen
            }
        }

        private void PlayerMouseEnter(object sender, EventArgs e)
        {
            // Event wurde von einer Panel_DoubleBuffered ausgelöst...
            // Senderobjekt erhalten --> Panel welches das Event ausgelöst hat
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;
            this.DrawShips(ref tmp); // Schiff zeichnen
        }

        private void PlayerMouseLeave(object sender, EventArgs e)
        {
            // Alle Panels durchlaufen
            for (int x = 0; x < this.pf.GetLength(0); x++)
            {
                for (int y = 0; y < this.pf.GetLength(1); y++)
                {
                    // If no image is saved set current panel
                    if ((int)this.pf[x, y].Tag != (int)1)
                    {
                        this.pf[x, y].BackgroundImage = null; // Dann Bild löschen
                        this.pf[x, y].Tag = 0; // Bildflag auf false
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Checks whether the opponent has hit something or not.
        /// </summary>
        /// <param name="x">X-Coordinate of the shot.</param>
        /// <param name="y">Y-Coordinate of the shot.</param>
        /// <returns>False if missed, true if hit.</returns>
        public bool HitOrMiss(int x, int y)
        {
            if (this.pf[x, y].BackgroundImage == null)
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
        /// <param name="x">X-coordinate of the hit.</param>
        /// <param name="y">Y-coordinate of the hit.</param>
        /// <returns>true or false if enemy won or not.</returns>
        public bool SetImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion2.wav");
                this.DrawExplosion(x, y); // Draw an explosion on the field
                this.CheckShips(x, y); // Check which ship was hit where

                // Spielstatus prüfen
                if (this.CheckGameStatus())
                {
                    return true; // Opponents won, because all ships were destroyed
                }

                return false; // No one has won up here...
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public void SetMiss(int x, int y)
        {
            BattleshipsForm.SoundPlayer.PlaySoundAsync("splash.wav");
            this.DrawMiss(x, y); // Fehlschuss auf dem Spielfeld darstellen
        }

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
            this.pf[x, y].BackgroundImage = Properties.Resources.splash;
            this.pf[x, y].Tag = 1;
        }

        /// <summary>
        /// Shows a destroyed boat on the enemies playing field.
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

                // Explosionsbild an der angegeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pf[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pf[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                if (horizontal)
                {
                    this.pf[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h2;
                    this.pf[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                }
                else
                {
                    this.pf[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                    this.pf[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                }
            }
        }

        /// <summary>
        /// Display on the enemy field a ruined cruiser.
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
                // TODO: Show destroyed cruiser.
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // Explosionsbild an der angegebeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pf[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pf[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.pf[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

                if (horizontal)
                {
                    this.pf[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_h1;
                    this.pf[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_h2;
                    this.pf[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_h3;
                }
                else
                {
                    this.pf[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_v1;
                    this.pf[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_v2;
                    this.pf[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_v3;
                }
            }
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
            // Alle boote überprüfen
            for (int i = 0; i < this.boatReference.Length; i++)
            {
                // Überprüfen ob alle Boote zerstört sind
                if (!this.boatReference[i].ShipDestryoed)
                {
                    // Wenn auch nur noch ein Boot existiert, dann ist das Spiel nocht nicht vorbei
                    return false;
                }
            }
            
            // Alle Cruiser überprüfen
            for (int i = 0; i < this.cruiserReference.Length; i++)
            {
                // Überprüfen ob alle Cruiser zerstört sind
                if (!this.cruiserReference[i].ShipDestryoed)
                { 
                    // Wenn auch nur noch ein Cruiser existiert, dann ist das Spiel noch nicht vorbei
                    return false;
                }
            }

             // Die Galley überprüfen
            if (!this.galleyReference.ShipDestryoed)
            {
                // Existiert die Galley noch, dann ist das Spiel nocht nicht vorbei
                return false;
            }

            // Das Battleship überprüfen
            if (!this.battleshipReference.ShipDestryoed)
            {
                // Exisitiert das Battleship noch, dann ist das Spiel noch nicht vorbei
                return false;
            }

            return true; // Wurden ALLE Schiffe zerstört, dann ist das Spiel vorbei und DU hast verloren!!
        }

        /// <summary>
        /// Find out what ship was hit, on which part, and if a ship was completely destroyed.
        /// </summary>
        /// <param name="x">X-Coordinate of the hit.</param>
        /// <param name="y">Y-Coordinate of the hit.</param>
        private void CheckShips(int x, int y)
        {
            // Herausfinden welches Schiff getroffen wurde
            // Wurde ein Boot getroffen?
            if (this.playfieldStore[x, y].Name.StartsWith("Boat_"))
            {
                // Die Nr. des Bootes herausfinden (von 1-3)
                string[] shipBoat = this.playfieldStore[x, y].Name.Split('_');
                int boatNr = int.Parse(shipBoat[1]);
                string boatPart = shipBoat[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (boatPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (horizontal)
                        this.boatReference[boatNr].Rear = true;
                        this.SetTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (horizontal)
                        this.boatReference[boatNr].Front = true;
                        this.SetTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Boot komplett zerstört ist
                if (this.boatReference[boatNr].Front && this.boatReference[boatNr].Rear)
                {
                    this.boatReference[boatNr].ShipDestryoed = true;
                    this.ShowDestroyedBoat( // Das zerstörte Boot auf dem Spielfeld darstellen
                        new int[4]
                        {
                        this.boatReference[boatNr].PosRearX, this.boatReference[boatNr].PosRearY,
                        this.boatReference[boatNr].PosFrontX, this.boatReference[boatNr].PosFrontY
                        },
                        this.boatReference[boatNr].Horizontal);
                    this.ShowDestroyedBoat(
                        new int[4]
                        {
                        this.boatReference[boatNr].PosRearX, this.boatReference[boatNr].PosRearY,
                        this.boatReference[boatNr].PosFrontX, this.boatReference[boatNr].PosFrontY
                        },
                        this.boatReference[boatNr].Horizontal);
                    this.SetTextLblStatus("\nBoat Nr. " + boatNr.ToString() + " destroyed!");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Cruiser_"))
            {
            // Wurde ein Cruiser getroffen?
                string[] shipCruiser = this.playfieldStore[x, y].Name.Split('_');
                int cruiserNr = int.Parse(shipCruiser[1]);
                string cruiserPart = shipCruiser[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (cruiserPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (horizontal)
                        this.cruiserReference[cruiserNr].Rear = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Middle"):
                        // Mittelteil wurde getroffen
                        this.cruiserReference[cruiserNr].Middle = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Mittelteil getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (horizontal)
                        this.cruiserReference[cruiserNr].Front = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob der Cruiser komplett zerstört wurde
                if (this.cruiserReference[cruiserNr].Rear && this.cruiserReference[cruiserNr].Middle && this.cruiserReference[cruiserNr].Front)
                {
                    this.cruiserReference[cruiserNr].ShipDestryoed = true;
                    
                    this.ShowDestroyedCruiser( // Den zerstörten Cruiser auf dem Spielfeld darstellen
                        new int[6]
                        {
                        this.cruiserReference[cruiserNr].PosRearX, this.cruiserReference[cruiserNr].PosRearY,
                        this.cruiserReference[cruiserNr].PosMiddleX, this.cruiserReference[cruiserNr].PosMiddleY,
                        this.cruiserReference[cruiserNr].PosFrontX, this.cruiserReference[cruiserNr].PosFrontY
                        },
                        this.cruiserReference[cruiserNr].Horizontal);
                    this.ShowDestroyedCruiser(
                        new int[6]
                        {
                        this.cruiserReference[cruiserNr].PosRearX, this.cruiserReference[cruiserNr].PosRearY,
                        this.cruiserReference[cruiserNr].PosMiddleX, this.cruiserReference[cruiserNr].PosMiddleY,
                        this.cruiserReference[cruiserNr].PosFrontX, this.cruiserReference[cruiserNr].PosFrontY
                        },
                        this.cruiserReference[cruiserNr].Horizontal);
                    this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Galley_"))
            {
                // Was the galley hit?
                string[] shipGalley = this.playfieldStore[x, y].Name.Split('_');
                string galleyPart = shipGalley[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (galleyPart)
                {
                    case ("Rear"):
                        this.galleyReference.Rear = true;
                        this.SetTextLblStatus("Galley wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.galleyReference.MiddleFirstPart = true;
                        this.SetTextLblStatus("Galley wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.galleyReference.MiddleSecondPart = true;
                        this.SetTextLblStatus("Galley wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.galleyReference.Front = true;
                        this.SetTextLblStatus("Galley wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob die Galley komplett zerstört wurde
                if (this.galleyReference.Rear && this.galleyReference.MiddleFirstPart && this.galleyReference.MiddleSecondPart && this.galleyReference.Front)
                {
                    this.galleyReference.ShipDestryoed = true;
                    this.SetTextLblStatus("Galley destroyed!\n");
                }
            }
            else if (this.playfieldStore[x, y].Name.StartsWith("Battleship_"))
            {
            // Wurde das Battleship getroffen?
                string[] shipBattleship = this.playfieldStore[x, y].Name.Split('_');
                string battleshipPart = shipBattleship[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (battleshipPart)
                {
                    case ("Rear"):
                        this.battleshipReference.Rear = true;
                        this.SetTextLblStatus("Battleship wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.battleshipReference.MiddleFirstPart = true;
                        this.SetTextLblStatus("Battleship wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.battleshipReference.MiddleSecondPart = true;
                        this.SetTextLblStatus("Battleship wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.battleshipReference.Front = true;
                        this.SetTextLblStatus("Battleship wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Battleship komplett zerstört wurde
                if (this.battleshipReference.Rear && this.battleshipReference.MiddleFirstPart && this.battleshipReference.MiddleSecondPart && this.battleshipReference.Front)
                {
                    this.battleshipReference.ShipDestryoed = true;
                    this.SetTextLblStatus("Battleship destroyed!\n");
                }
            }
        }

        /// <summary>
        /// Decides which explosion is to be displayed in the panel.
        /// </summary>
        /// <param name="x">X coordinate of the hit.</param>
        /// <param name="y">Y coordinate of the hit.</param>
        public void DrawExplosion(int x, int y)
        {
            // PictureBox_DoubleBuffered explPicture = new PictureBox_DoubleBuffered();
            PictureBox explPicture = new PictureBox();
            explPicture.Name = "expl_" + x.ToString() + ":" + y.ToString();
            //// explPicture.Location = new Point(x * 30, y * 30);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explo6;
            this.AddControl(explPicture, x, y); // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
        }

        /// <summary>
        /// Sets the ship parts to the respective positions.
        /// (For a Mouse Click event).
        /// </summary>
        /// <param name="tmp">The Panel, which has thrown the MouseClick event (as a reference).</param>
        private void SetShips(ref Battleships.DoubleBufferedPanel tmp)
        {
            string positionString = tmp.Name;
            positionString = positionString.Remove(0, 3); // pf_ aus dem String entfernen
            string[] position = positionString.Split(':'); // x und y Position
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (this.Ships)
            {
                // galley
                case ShipModels.galley:
                    if (this.horizontal)
                    {
                        // horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != (int)1 && (int)this.pf[x - 1, y].Tag != (int)1 && (int)this.pf[x - 2, y].Tag != (int)1 && (int)this.pf[x - 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pf[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                                tmp.Tag = 1;
                                this.pf[x - 1, y].Tag = 1;
                                this.pf[x - 2, y].Tag = 1;
                                this.pf[x - 3, y].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosRearX = x;
                                this.galleyReference.PosRearY = y;
                                this.galleyReference.ShipDestryoed = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x - 2, y].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x - 3, y].Name = this.galleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x - 1, y].BackColor = this.collisionColor;
                                this.pf[x - 2, y].BackColor = this.collisionColor;
                                this.pf[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x - 1, y].BackColor = Color.Transparent;
                                this.pf[x - 2, y].BackColor = Color.Transparent;
                                this.pf[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != (int)1 && (int)this.pf[x + 1, y].Tag != (int)1 && (int)this.pf[x + 2, y].Tag != (int)1 && (int)this.pf[x + 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pf[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                                tmp.Tag = 1;
                                this.pf[x + 1, y].Tag = 1;
                                this.pf[x + 2, y].Tag = 1;
                                this.pf[x + 3, y].Tag = 1;

                                this.galleyReference.ShipName = "Galley";
                                this.galleyReference.PosRearX = x;
                                this.galleyReference.PosRearY = y;
                                this.galleyReference.ShipDestryoed = false;
                                this.galleyReference.Rear = false;
                                this.galleyReference.Front = false;
                                this.galleyReference.MiddleFirstPart = false;
                                this.galleyReference.MiddleSecondPart = false;
                                this.galleyReference.Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.galleyReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.galleyReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x + 2, y].Name = this.galleyReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x + 3, y].Name = this.galleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x + 1, y].BackColor = this.collisionColor;
                                this.pf[x + 2, y].BackColor = this.collisionColor;
                                this.pf[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x + 1, y].BackColor = Color.Transparent;
                                this.pf[x + 2, y].BackColor = Color.Transparent;
                                this.pf[x + 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1 && (int)pf[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pf[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                                tmp.Tag = 1;
                                pf[x, y - 1].Tag = 1;
                                pf[x, y - 2].Tag = 1;
                                pf[x, y - 3].Tag = 1;

                                galleyReference.ShipName = "Galley";
                                galleyReference.PosRearX = x;
                                galleyReference.PosRearY = y;
                                galleyReference.ShipDestryoed = false;
                                galleyReference.Rear = false;
                                galleyReference.Front = false;
                                galleyReference.MiddleFirstPart = false;
                                galleyReference.MiddleSecondPart = false;
                                galleyReference.Horizontal = horizontal;

                                playfieldStore[x, y].Name = galleyReference.ShipName + "_" + "Front";
                                playfieldStore[x, y - 1].Name = galleyReference.ShipName + "_" + "Middle2";
                                playfieldStore[x, y - 2].Name = galleyReference.ShipName + "_" + "Middle1";
                                playfieldStore[x, y - 3].Name = galleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y - 1].BackColor = collisionColor;
                                pf[x, y - 2].BackColor = collisionColor;
                                pf[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y - 1].BackColor = Color.Transparent;
                                pf[x, y - 2].BackColor = Color.Transparent;
                                pf[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1 && (int)pf[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pf[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                                tmp.Tag = 1;
                                pf[x, y + 1].Tag = 1;
                                pf[x, y + 2].Tag = 1;
                                pf[x, y + 3].Tag = 1;

                                galleyReference.ShipName = "Galley";
                                galleyReference.PosRearX = x;
                                galleyReference.PosRearY = y;
                                galleyReference.ShipDestryoed = false;
                                galleyReference.Rear = false;
                                galleyReference.Front = false;
                                galleyReference.MiddleFirstPart = false;
                                galleyReference.MiddleSecondPart = false;
                                galleyReference.Horizontal = horizontal;

                                playfieldStore[x, y].Name = galleyReference.ShipName + "_" + "Rear";
                                playfieldStore[x, y + 1].Name = galleyReference.ShipName + "_" + "Middle1";
                                playfieldStore[x, y + 2].Name = galleyReference.ShipName + "_" + "Middle2";
                                playfieldStore[x, y + 3].Name = galleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y + 1].BackColor = collisionColor;
                                pf[x, y + 2].BackColor = collisionColor;
                                pf[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y + 1].BackColor = Color.Transparent;
                                pf[x, y + 2].BackColor = Color.Transparent;
                                pf[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.battleship: // battleship
                    if (this.horizontal)
                    {
                        // horizontal
                        // battleship ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1 && (int)this.pf[x - 2, y].Tag != 1 && (int)this.pf[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pf[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                                tmp.Tag = 1;
                                this.pf[x - 1, y].Tag = 1;
                                this.pf[x - 2, y].Tag = 1;
                                this.pf[x - 3, y].Tag = 1;

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosRearX = x;
                                this.battleshipReference.PosRearY = y;
                                this.battleshipReference.ShipDestryoed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x - 2, y].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x - 3, y].Name = this.battleshipReference.ShipName + "_" + "Rear";
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x - 1, y].BackColor = this.collisionColor;
                                this.pf[x - 2, y].BackColor = this.collisionColor;
                                this.pf[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x - 1, y].BackColor = Color.Transparent;
                                this.pf[x - 2, y].BackColor = Color.Transparent;
                                this.pf[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1 && (int)this.pf[x + 2, y].Tag != 1 && (int)this.pf[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pf[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                                tmp.Tag = 1;
                                this.pf[x + 1, y].Tag = 1;
                                this.pf[x + 2, y].Tag = 1;
                                this.pf[x + 3, y].Tag = 1;

                                this.battleshipReference.ShipName = "Battleship";
                                this.battleshipReference.PosRearX = x;
                                this.battleshipReference.PosRearY = y;
                                this.battleshipReference.ShipDestryoed = false;
                                this.battleshipReference.Rear = false;
                                this.battleshipReference.Front = false;
                                this.battleshipReference.MiddleFirstPart = false;
                                this.battleshipReference.MiddleSecondPart = false;
                                this.battleshipReference.Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.battleshipReference.ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.battleshipReference.ShipName + "_" + "Middle1";
                                this.playfieldStore[x + 2, y].Name = this.battleshipReference.ShipName + "_" + "Middle2";
                                this.playfieldStore[x + 3, y].Name = this.battleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x + 1, y].BackColor = this.collisionColor;
                                this.pf[x + 2, y].BackColor = this.collisionColor;
                                this.pf[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x + 1, y].BackColor = Color.Transparent;
                                this.pf[x + 2, y].BackColor = Color.Transparent;
                                this.pf[x + 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // battleship ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1 && (int)pf[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pf[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                                tmp.Tag = 1;
                                pf[x, y - 1].Tag = 1;
                                pf[x, y - 2].Tag = 1;
                                pf[x, y - 3].Tag = 1;

                                battleshipReference.ShipName = "Battleship";
                                battleshipReference.PosRearX = x;
                                battleshipReference.PosRearY = y;
                                battleshipReference.ShipDestryoed = false;
                                battleshipReference.Rear = false;
                                battleshipReference.Front = false;
                                battleshipReference.MiddleFirstPart = false;
                                battleshipReference.MiddleSecondPart = false;
                                battleshipReference.Horizontal = horizontal;

                                playfieldStore[x, y].Name = battleshipReference.ShipName + "_" + "Front";
                                playfieldStore[x, y - 1].Name = battleshipReference.ShipName + "_" + "Middle2";
                                playfieldStore[x, y - 2].Name = battleshipReference.ShipName + "_" + "Middle1";
                                playfieldStore[x, y - 3].Name = battleshipReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y - 1].BackColor = collisionColor;
                                pf[x, y - 2].BackColor = collisionColor;
                                pf[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y - 1].BackColor = Color.Transparent;
                                pf[x, y - 2].BackColor = Color.Transparent;
                                pf[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1 && (int)pf[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pf[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                                tmp.Tag = 1;
                                pf[x, y + 1].Tag = 1;
                                pf[x, y + 2].Tag = 1;
                                pf[x, y + 3].Tag = 1;

                                battleshipReference.ShipName = "Battleship";
                                battleshipReference.PosRearX = x;
                                battleshipReference.PosRearY = y;
                                battleshipReference.ShipDestryoed = false;
                                battleshipReference.Rear = false;
                                battleshipReference.Front = false;
                                battleshipReference.MiddleFirstPart = false;
                                battleshipReference.MiddleSecondPart = false;
                                battleshipReference.Horizontal = horizontal;

                                playfieldStore[x, y].Name = battleshipReference.ShipName + "_" + "Rear";
                                playfieldStore[x, y + 1].Name = battleshipReference.ShipName + "_" + "Middle1";
                                playfieldStore[x, y + 2].Name = battleshipReference.ShipName + "_" + "Middle2";
                                playfieldStore[x, y + 3].Name = battleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y + 1].BackColor = collisionColor;
                                pf[x, y + 2].BackColor = collisionColor;
                                pf[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y + 1].BackColor = Color.Transparent;
                                pf[x, y + 2].BackColor = Color.Transparent;
                                pf[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.cruiser: // Cruiser
                    if (this.horizontal)
                    {
                        // horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1 && (int)this.pf[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                tmp.Tag = 1;
                                this.pf[x - 1, y].Tag = 1;
                                this.pf[x - 2, y].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.cruiserReference[this.CounterCruiser].PosRearX = x;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y;
                                this.cruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x - 2, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x - 1, y].BackColor = this.collisionColor;
                                this.pf[x - 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x - 1, y].BackColor = Color.Transparent;
                                this.pf[x - 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1 && (int)this.pf[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                tmp.Tag = 1;
                                this.pf[x + 1, y].Tag = 1;
                                this.pf[x + 2, y].Tag = 1;

                                this.cruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.cruiserReference[this.CounterCruiser].PosRearX = x;
                                this.cruiserReference[this.CounterCruiser].PosRearY = y;
                                this.cruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.cruiserReference[this.CounterCruiser].Front = false;
                                this.cruiserReference[this.CounterCruiser].Rear = false;
                                this.cruiserReference[this.CounterCruiser].Middle = false;
                                this.cruiserReference[this.CounterCruiser].Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.playfieldStore[x + 2, y].Name = this.cruiserReference[this.CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x + 1, y].BackColor = this.collisionColor;
                                this.pf[x + 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x + 1, y].BackColor = Color.Transparent;
                                this.pf[x + 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // Cruiser ist 5 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                tmp.Tag = 1;
                                pf[x, y - 1].Tag = 1;
                                pf[x, y - 2].Tag = 1;

                                cruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                cruiserReference[CounterCruiser].PosRearX = x;
                                cruiserReference[CounterCruiser].PosRearY = y;
                                cruiserReference[CounterCruiser].ShipDestryoed = false;
                                cruiserReference[CounterCruiser].Front = false;
                                cruiserReference[CounterCruiser].Rear = false;
                                cruiserReference[CounterCruiser].Middle = false;
                                cruiserReference[CounterCruiser].Horizontal = horizontal;

                                playfieldStore[x, y].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Front";
                                playfieldStore[x, y - 1].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                playfieldStore[x, y - 2].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y - 1].BackColor = collisionColor;
                                pf[x, y - 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y - 1].BackColor = Color.Transparent;
                                pf[x, y - 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                tmp.Tag = 1;
                                pf[x, y + 1].Tag = 1;
                                pf[x, y + 2].Tag = 1;

                                cruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                cruiserReference[CounterCruiser].PosRearX = x;
                                cruiserReference[CounterCruiser].PosRearY = y;
                                cruiserReference[CounterCruiser].ShipDestryoed = false;
                                cruiserReference[CounterCruiser].Front = false;
                                cruiserReference[CounterCruiser].Rear = false;
                                cruiserReference[CounterCruiser].Middle = false;
                                cruiserReference[CounterCruiser].Horizontal = horizontal;

                                playfieldStore[x, y].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Rear";
                                playfieldStore[x, y + 1].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                playfieldStore[x, y + 2].Name = cruiserReference[CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y + 1].BackColor = collisionColor;
                                pf[x, y + 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y + 1].BackColor = Color.Transparent;
                                pf[x, y + 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.boat: // boat
                    if (this.horizontal)
                    {
                        // horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                                tmp.Tag = 1;
                                this.pf[x - 1, y].Tag = 1;

                                // Position sowie name des Schiffes speichern
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString();
                                this.boatReference[this.CounterBoat].PosRearX = x;
                                this.boatReference[this.CounterBoat].PosRearY = y;
                                this.boatReference[this.CounterBoat].PosFrontX = x - 1;
                                this.boatReference[this.CounterBoat].PosFrontY = y;
                                this.boatReference[this.CounterBoat].ShipDestryoed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].Horizontal = this.horizontal;

                                this.playfieldStore[x, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.playfieldStore[x - 1, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x - 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x - 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1)
                            {
                                // Otherwise assemble the boat in the normal direction (2 fields)
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                                tmp.Tag = 1;
                                this.pf[x + 1, y].Tag = 1;
                                this.boatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                this.boatReference[this.CounterBoat].PosRearX = x;
                                this.boatReference[this.CounterBoat].PosRearY = y;
                                this.boatReference[this.CounterBoat].PosFrontX = x + 1;
                                this.boatReference[this.CounterBoat].PosFrontY = y;
                                this.boatReference[this.CounterBoat].ShipDestryoed = false;
                                this.boatReference[this.CounterBoat].Front = false;
                                this.boatReference[this.CounterBoat].Rear = false;
                                this.boatReference[this.CounterBoat].Horizontal = this.horizontal;
                                this.playfieldStore[x, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Rear";
                                this.playfieldStore[x + 1, y].Name = this.boatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.Ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pf[x + 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pf[x + 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // boat ist 2 Fleder groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                                tmp.Tag = 1;
                                pf[x, y - 1].Tag = 1;
                                boatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                boatReference[CounterBoat].PosRearX = x;
                                boatReference[CounterBoat].PosRearY = y;
                                boatReference[CounterBoat].PosFrontX = x;
                                boatReference[CounterBoat].PosFrontY = y - 1;
                                boatReference[CounterBoat].ShipDestryoed = false;
                                boatReference[CounterBoat].Front = false;
                                boatReference[CounterBoat].Rear = false;
                                boatReference[CounterBoat].Horizontal = horizontal;
                                playfieldStore[x, y].Name = boatReference[CounterBoat].ShipName + "_" + "Front";
                                playfieldStore[x, y - 1].Name = boatReference[CounterBoat].ShipName + "_" + "Rear";
                                Ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y - 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y - 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                                tmp.Tag = 1;
                                pf[x, y + 1].Tag = 1;

                                // Position sowie name des schiffes speichern
                                boatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString();
                                boatReference[CounterBoat].PosRearX = x;
                                boatReference[CounterBoat].PosRearY = y;
                                boatReference[CounterBoat].PosFrontX = x;
                                boatReference[CounterBoat].PosFrontY = y + 1;
                                boatReference[CounterBoat].ShipDestryoed = false;
                                boatReference[CounterBoat].Front = false;
                                boatReference[CounterBoat].Rear = false;
                                boatReference[CounterBoat].Horizontal = horizontal;

                                playfieldStore[x, y].Name = boatReference[CounterBoat].ShipName + "_" + "Rear";
                                playfieldStore[x, y + 1].Name = boatReference[CounterBoat].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pf[x, y + 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pf[x, y + 1].BackColor = Color.Transparent;
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
            positionString = positionString.Remove(0, 3); // pf_ aus dem String entfernen
            string[] position = positionString.Split(':'); // x und y Position
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (this.Ships)
            {
                case ShipModels.galley: // galley
                    if (this.horizontal)
                    {
                        // horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1 && (int)this.pf[x - 2, y].Tag != 1 && (int)this.pf[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pf[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1 && (int)this.pf[x + 2, y].Tag != 1 && (int)this.pf[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pf[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1 && (int)pf[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pf[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1 && (int)pf[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pf[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                            }
                        }
                    }

                    break;
                case ShipModels.battleship: // battleship
                    if (this.horizontal)
                    {
                        // horizontal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1 && (int)this.pf[x - 2, y].Tag != 1 && (int)this.pf[x - 3, y].Tag != 1 && (int)this.pf[x - 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pf[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1 && (int)this.pf[x + 2, y].Tag != 1 && (int)this.pf[x + 3, y].Tag != 1 && (int)this.pf[x + 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pf[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1 && (int)pf[x, y - 3].Tag != 1 && (int)pf[x, y - 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pf[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1 && (int)pf[x, y + 3].Tag != 1 && (int)pf[x, y + 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pf[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                            }
                        }
                    }

                    break;
                case ShipModels.cruiser: // Cruiser
                    if (this.horizontal)
                    {
                        // horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1 && (int)this.pf[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pf[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1 && (int)this.pf[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pf[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // Cruiser ist 5 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1 && (int)pf[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pf[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1 && (int)pf[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pf[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                            }
                        }
                    }

                    break;
                case ShipModels.boat: // boat
                    if (this.horizontal)
                    {
                        // horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pf[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.pf[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                            }
                        }
                        else
                        {
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pf[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.pf[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // boat ist 2 Fleder groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                pf[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pf[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                pf[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                            }
                        }
                    }

                    break;
            }
        }

        protected virtual void SetTextLblStatus(string text)
        {
            if (BattleshipsForm.LabelStatus.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(this.SetTextLblStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                BattleshipsForm.LabelStatus.Text += text;
                BattleshipsForm.PanelStatus.VerticalScroll.Value += BattleshipsForm.PanelStatus.VerticalScroll.SmallChange;
                BattleshipsForm.PanelStatus.Refresh();
            }
        }

        protected virtual void AddControl(Control contr, int x, int y)
        {
            if (this.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(this.AddControl);
                this.Invoke(d, new object[] { contr, x, y });
            }
            else
            {
                // Die Explosion dem Panel zuordnen, in dem der Treffer war (Die Explosion wird somit vor dem Panelbild angezeigt)
                this.pf[x, y].Controls.Add(contr);
                //// Control[] s = this.Controls.Find(contr.ShipName, false);
                //// Die PictureBox in den Fordergrund bringen
                //// s[0].BringToFront();
            }
        }
    }
}