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
        public Ships.Boat[] BoatReference = new Ships.Boat[3];                        // 3 Boote
        public Ships.Cruiser[] CruiserReference = new Ships.Cruiser[3];               // 3 Cruiser
        public Ships.Galley GalleyReference = new Ships.Galley();                     // 1 Galley
        public Ships.Battleship BattleshipReference = new Ships.Battleship();         // 1 Schlachtschiff
        public int CounterGalley = 0;
        public int CounterBattleship = 0;
        public int CounterCruiser = 0;
        public int CounterBoat = 0;
        
        /// <summary>
        /// Contains a collection of ship models.
        /// </summary>
        public ShipModels Ships;

        /// <summary>
        /// Contains the playing field and all ships.
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] Pb = new Battleships.DoubleBufferedPanel[10, 10];

        /// <summary>
        /// Contains a shadow copy of the playing field.
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] PbStore = new Battleships.DoubleBufferedPanel[10, 10];

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
            for (int i = 0; i < this.Pb.GetLength(0); i++)
            {
                for (int j = 0; j < this.Pb.GetLength(1); j++)
                {
                    Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Padding = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.MouseClick += new MouseEventHandler(this.PlayerMouseClick);
                    p.MouseEnter += new EventHandler(this.PlayerMouseEnter);
                    p.MouseLeave += new EventHandler(this.PlayerMouseLeave);
                    //// p.Visible = false;
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.Pb[i, j] = p;
                    this.Controls.Add(p);

                    this.PbStore[i, j] = new Battleships.DoubleBufferedPanel();
                }
            }
        }

        #region Mouse-Events
        private void PlayerMouseClick(object sender, MouseEventArgs e)
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
                for (int i = 0; i < this.Pb.GetLength(0); i++)
                {
                    for (int j = 0; j < this.Pb.GetLength(1); j++)
                    {
                        // Prüfen ob Feld ein Schiffsteil beihnaltet (Tag = 1)
                        if ((int)this.Pb[i, j].Tag != (int)1)
                        {
                            // Wenn nein, dann Bild im Feld löschen
                            this.Pb[i, j].BackgroundImage = null;
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
            for (int x = 0; x < this.Pb.GetLength(0); x++)
            {
                for (int y = 0; y < this.Pb.GetLength(1); y++)
                {
                    // If no image is saved set current panel
                    if ((int)this.Pb[x, y].Tag != (int)1)
                    {
                        this.Pb[x, y].BackgroundImage = null; // Dann Bild löschen
                        this.Pb[x, y].Tag = 0; // Bildflag auf false
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
            if (this.Pb[x, y].BackgroundImage == null)
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
            this.Pb[x, y].BackgroundImage = Properties.Resources.splash;
            this.Pb[x, y].Tag = 1;
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
                this.Pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.Pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                if (horizontal)
                {
                    this.Pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h2;
                    this.Pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                }
                else
                {
                    this.Pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                    this.Pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
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
                this.Pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.Pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.Pb[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

                if (horizontal)
                {
                    this.Pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_h1;
                    this.Pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_h2;
                    this.Pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_h3;
                }
                else
                {
                    this.Pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_v1;
                    this.Pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_v2;
                    this.Pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_v3;
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
            for (int i = 0; i < this.BoatReference.Length; i++)
            {
                // Überprüfen ob alle Boote zerstört sind
                if (!this.BoatReference[i].ShipDestryoed)
                {
                    // Wenn auch nur noch ein Boot existiert, dann ist das Spiel nocht nicht vorbei
                    return false;
                }
            }
            
            // Alle Cruiser überprüfen
            for (int i = 0; i < this.CruiserReference.Length; i++)
            {
                // Überprüfen ob alle Cruiser zerstört sind
                if (!this.CruiserReference[i].ShipDestryoed)
                { 
                    // Wenn auch nur noch ein Cruiser existiert, dann ist das Spiel noch nicht vorbei
                    return false;
                }
            }

             // Die Galley überprüfen
            if (!this.GalleyReference.ShipDestryoed)
            {
                // Existiert die Galley noch, dann ist das Spiel nocht nicht vorbei
                return false;
            }

            // Das Battleship überprüfen
            if (!this.BattleshipReference.ShipDestryoed)
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
            if (this.PbStore[x, y].Name.StartsWith("Boat_"))
            {
                // Die Nr. des Bootes herausfinden (von 1-3)
                string[] shipBoat = this.PbStore[x, y].Name.Split('_');
                int boatNr = int.Parse(shipBoat[1]);
                string boatPart = shipBoat[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (boatPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (horizontal)
                        this.BoatReference[boatNr].Rear = true;
                        this.SetTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (horizontal)
                        this.BoatReference[boatNr].Front = true;
                        this.SetTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Boot komplett zerstört ist
                if (this.BoatReference[boatNr].Front && this.BoatReference[boatNr].Rear)
                {
                    this.BoatReference[boatNr].ShipDestryoed = true;
                    this.ShowDestroyedBoat( // Das zerstörte Boot auf dem Spielfeld darstellen
                        new int[4]
                        {
                        this.BoatReference[boatNr].PosRearX, this.BoatReference[boatNr].PosRearY,
                        this.BoatReference[boatNr].PosFrontX, this.BoatReference[boatNr].PosFrontY
                        },
                        this.BoatReference[boatNr].Horizontal);
                    this.ShowDestroyedBoat(
                        new int[4]
                        {
                        this.BoatReference[boatNr].PosRearX, this.BoatReference[boatNr].PosRearY,
                        this.BoatReference[boatNr].PosFrontX, this.BoatReference[boatNr].PosFrontY
                        },
                        this.BoatReference[boatNr].Horizontal);
                    this.SetTextLblStatus("\nBoat Nr. " + boatNr.ToString() + " destroyed!");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Cruiser_"))
            {
            // Wurde ein Cruiser getroffen?
                string[] shipCruiser = this.PbStore[x, y].Name.Split('_');
                int cruiserNr = int.Parse(shipCruiser[1]);
                string cruiserPart = shipCruiser[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (cruiserPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (horizontal)
                        this.CruiserReference[cruiserNr].Rear = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Middle"):
                        // Mittelteil wurde getroffen
                        this.CruiserReference[cruiserNr].Middle = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Mittelteil getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (horizontal)
                        this.CruiserReference[cruiserNr].Front = true;
                        this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob der Cruiser komplett zerstört wurde
                if (this.CruiserReference[cruiserNr].Rear && this.CruiserReference[cruiserNr].Middle && this.CruiserReference[cruiserNr].Front)
                {
                    this.CruiserReference[cruiserNr].ShipDestryoed = true;
                    
                    this.ShowDestroyedCruiser( // Den zerstörten Cruiser auf dem Spielfeld darstellen
                        new int[6]
                        {
                        this.CruiserReference[cruiserNr].PosRearX, this.CruiserReference[cruiserNr].PosRearY,
                        this.CruiserReference[cruiserNr].PosMiddleX, this.CruiserReference[cruiserNr].PosMiddleY,
                        this.CruiserReference[cruiserNr].PosFrontX, this.CruiserReference[cruiserNr].PosFrontY
                        },
                        this.CruiserReference[cruiserNr].Horizontal);
                    this.ShowDestroyedCruiser(
                        new int[6]
                        {
                        this.CruiserReference[cruiserNr].PosRearX, this.CruiserReference[cruiserNr].PosRearY,
                        this.CruiserReference[cruiserNr].PosMiddleX, this.CruiserReference[cruiserNr].PosMiddleY,
                        this.CruiserReference[cruiserNr].PosFrontX, this.CruiserReference[cruiserNr].PosFrontY
                        },
                        this.CruiserReference[cruiserNr].Horizontal);
                    this.SetTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Galley_"))
            {
                // Was the galley hit?
                string[] shipGalley = this.PbStore[x, y].Name.Split('_');
                string galleyPart = shipGalley[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (galleyPart)
                {
                    case ("Rear"):
                        this.GalleyReference.Rear = true;
                        this.SetTextLblStatus("Galley wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.GalleyReference.MiddleFirstPart = true;
                        this.SetTextLblStatus("Galley wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.GalleyReference.MiddleSecondPart = true;
                        this.SetTextLblStatus("Galley wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.GalleyReference.Front = true;
                        this.SetTextLblStatus("Galley wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob die Galley komplett zerstört wurde
                if (this.GalleyReference.Rear && this.GalleyReference.MiddleFirstPart && this.GalleyReference.MiddleSecondPart && this.GalleyReference.Front)
                {
                    this.GalleyReference.ShipDestryoed = true;
                    this.SetTextLblStatus("Galley destroyed!\n");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Battleship_"))
            {
            // Wurde das Battleship getroffen?
                string[] shipBattleship = this.PbStore[x, y].Name.Split('_');
                string battleshipPart = shipBattleship[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (battleshipPart)
                {
                    case ("Rear"):
                        this.BattleshipReference.Rear = true;
                        this.SetTextLblStatus("Battleship wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.BattleshipReference.MiddleFirstPart = true;
                        this.SetTextLblStatus("Battleship wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.BattleshipReference.MiddleSecondPart = true;
                        this.SetTextLblStatus("Battleship wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.BattleshipReference.Front = true;
                        this.SetTextLblStatus("Battleship wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Battleship komplett zerstört wurde
                if (this.BattleshipReference.Rear && this.BattleshipReference.MiddleFirstPart && this.BattleshipReference.MiddleSecondPart && this.BattleshipReference.Front)
                {
                    this.BattleshipReference.ShipDestryoed = true;
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
            positionString = positionString.Remove(0, 3); // pb_ aus dem String entfernen
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
                            if ((int)tmp.Tag != (int)1 && (int)this.Pb[x - 1, y].Tag != (int)1 && (int)this.Pb[x - 2, y].Tag != (int)1 && (int)this.Pb[x - 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.Pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                                tmp.Tag = 1;
                                this.Pb[x - 1, y].Tag = 1;
                                this.Pb[x - 2, y].Tag = 1;
                                this.Pb[x - 3, y].Tag = 1;

                                this.GalleyReference.ShipName = "Galley";
                                this.GalleyReference.PosRearX = x;
                                this.GalleyReference.PosRearY = y;
                                this.GalleyReference.ShipDestryoed = false;
                                this.GalleyReference.Rear = false;
                                this.GalleyReference.Front = false;
                                this.GalleyReference.MiddleFirstPart = false;
                                this.GalleyReference.MiddleSecondPart = false;
                                this.GalleyReference.Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.GalleyReference.ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.GalleyReference.ShipName + "_" + "Middle2";
                                this.PbStore[x - 2, y].Name = this.GalleyReference.ShipName + "_" + "Middle1";
                                this.PbStore[x - 3, y].Name = this.GalleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x - 1, y].BackColor = this.collisionColor;
                                this.Pb[x - 2, y].BackColor = this.collisionColor;
                                this.Pb[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x - 1, y].BackColor = Color.Transparent;
                                this.Pb[x - 2, y].BackColor = Color.Transparent;
                                this.Pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != (int)1 && (int)this.Pb[x + 1, y].Tag != (int)1 && (int)this.Pb[x + 2, y].Tag != (int)1 && (int)this.Pb[x + 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.Pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                                tmp.Tag = 1;
                                this.Pb[x + 1, y].Tag = 1;
                                this.Pb[x + 2, y].Tag = 1;
                                this.Pb[x + 3, y].Tag = 1;

                                this.GalleyReference.ShipName = "Galley";
                                this.GalleyReference.PosRearX = x;
                                this.GalleyReference.PosRearY = y;
                                this.GalleyReference.ShipDestryoed = false;
                                this.GalleyReference.Rear = false;
                                this.GalleyReference.Front = false;
                                this.GalleyReference.MiddleFirstPart = false;
                                this.GalleyReference.MiddleSecondPart = false;
                                this.GalleyReference.Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.GalleyReference.ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.GalleyReference.ShipName + "_" + "Middle1";
                                this.PbStore[x + 2, y].Name = this.GalleyReference.ShipName + "_" + "Middle2";
                                this.PbStore[x + 3, y].Name = this.GalleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x + 1, y].BackColor = this.collisionColor;
                                this.Pb[x + 2, y].BackColor = this.collisionColor;
                                this.Pb[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x + 1, y].BackColor = Color.Transparent;
                                this.Pb[x + 2, y].BackColor = Color.Transparent;
                                this.Pb[x + 3, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1 && (int)Pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                Pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                                tmp.Tag = 1;
                                Pb[x, y - 1].Tag = 1;
                                Pb[x, y - 2].Tag = 1;
                                Pb[x, y - 3].Tag = 1;

                                GalleyReference.ShipName = "Galley";
                                GalleyReference.PosRearX = x;
                                GalleyReference.PosRearY = y;
                                GalleyReference.ShipDestryoed = false;
                                GalleyReference.Rear = false;
                                GalleyReference.Front = false;
                                GalleyReference.MiddleFirstPart = false;
                                GalleyReference.MiddleSecondPart = false;
                                GalleyReference.Horizontal = horizontal;

                                PbStore[x, y].Name = GalleyReference.ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = GalleyReference.ShipName + "_" + "Middle2";
                                PbStore[x, y - 2].Name = GalleyReference.ShipName + "_" + "Middle1";
                                PbStore[x, y - 3].Name = GalleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y - 1].BackColor = collisionColor;
                                Pb[x, y - 2].BackColor = collisionColor;
                                Pb[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y - 1].BackColor = Color.Transparent;
                                Pb[x, y - 2].BackColor = Color.Transparent;
                                Pb[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1 && (int)Pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                Pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                                tmp.Tag = 1;
                                Pb[x, y + 1].Tag = 1;
                                Pb[x, y + 2].Tag = 1;
                                Pb[x, y + 3].Tag = 1;

                                GalleyReference.ShipName = "Galley";
                                GalleyReference.PosRearX = x;
                                GalleyReference.PosRearY = y;
                                GalleyReference.ShipDestryoed = false;
                                GalleyReference.Rear = false;
                                GalleyReference.Front = false;
                                GalleyReference.MiddleFirstPart = false;
                                GalleyReference.MiddleSecondPart = false;
                                GalleyReference.Horizontal = horizontal;

                                PbStore[x, y].Name = GalleyReference.ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = GalleyReference.ShipName + "_" + "Middle1";
                                PbStore[x, y + 2].Name = GalleyReference.ShipName + "_" + "Middle2";
                                PbStore[x, y + 3].Name = GalleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y + 1].BackColor = collisionColor;
                                Pb[x, y + 2].BackColor = collisionColor;
                                Pb[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y + 1].BackColor = Color.Transparent;
                                Pb[x, y + 2].BackColor = Color.Transparent;
                                Pb[x, y + 3].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1 && (int)this.Pb[x - 2, y].Tag != 1 && (int)this.Pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.Pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                                tmp.Tag = 1;
                                this.Pb[x - 1, y].Tag = 1;
                                this.Pb[x - 2, y].Tag = 1;
                                this.Pb[x - 3, y].Tag = 1;

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;

                                this.BattleshipReference.ShipName = "Battleship";
                                this.BattleshipReference.PosRearX = x;
                                this.BattleshipReference.PosRearY = y;
                                this.BattleshipReference.ShipDestryoed = false;
                                this.BattleshipReference.Rear = false;
                                this.BattleshipReference.Front = false;
                                this.BattleshipReference.MiddleFirstPart = false;
                                this.BattleshipReference.MiddleSecondPart = false;
                                this.BattleshipReference.Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.BattleshipReference.ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.BattleshipReference.ShipName + "_" + "Middle2";
                                this.PbStore[x - 2, y].Name = this.BattleshipReference.ShipName + "_" + "Middle1";
                                this.PbStore[x - 3, y].Name = this.BattleshipReference.ShipName + "_" + "Rear";
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x - 1, y].BackColor = this.collisionColor;
                                this.Pb[x - 2, y].BackColor = this.collisionColor;
                                this.Pb[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x - 1, y].BackColor = Color.Transparent;
                                this.Pb[x - 2, y].BackColor = Color.Transparent;
                                this.Pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1 && (int)this.Pb[x + 2, y].Tag != 1 && (int)this.Pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.Pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                                tmp.Tag = 1;
                                this.Pb[x + 1, y].Tag = 1;
                                this.Pb[x + 2, y].Tag = 1;
                                this.Pb[x + 3, y].Tag = 1;

                                this.BattleshipReference.ShipName = "Battleship";
                                this.BattleshipReference.PosRearX = x;
                                this.BattleshipReference.PosRearY = y;
                                this.BattleshipReference.ShipDestryoed = false;
                                this.BattleshipReference.Rear = false;
                                this.BattleshipReference.Front = false;
                                this.BattleshipReference.MiddleFirstPart = false;
                                this.BattleshipReference.MiddleSecondPart = false;
                                this.BattleshipReference.Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.BattleshipReference.ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.BattleshipReference.ShipName + "_" + "Middle1";
                                this.PbStore[x + 2, y].Name = this.BattleshipReference.ShipName + "_" + "Middle2";
                                this.PbStore[x + 3, y].Name = this.BattleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x + 1, y].BackColor = this.collisionColor;
                                this.Pb[x + 2, y].BackColor = this.collisionColor;
                                this.Pb[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x + 1, y].BackColor = Color.Transparent;
                                this.Pb[x + 2, y].BackColor = Color.Transparent;
                                this.Pb[x + 3, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1 && (int)Pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                Pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                                tmp.Tag = 1;
                                Pb[x, y - 1].Tag = 1;
                                Pb[x, y - 2].Tag = 1;
                                Pb[x, y - 3].Tag = 1;

                                BattleshipReference.ShipName = "Battleship";
                                BattleshipReference.PosRearX = x;
                                BattleshipReference.PosRearY = y;
                                BattleshipReference.ShipDestryoed = false;
                                BattleshipReference.Rear = false;
                                BattleshipReference.Front = false;
                                BattleshipReference.MiddleFirstPart = false;
                                BattleshipReference.MiddleSecondPart = false;
                                BattleshipReference.Horizontal = horizontal;

                                PbStore[x, y].Name = BattleshipReference.ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = BattleshipReference.ShipName + "_" + "Middle2";
                                PbStore[x, y - 2].Name = BattleshipReference.ShipName + "_" + "Middle1";
                                PbStore[x, y - 3].Name = BattleshipReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y - 1].BackColor = collisionColor;
                                Pb[x, y - 2].BackColor = collisionColor;
                                Pb[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y - 1].BackColor = Color.Transparent;
                                Pb[x, y - 2].BackColor = Color.Transparent;
                                Pb[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1 && (int)Pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                Pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                                tmp.Tag = 1;
                                Pb[x, y + 1].Tag = 1;
                                Pb[x, y + 2].Tag = 1;
                                Pb[x, y + 3].Tag = 1;

                                BattleshipReference.ShipName = "Battleship";
                                BattleshipReference.PosRearX = x;
                                BattleshipReference.PosRearY = y;
                                BattleshipReference.ShipDestryoed = false;
                                BattleshipReference.Rear = false;
                                BattleshipReference.Front = false;
                                BattleshipReference.MiddleFirstPart = false;
                                BattleshipReference.MiddleSecondPart = false;
                                BattleshipReference.Horizontal = horizontal;

                                PbStore[x, y].Name = BattleshipReference.ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = BattleshipReference.ShipName + "_" + "Middle1";
                                PbStore[x, y + 2].Name = BattleshipReference.ShipName + "_" + "Middle2";
                                PbStore[x, y + 3].Name = BattleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y + 1].BackColor = collisionColor;
                                Pb[x, y + 2].BackColor = collisionColor;
                                Pb[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y + 1].BackColor = Color.Transparent;
                                Pb[x, y + 2].BackColor = Color.Transparent;
                                Pb[x, y + 3].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1 && (int)this.Pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                tmp.Tag = 1;
                                this.Pb[x - 1, y].Tag = 1;
                                this.Pb[x - 2, y].Tag = 1;

                                this.CruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.CruiserReference[this.CounterCruiser].PosRearX = x;
                                this.CruiserReference[this.CounterCruiser].PosRearY = y;
                                this.CruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.CruiserReference[this.CounterCruiser].Front = false;
                                this.CruiserReference[this.CounterCruiser].Rear = false;
                                this.CruiserReference[this.CounterCruiser].Middle = false;
                                this.CruiserReference[this.CounterCruiser].Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.PbStore[x - 2, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x - 1, y].BackColor = this.collisionColor;
                                this.Pb[x - 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x - 1, y].BackColor = Color.Transparent;
                                this.Pb[x - 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1 && (int)this.Pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                tmp.Tag = 1;
                                this.Pb[x + 1, y].Tag = 1;
                                this.Pb[x + 2, y].Tag = 1;

                                this.CruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.CruiserReference[this.CounterCruiser].PosRearX = x;
                                this.CruiserReference[this.CounterCruiser].PosRearY = y;
                                this.CruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.CruiserReference[this.CounterCruiser].Front = false;
                                this.CruiserReference[this.CounterCruiser].Rear = false;
                                this.CruiserReference[this.CounterCruiser].Middle = false;
                                this.CruiserReference[this.CounterCruiser].Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.PbStore[x + 2, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x + 1, y].BackColor = this.collisionColor;
                                this.Pb[x + 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x + 1, y].BackColor = Color.Transparent;
                                this.Pb[x + 2, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                tmp.Tag = 1;
                                Pb[x, y - 1].Tag = 1;
                                Pb[x, y - 2].Tag = 1;

                                CruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                CruiserReference[CounterCruiser].PosRearX = x;
                                CruiserReference[CounterCruiser].PosRearY = y;
                                CruiserReference[CounterCruiser].ShipDestryoed = false;
                                CruiserReference[CounterCruiser].Front = false;
                                CruiserReference[CounterCruiser].Rear = false;
                                CruiserReference[CounterCruiser].Middle = false;
                                CruiserReference[CounterCruiser].Horizontal = horizontal;

                                PbStore[x, y].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                PbStore[x, y - 2].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y - 1].BackColor = collisionColor;
                                Pb[x, y - 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y - 1].BackColor = Color.Transparent;
                                Pb[x, y - 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                tmp.Tag = 1;
                                Pb[x, y + 1].Tag = 1;
                                Pb[x, y + 2].Tag = 1;

                                CruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                CruiserReference[CounterCruiser].PosRearX = x;
                                CruiserReference[CounterCruiser].PosRearY = y;
                                CruiserReference[CounterCruiser].ShipDestryoed = false;
                                CruiserReference[CounterCruiser].Front = false;
                                CruiserReference[CounterCruiser].Rear = false;
                                CruiserReference[CounterCruiser].Middle = false;
                                CruiserReference[CounterCruiser].Horizontal = horizontal;

                                PbStore[x, y].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                PbStore[x, y + 2].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y + 1].BackColor = collisionColor;
                                Pb[x, y + 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y + 1].BackColor = Color.Transparent;
                                Pb[x, y + 2].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                                tmp.Tag = 1;
                                this.Pb[x - 1, y].Tag = 1;

                                // Position sowie name des Schiffes speichern
                                this.BoatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString();
                                this.BoatReference[this.CounterBoat].PosRearX = x;
                                this.BoatReference[this.CounterBoat].PosRearY = y;
                                this.BoatReference[this.CounterBoat].PosFrontX = x - 1;
                                this.BoatReference[this.CounterBoat].PosFrontY = y;
                                this.BoatReference[this.CounterBoat].ShipDestryoed = false;
                                this.BoatReference[this.CounterBoat].Front = false;
                                this.BoatReference[this.CounterBoat].Rear = false;
                                this.BoatReference[this.CounterBoat].Horizontal = this.horizontal;

                                this.PbStore[x, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x - 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x - 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1)
                            {
                                // Otherwise assemble the boat in the normal direction (2 fields)
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                                tmp.Tag = 1;
                                this.Pb[x + 1, y].Tag = 1;
                                this.BoatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                this.BoatReference[this.CounterBoat].PosRearX = x;
                                this.BoatReference[this.CounterBoat].PosRearY = y;
                                this.BoatReference[this.CounterBoat].PosFrontX = x + 1;
                                this.BoatReference[this.CounterBoat].PosFrontY = y;
                                this.BoatReference[this.CounterBoat].ShipDestryoed = false;
                                this.BoatReference[this.CounterBoat].Front = false;
                                this.BoatReference[this.CounterBoat].Rear = false;
                                this.BoatReference[this.CounterBoat].Horizontal = this.horizontal;
                                this.PbStore[x, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.Ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.Pb[x + 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.Pb[x + 1, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                                tmp.Tag = 1;
                                Pb[x, y - 1].Tag = 1;
                                BoatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                BoatReference[CounterBoat].PosRearX = x;
                                BoatReference[CounterBoat].PosRearY = y;
                                BoatReference[CounterBoat].PosFrontX = x;
                                BoatReference[CounterBoat].PosFrontY = y - 1;
                                BoatReference[CounterBoat].ShipDestryoed = false;
                                BoatReference[CounterBoat].Front = false;
                                BoatReference[CounterBoat].Rear = false;
                                BoatReference[CounterBoat].Horizontal = horizontal;
                                PbStore[x, y].Name = BoatReference[CounterBoat].ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = BoatReference[CounterBoat].ShipName + "_" + "Rear";
                                Ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y - 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y - 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                                tmp.Tag = 1;
                                Pb[x, y + 1].Tag = 1;

                                // Position sowie name des schiffes speichern
                                BoatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString();
                                BoatReference[CounterBoat].PosRearX = x;
                                BoatReference[CounterBoat].PosRearY = y;
                                BoatReference[CounterBoat].PosFrontX = x;
                                BoatReference[CounterBoat].PosFrontY = y + 1;
                                BoatReference[CounterBoat].ShipDestryoed = false;
                                BoatReference[CounterBoat].Front = false;
                                BoatReference[CounterBoat].Rear = false;
                                BoatReference[CounterBoat].Horizontal = horizontal;

                                PbStore[x, y].Name = BoatReference[CounterBoat].ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = BoatReference[CounterBoat].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                Ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                Pb[x, y + 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                Pb[x, y + 1].BackColor = Color.Transparent;
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
            positionString = positionString.Remove(0, 3); // pb_ aus dem String entfernen
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1 && (int)this.Pb[x - 2, y].Tag != 1 && (int)this.Pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.Pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1 && (int)this.Pb[x + 2, y].Tag != 1 && (int)this.Pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.Pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1 && (int)Pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                Pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1 && (int)Pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                Pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1 && (int)this.Pb[x - 2, y].Tag != 1 && (int)this.Pb[x - 3, y].Tag != 1 && (int)this.Pb[x - 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.Pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1 && (int)this.Pb[x + 2, y].Tag != 1 && (int)this.Pb[x + 3, y].Tag != 1 && (int)this.Pb[x + 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.Pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1 && (int)Pb[x, y - 3].Tag != 1 && (int)Pb[x, y - 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                Pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1 && (int)Pb[x, y + 3].Tag != 1 && (int)Pb[x, y + 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                Pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1 && (int)this.Pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.Pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1 && (int)this.Pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.Pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // Cruiser ist 5 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1 && (int)Pb[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                Pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1 && (int)Pb[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                Pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
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
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.Pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                            }
                        }
                        else
                        {
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.Pb[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.Pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // boat ist 2 Fleder groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                Pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)Pb[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                Pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
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
                this.Pb[x, y].Controls.Add(contr);
                //// Control[] s = this.Controls.Find(contr.ShipName, false);
                //// Die PictureBox in den Fordergrund bringen
                //// s[0].BringToFront();
            }
        }
    }
}