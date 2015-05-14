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

        private delegate void setTextCallback(string text);

        private delegate void ShowDestroyedShipsCallback(int[] args, bool Horizontal);

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
        /// Verwaltet die Position der boote sowie den Zustand
        /// </summary>
        public Ships.boat[] BoatReference = new Ships.boat[3];                        // 3 Boote
        public Ships.cruiser[] CruiserReference = new Ships.cruiser[3];               // 3 Cruiser
        public Ships.galley GalleyReference = new Ships.galley();                     // 1 Galley
        public Ships.battleship BattleshipReference = new Ships.battleship();         // 1 Schlachtschiff

        public int CounterGalley = 0;
        public int CounterBattleship = 0;
        public int CounterCruiser = 0;
        public int CounterBoat = 0;

        /// <summary>
        /// Enthält eine Auflistung an Schiffsmodellen
        /// </summary>
        public ShipModels ships;

        /// <summary>
        /// Enthält das Spielfeld und alle darin gesetzten Schiffe
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] pb = new Battleships.DoubleBufferedPanel[10, 10];

        /// <summary>
        /// Enthält eine Schattenkopie des Spielfeldes
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] PbStore = new Battleships.DoubleBufferedPanel[10, 10];

        /// <summary>
        /// Farbwert der angezeigt wird, wenn eine Kollsision beim Schiffe setzen erkannt wird
        /// </summary>
        private Color collisionColor;

        /// <summary>
        /// Flag welches angibt, ob ein Schiff Horizontal oder vertikal gesetzt werden soll
        /// </summary>
        private bool Horizontal;

        public BattlefieldPlayer(int x, int y)
        {
            // Schiffe werden standardgemäß Horizontal gesetzt (mit einem Klick auf die rechte Maustaste kann das geändert werden)
            this.Horizontal = true;
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
            for (int i = 0; i < this.pb.GetLength(0); i++)
            {
                for (int j = 0; j < this.pb.GetLength(1); j++)
                {
                    Battleships.DoubleBufferedPanel p = new Battleships.DoubleBufferedPanel();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Padding = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.MouseClick += new MouseEventHandler(this.p_MouseClick);
                    p.MouseEnter += new EventHandler(this.p_MouseEnter);
                    p.MouseLeave += new EventHandler(this.p_MouseLeave);
                    //// p.Visible = false;
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    this.pb[i, j] = p;
                    this.Controls.Add(p);

                    this.PbStore[i, j] = new Battleships.DoubleBufferedPanel();
                }
            }
        }

        #region Mouse-Events
        public void p_MouseClick(object sender, MouseEventArgs e)
        {
            // Das Panel holen, welches das MouseClick-Event ausgelöst hat
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;

            // Linke Maustaste gedrückt
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Schiff an angeklickte Position setzen
                this.setShips(ref tmp);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Loop through the fields (PictureBox)
                for (int i = 0; i < this.pb.GetLength(0); i++)
                {
                    for (int j = 0; j < this.pb.GetLength(1); j++)
                    {
                        // Prüfen ob Feld ein Schiffsteil beihnaltet (Tag = 1)
                        if ((int)this.pb[i, j].Tag != (int)1)
                        {
                            // Wenn nein, dann Bild im Feld löschen
                            this.pb[i, j].BackgroundImage = null;
                        }
                    }
                }

                this.Horizontal = !this.Horizontal; // Wert negieren
                this.drawShips(ref tmp); // Schiff zeichnen
            }
        }

        public void p_MouseEnter(object sender, EventArgs e)
        {
            // Event wurde von einer Panel_DoubleBuffered ausgelöst...
            // Senderobjekt erhalten --> Panel welches das Event ausgelöst hat
            Battleships.DoubleBufferedPanel tmp = (Battleships.DoubleBufferedPanel)sender;
            this.drawShips(ref tmp); // Schiff zeichnen
        }

        public void p_MouseLeave(object sender, EventArgs e)
        {
            // Alle Panels durchlaufen
            for (int x = 0; x < this.pb.GetLength(0); x++)
            {
                for (int y = 0; y < this.pb.GetLength(1); y++)
                {
                    // If no image is saved set current panel
                    if ((int)this.pb[x, y].Tag != (int)1)
                    {
                        this.pb[x, y].BackgroundImage = null; // Dann Bild löschen
                        this.pb[x, y].Tag = 0; // Bildflag auf false
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Überprüft ob der Gegner etwas getroffen hat oder nicht
        /// </summary>
        /// <param name="x">X-Koordinate des Schusses</param>
        /// <param name="y">Y-Koordniate des Schusses</param>
        /// <returns>false wenn nicht getroffen, true wenn getroffen</returns>
        public bool hitOrMiss(int x, int y)
        {
            if (this.pb[x, y].BackgroundImage == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Setzt einen Treffer auf das angegebene Feld
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        /// <returns>true or false if enemy won or not</returns>
        public bool SetImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion2.wav");
                this.DrawExplosion(x, y); // Explosion auf dem Spielfeld darstellen
                this.checkShips(x, y); // Überprüfen welches Schiff wo getroffen wurde

                // Spielstatus prüfen
                if (this.checkGameStatus())
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
            this.pb[x, y].BackgroundImage = Properties.Resources.splash;
            this.pb[x, y].Tag = 1;
        }

        /// <summary>
        /// Shows a destroyed boat on the enemies playing field
        /// </summary>
        /// <param name="args">Contains the coordinates of the vessel</param>
        /// <param name="Horizontal">Specifies whether the ship was used Horizontally or vertically</param>
        public void ShowDestroyedBoat(int[] args, bool Horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedBoat);
                this.Invoke(d, new object[] { args, Horizontal });
            }
            else
            {
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // Explosionsbild an der angegeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                if (Horizontal)
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h2;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                }
                else
                {
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                }
            }
        }

        /// <summary>
        /// Displays on the enemy field a ruined cruiser
        /// </summary>
        /// <param name="args">the co-ordinates of the vessel</param>
        /// <param name="Horizontal">Specifies whether the ship was placed Horizontally or vertically</param>
        public void ShowDestroyedCruiser(int[] args, bool Horizontal)
        {
            if (this.InvokeRequired)
            {
                ShowDestroyedShipsCallback d = new ShowDestroyedShipsCallback(this.ShowDestroyedCruiser);
                this.Invoke(d, new object[] { args, Horizontal });
            }
            else
            {
                // TODO: Siehe ShowDestroyedBoat
                BattleshipsForm.SoundPlayer.PlaySoundAsync("explosion1.wav");

                // Explosionsbild an der angegebeben Stelle entfernen (Control entfernen --> PictureBox)
                this.pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                this.pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                this.pb[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

                if (Horizontal)
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
        /// Überprüft den Spielstatus (Sind alle Schiffe von mir zerstört, hat der Gegner gewonnen)
        /// </summary>
        /// <returns>
        /// true wenn alle meine Schiffe zerstört sind
        /// false wenn auch nur noch ein Schiff von mir existiert
        /// </returns>
        private bool checkGameStatus()
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
        /// Findet heraus welches Schiff an welchem Teil getroffen wurde und ob ein Schiff komplett zerstört wurde
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        private void checkShips(int x, int y)
        {
            // Herausfinden welches Schiff getroffen wurde
            // Wurde ein Boot getroffen?
            if (this.PbStore[x, y].Name.StartsWith("Boat_"))
            {
                // Die Nr. des Bootes herausfinden (von 1-3)
                string[] sBoat = this.PbStore[x, y].Name.Split('_');
                int boatNr = int.Parse(sBoat[1]);
                string boatPart = sBoat[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (boatPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (Horizontal)
                        this.BoatReference[boatNr].Rear = true;
                        this.setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        this.BoatReference[boatNr].Front = true;
                        this.setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde an der Front getroffen!\n");
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
                    BattleshipsForm.battlefieldOpponent.ShowDestroyedBoat(
                        new int[4]
                        {
                        this.BoatReference[boatNr].PosRearX, this.BoatReference[boatNr].PosRearY,
                        this.BoatReference[boatNr].PosFrontX, this.BoatReference[boatNr].PosFrontY
                        },
                        this.BoatReference[boatNr].Horizontal);
                    this.setTextLblStatus("\nBoat Nr. " + boatNr.ToString() + " destroyed!");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Cruiser_"))
            {
            // Wurde ein Cruiser getroffen?
                string[] sCruiser = this.PbStore[x, y].Name.Split('_');
                int cruiserNr = int.Parse(sCruiser[1]);
                string cruiserPart = sCruiser[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (cruiserPart)
                {
                    case ("Rear"):
                        // heck wurde getroffen (Horizontal)
                        this.CruiserReference[cruiserNr].Rear = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am heck getroffen!\n");
                        break;
                    case ("Middle"):
                        // Mittelteil wurde getroffen
                        this.CruiserReference[cruiserNr].middle = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Mittelteil getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        this.CruiserReference[cruiserNr].Front = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob der Cruiser komplett zerstört wurde
                if (this.CruiserReference[cruiserNr].Rear && this.CruiserReference[cruiserNr].middle && this.CruiserReference[cruiserNr].Front)
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
                    BattleshipsForm.battlefieldOpponent.ShowDestroyedCruiser(
                        new int[6]
                        {
                        this.CruiserReference[cruiserNr].PosRearX, this.CruiserReference[cruiserNr].PosRearY,
                        this.CruiserReference[cruiserNr].PosMiddleX, this.CruiserReference[cruiserNr].PosMiddleY,
                        this.CruiserReference[cruiserNr].PosFrontX, this.CruiserReference[cruiserNr].PosFrontY
                        },
                        this.CruiserReference[cruiserNr].Horizontal);
                    this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Galley_"))
            {
            // Wurde die Galley getroffen?
                string[] sGalley = this.PbStore[x, y].Name.Split('_');
                string galleyPart = sGalley[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (galleyPart)
                {
                    case ("Rear"):
                        this.GalleyReference.Rear = true;
                        this.setTextLblStatus("Galley wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.GalleyReference.MiddleFirstPart = true;
                        this.setTextLblStatus("Galley wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.GalleyReference.MiddleSecondPart = true;
                        this.setTextLblStatus("Galley wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.GalleyReference.Front = true;
                        this.setTextLblStatus("Galley wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob die Galley komplett zerstört wurde
                if (this.GalleyReference.Rear && this.GalleyReference.MiddleFirstPart && this.GalleyReference.MiddleSecondPart && this.GalleyReference.Front)
                {
                    this.GalleyReference.ShipDestryoed = true;
                    this.setTextLblStatus("Galley destroyed!\n");
                }
            }
            else if (this.PbStore[x, y].Name.StartsWith("Battleship_"))
            {
            // Wurde das Battleship getroffen?
                string[] sBattleship = this.PbStore[x, y].Name.Split('_');
                string battleshipPart = sBattleship[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (battleshipPart)
                {
                    case ("Rear"):
                        this.BattleshipReference.Rear = true;
                        this.setTextLblStatus("Battleship wurde am heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.BattleshipReference.MiddleFirstPart = true;
                        this.setTextLblStatus("Battleship wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.BattleshipReference.MiddleSecondPart = true;
                        this.setTextLblStatus("Battleship wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.BattleshipReference.Front = true;
                        this.setTextLblStatus("Battleship wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Battleship komplett zerstört wurde
                if (this.BattleshipReference.Rear && this.BattleshipReference.MiddleFirstPart && this.BattleshipReference.MiddleSecondPart && this.BattleshipReference.Front)
                {
                    this.BattleshipReference.ShipDestryoed = true;
                    this.setTextLblStatus("Battleship destroyed!\n");
                }
            }
        }

        /// <summary>
        /// Entscheidet anhand des bereits im Panel gespeicherten Bildes welche Explosion dargestellt werden soll
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
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
        /// Setzt die Schiffsteile an den jeweiligen Positionen fest (Bei einem MouseClick-Event)
        /// </summary>
        /// <param name="tmp">Das Panel welches das MouseClick-Event ausgelöst hat (als Referenz)</param>
        private void setShips(ref Battleships.DoubleBufferedPanel tmp)
        {
            string positionString = tmp.Name;
            positionString = positionString.Remove(0, 3); // pb_ aus dem String entfernen
            string[] position = positionString.Split(':'); // x und y Position
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (this.ships)
            {
                // galley
                case ShipModels.galley:
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != (int)1 && (int)this.pb[x - 1, y].Tag != (int)1 && (int)this.pb[x - 2, y].Tag != (int)1 && (int)this.pb[x - 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                                tmp.Tag = 1;
                                this.pb[x - 1, y].Tag = 1;
                                this.pb[x - 2, y].Tag = 1;
                                this.pb[x - 3, y].Tag = 1;

                                this.GalleyReference.ShipName = "Galley";
                                this.GalleyReference.PosRearX = x;
                                this.GalleyReference.PosRearY = y;
                                this.GalleyReference.ShipDestryoed = false;
                                this.GalleyReference.Rear = false;
                                this.GalleyReference.Front = false;
                                this.GalleyReference.MiddleFirstPart = false;
                                this.GalleyReference.MiddleSecondPart = false;
                                this.GalleyReference.Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.GalleyReference.ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.GalleyReference.ShipName + "_" + "Middle2";
                                this.PbStore[x - 2, y].Name = this.GalleyReference.ShipName + "_" + "Middle1";
                                this.PbStore[x - 3, y].Name = this.GalleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x - 1, y].BackColor = this.collisionColor;
                                this.pb[x - 2, y].BackColor = this.collisionColor;
                                this.pb[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x - 1, y].BackColor = Color.Transparent;
                                this.pb[x - 2, y].BackColor = Color.Transparent;
                                this.pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != (int)1 && (int)this.pb[x + 1, y].Tag != (int)1 && (int)this.pb[x + 2, y].Tag != (int)1 && (int)this.pb[x + 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                                tmp.Tag = 1;
                                this.pb[x + 1, y].Tag = 1;
                                this.pb[x + 2, y].Tag = 1;
                                this.pb[x + 3, y].Tag = 1;

                                this.GalleyReference.ShipName = "Galley";
                                this.GalleyReference.PosRearX = x;
                                this.GalleyReference.PosRearY = y;
                                this.GalleyReference.ShipDestryoed = false;
                                this.GalleyReference.Rear = false;
                                this.GalleyReference.Front = false;
                                this.GalleyReference.MiddleFirstPart = false;
                                this.GalleyReference.MiddleSecondPart = false;
                                this.GalleyReference.Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.GalleyReference.ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.GalleyReference.ShipName + "_" + "Middle1";
                                this.PbStore[x + 2, y].Name = this.GalleyReference.ShipName + "_" + "Middle2";
                                this.PbStore[x + 3, y].Name = this.GalleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x + 1, y].BackColor = this.collisionColor;
                                this.pb[x + 2, y].BackColor = this.collisionColor;
                                this.pb[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x + 1, y].BackColor = Color.Transparent;
                                this.pb[x + 2, y].BackColor = Color.Transparent;
                                this.pb[x + 3, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1 && (int)pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                                tmp.Tag = 1;
                                pb[x, y - 1].Tag = 1;
                                pb[x, y - 2].Tag = 1;
                                pb[x, y - 3].Tag = 1;

                                GalleyReference.ShipName = "Galley";
                                GalleyReference.PosRearX = x;
                                GalleyReference.PosRearY = y;
                                GalleyReference.ShipDestryoed = false;
                                GalleyReference.Rear = false;
                                GalleyReference.Front = false;
                                GalleyReference.MiddleFirstPart = false;
                                GalleyReference.MiddleSecondPart = false;
                                GalleyReference.Horizontal = Horizontal;

                                PbStore[x, y].Name = GalleyReference.ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = GalleyReference.ShipName + "_" + "Middle2";
                                PbStore[x, y - 2].Name = GalleyReference.ShipName + "_" + "Middle1";
                                PbStore[x, y - 3].Name = GalleyReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y - 1].BackColor = collisionColor;
                                pb[x, y - 2].BackColor = collisionColor;
                                pb[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y - 1].BackColor = Color.Transparent;
                                pb[x, y - 2].BackColor = Color.Transparent;
                                pb[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1 && (int)pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                                tmp.Tag = 1;
                                pb[x, y + 1].Tag = 1;
                                pb[x, y + 2].Tag = 1;
                                pb[x, y + 3].Tag = 1;

                                GalleyReference.ShipName = "Galley";
                                GalleyReference.PosRearX = x;
                                GalleyReference.PosRearY = y;
                                GalleyReference.ShipDestryoed = false;
                                GalleyReference.Rear = false;
                                GalleyReference.Front = false;
                                GalleyReference.MiddleFirstPart = false;
                                GalleyReference.MiddleSecondPart = false;
                                GalleyReference.Horizontal = Horizontal;

                                PbStore[x, y].Name = GalleyReference.ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = GalleyReference.ShipName + "_" + "Middle1";
                                PbStore[x, y + 2].Name = GalleyReference.ShipName + "_" + "Middle2";
                                PbStore[x, y + 3].Name = GalleyReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y + 1].BackColor = collisionColor;
                                pb[x, y + 2].BackColor = collisionColor;
                                pb[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y + 1].BackColor = Color.Transparent;
                                pb[x, y + 2].BackColor = Color.Transparent;
                                pb[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.battleship: // battleship
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // battleship ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1 && (int)this.pb[x - 2, y].Tag != 1 && (int)this.pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                                tmp.Tag = 1;
                                this.pb[x - 1, y].Tag = 1;
                                this.pb[x - 2, y].Tag = 1;
                                this.pb[x - 3, y].Tag = 1;

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;

                                this.BattleshipReference.ShipName = "Battleship";
                                this.BattleshipReference.PosRearX = x;
                                this.BattleshipReference.PosRearY = y;
                                this.BattleshipReference.ShipDestryoed = false;
                                this.BattleshipReference.Rear = false;
                                this.BattleshipReference.Front = false;
                                this.BattleshipReference.MiddleFirstPart = false;
                                this.BattleshipReference.MiddleSecondPart = false;
                                this.BattleshipReference.Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.BattleshipReference.ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.BattleshipReference.ShipName + "_" + "Middle2";
                                this.PbStore[x - 2, y].Name = this.BattleshipReference.ShipName + "_" + "Middle1";
                                this.PbStore[x - 3, y].Name = this.BattleshipReference.ShipName + "_" + "Rear";
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x - 1, y].BackColor = this.collisionColor;
                                this.pb[x - 2, y].BackColor = this.collisionColor;
                                this.pb[x - 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x - 1, y].BackColor = Color.Transparent;
                                this.pb[x - 2, y].BackColor = Color.Transparent;
                                this.pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1 && (int)this.pb[x + 2, y].Tag != 1 && (int)this.pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                                tmp.Tag = 1;
                                this.pb[x + 1, y].Tag = 1;
                                this.pb[x + 2, y].Tag = 1;
                                this.pb[x + 3, y].Tag = 1;

                                this.BattleshipReference.ShipName = "Battleship";
                                this.BattleshipReference.PosRearX = x;
                                this.BattleshipReference.PosRearY = y;
                                this.BattleshipReference.ShipDestryoed = false;
                                this.BattleshipReference.Rear = false;
                                this.BattleshipReference.Front = false;
                                this.BattleshipReference.MiddleFirstPart = false;
                                this.BattleshipReference.MiddleSecondPart = false;
                                this.BattleshipReference.Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.BattleshipReference.ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.BattleshipReference.ShipName + "_" + "Middle1";
                                this.PbStore[x + 2, y].Name = this.BattleshipReference.ShipName + "_" + "Middle2";
                                this.PbStore[x + 3, y].Name = this.BattleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x + 1, y].BackColor = this.collisionColor;
                                this.pb[x + 2, y].BackColor = this.collisionColor;
                                this.pb[x + 3, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x + 1, y].BackColor = Color.Transparent;
                                this.pb[x + 2, y].BackColor = Color.Transparent;
                                this.pb[x + 3, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1 && (int)pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                                tmp.Tag = 1;
                                pb[x, y - 1].Tag = 1;
                                pb[x, y - 2].Tag = 1;
                                pb[x, y - 3].Tag = 1;

                                BattleshipReference.ShipName = "Battleship";
                                BattleshipReference.PosRearX = x;
                                BattleshipReference.PosRearY = y;
                                BattleshipReference.ShipDestryoed = false;
                                BattleshipReference.Rear = false;
                                BattleshipReference.Front = false;
                                BattleshipReference.MiddleFirstPart = false;
                                BattleshipReference.MiddleSecondPart = false;
                                BattleshipReference.Horizontal = Horizontal;

                                PbStore[x, y].Name = BattleshipReference.ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = BattleshipReference.ShipName + "_" + "Middle2";
                                PbStore[x, y - 2].Name = BattleshipReference.ShipName + "_" + "Middle1";
                                PbStore[x, y - 3].Name = BattleshipReference.ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y - 1].BackColor = collisionColor;
                                pb[x, y - 2].BackColor = collisionColor;
                                pb[x, y - 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y - 1].BackColor = Color.Transparent;
                                pb[x, y - 2].BackColor = Color.Transparent;
                                pb[x, y - 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1 && (int)pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                                tmp.Tag = 1;
                                pb[x, y + 1].Tag = 1;
                                pb[x, y + 2].Tag = 1;
                                pb[x, y + 3].Tag = 1;

                                BattleshipReference.ShipName = "Battleship";
                                BattleshipReference.PosRearX = x;
                                BattleshipReference.PosRearY = y;
                                BattleshipReference.ShipDestryoed = false;
                                BattleshipReference.Rear = false;
                                BattleshipReference.Front = false;
                                BattleshipReference.MiddleFirstPart = false;
                                BattleshipReference.MiddleSecondPart = false;
                                BattleshipReference.Horizontal = Horizontal;

                                PbStore[x, y].Name = BattleshipReference.ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = BattleshipReference.ShipName + "_" + "Middle1";
                                PbStore[x, y + 2].Name = BattleshipReference.ShipName + "_" + "Middle2";
                                PbStore[x, y + 3].Name = BattleshipReference.ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y + 1].BackColor = collisionColor;
                                pb[x, y + 2].BackColor = collisionColor;
                                pb[x, y + 3].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y + 1].BackColor = Color.Transparent;
                                pb[x, y + 2].BackColor = Color.Transparent;
                                pb[x, y + 3].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.cruiser: // Cruiser
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1 && (int)this.pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                tmp.Tag = 1;
                                this.pb[x - 1, y].Tag = 1;
                                this.pb[x - 2, y].Tag = 1;

                                this.CruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.CruiserReference[this.CounterCruiser].PosRearX = x;
                                this.CruiserReference[this.CounterCruiser].PosRearY = y;
                                this.CruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.CruiserReference[this.CounterCruiser].Front = false;
                                this.CruiserReference[this.CounterCruiser].Rear = false;
                                this.CruiserReference[this.CounterCruiser].middle = false;
                                this.CruiserReference[this.CounterCruiser].Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.PbStore[x - 2, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x - 1, y].BackColor = this.collisionColor;
                                this.pb[x - 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x - 1, y].BackColor = Color.Transparent;
                                this.pb[x - 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1 && (int)this.pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                tmp.Tag = 1;
                                this.pb[x + 1, y].Tag = 1;
                                this.pb[x + 2, y].Tag = 1;

                                this.CruiserReference[this.CounterCruiser].ShipName = "Cruiser_" + this.CounterCruiser.ToString();
                                this.CruiserReference[this.CounterCruiser].PosRearX = x;
                                this.CruiserReference[this.CounterCruiser].PosRearY = y;
                                this.CruiserReference[this.CounterCruiser].ShipDestryoed = false;
                                this.CruiserReference[this.CounterCruiser].Front = false;
                                this.CruiserReference[this.CounterCruiser].Rear = false;
                                this.CruiserReference[this.CounterCruiser].middle = false;
                                this.CruiserReference[this.CounterCruiser].Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Middle";
                                this.PbStore[x + 2, y].Name = this.CruiserReference[this.CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x + 1, y].BackColor = this.collisionColor;
                                this.pb[x + 2, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x + 1, y].BackColor = Color.Transparent;
                                this.pb[x + 2, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                tmp.Tag = 1;
                                pb[x, y - 1].Tag = 1;
                                pb[x, y - 2].Tag = 1;

                                CruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                CruiserReference[CounterCruiser].PosRearX = x;
                                CruiserReference[CounterCruiser].PosRearY = y;
                                CruiserReference[CounterCruiser].ShipDestryoed = false;
                                CruiserReference[CounterCruiser].Front = false;
                                CruiserReference[CounterCruiser].Rear = false;
                                CruiserReference[CounterCruiser].middle = false;
                                CruiserReference[CounterCruiser].Horizontal = Horizontal;

                                PbStore[x, y].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                PbStore[x, y - 2].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y - 1].BackColor = collisionColor;
                                pb[x, y - 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y - 1].BackColor = Color.Transparent;
                                pb[x, y - 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                tmp.Tag = 1;
                                pb[x, y + 1].Tag = 1;
                                pb[x, y + 2].Tag = 1;

                                CruiserReference[CounterCruiser].ShipName = "Cruiser_" + CounterCruiser.ToString();
                                CruiserReference[CounterCruiser].PosRearX = x;
                                CruiserReference[CounterCruiser].PosRearY = y;
                                CruiserReference[CounterCruiser].ShipDestryoed = false;
                                CruiserReference[CounterCruiser].Front = false;
                                CruiserReference[CounterCruiser].Rear = false;
                                CruiserReference[CounterCruiser].middle = false;
                                CruiserReference[CounterCruiser].Horizontal = Horizontal;

                                PbStore[x, y].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Middle";
                                PbStore[x, y + 2].Name = CruiserReference[CounterCruiser].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y + 1].BackColor = collisionColor;
                                pb[x, y + 2].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y + 1].BackColor = Color.Transparent;
                                pb[x, y + 2].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
                case ShipModels.boat: // boat
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                                tmp.Tag = 1;
                                this.pb[x - 1, y].Tag = 1;

                                // Position sowie name des Schiffes speichern
                                this.BoatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString();
                                this.BoatReference[this.CounterBoat].PosRearX = x;
                                this.BoatReference[this.CounterBoat].PosRearY = y;
                                this.BoatReference[this.CounterBoat].PosFrontX = x - 1;
                                this.BoatReference[this.CounterBoat].PosFrontY = y;
                                this.BoatReference[this.CounterBoat].ShipDestryoed = false;
                                this.BoatReference[this.CounterBoat].Front = false;
                                this.BoatReference[this.CounterBoat].Rear = false;
                                this.BoatReference[this.CounterBoat].Horizontal = this.Horizontal;

                                this.PbStore[x, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.PbStore[x - 1, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Rear";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x - 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x - 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1)
                            {
                                // Otherwise assemble the boat in the normal direction (2 fields)
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                                tmp.Tag = 1;
                                this.pb[x + 1, y].Tag = 1;
                                this.BoatReference[this.CounterBoat].ShipName = "Boat_" + this.CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                this.BoatReference[this.CounterBoat].PosRearX = x;
                                this.BoatReference[this.CounterBoat].PosRearY = y;
                                this.BoatReference[this.CounterBoat].PosFrontX = x + 1;
                                this.BoatReference[this.CounterBoat].PosFrontY = y;
                                this.BoatReference[this.CounterBoat].ShipDestryoed = false;
                                this.BoatReference[this.CounterBoat].Front = false;
                                this.BoatReference[this.CounterBoat].Rear = false;
                                this.BoatReference[this.CounterBoat].Horizontal = this.Horizontal;
                                this.PbStore[x, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Rear";
                                this.PbStore[x + 1, y].Name = this.BoatReference[this.CounterBoat].ShipName + "_" + "Front";
                                this.ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = this.collisionColor;
                                this.pb[x + 1, y].BackColor = this.collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                this.pb[x + 1, y].BackColor = Color.Transparent;
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
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                                tmp.Tag = 1;
                                pb[x, y - 1].Tag = 1;
                                BoatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString(); // Position sowie name des schiffes speichern
                                BoatReference[CounterBoat].PosRearX = x;
                                BoatReference[CounterBoat].PosRearY = y;
                                BoatReference[CounterBoat].PosFrontX = x;
                                BoatReference[CounterBoat].PosFrontY = y - 1;
                                BoatReference[CounterBoat].ShipDestryoed = false;
                                BoatReference[CounterBoat].Front = false;
                                BoatReference[CounterBoat].Rear = false;
                                BoatReference[CounterBoat].Horizontal = Horizontal;
                                PbStore[x, y].Name = BoatReference[CounterBoat].ShipName + "_" + "Front";
                                PbStore[x, y - 1].Name = BoatReference[CounterBoat].ShipName + "_" + "Rear";
                                ships = ShipModels.nothing; // Schiffsauswahl auf nothing setzen
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y - 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y - 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                                tmp.Tag = 1;
                                pb[x, y + 1].Tag = 1;

                                // Position sowie name des schiffes speichern
                                BoatReference[CounterBoat].ShipName = "Boat_" + CounterBoat.ToString();
                                BoatReference[CounterBoat].PosRearX = x;
                                BoatReference[CounterBoat].PosRearY = y;
                                BoatReference[CounterBoat].PosFrontX = x;
                                BoatReference[CounterBoat].PosFrontY = y + 1;
                                BoatReference[CounterBoat].ShipDestryoed = false;
                                BoatReference[CounterBoat].Front = false;
                                BoatReference[CounterBoat].Rear = false;
                                BoatReference[CounterBoat].Horizontal = Horizontal;

                                PbStore[x, y].Name = BoatReference[CounterBoat].ShipName + "_" + "Rear";
                                PbStore[x, y + 1].Name = BoatReference[CounterBoat].ShipName + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = ShipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x, y + 1].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x, y + 1].BackColor = Color.Transparent;
                                return;
                            }
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Zeichnet die Schiffe an die entsprechende Stelle (Bei einem MouseEnter-Event auf ein Panel)
        /// </summary>
        /// <param name="tmp">Das Panel welches das MouseEnter-Event ausgelöst hat (Als Referenz)</param>
        private void drawShips(ref Battleships.DoubleBufferedPanel tmp)
        {
            string positionString = tmp.Name;
            positionString = positionString.Remove(0, 3); // pb_ aus dem String entfernen
            string[] position = positionString.Split(':'); // x und y Position
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (this.ships)
            {
                case ShipModels.galley: // galley
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1 && (int)this.pb[x - 2, y].Tag != 1 && (int)this.pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                            }
                        }
                        else
                        {
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1 && (int)this.pb[x + 2, y].Tag != 1 && (int)this.pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                this.pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1 && (int)pb[x, y - 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.galley1v;
                            }
                        }
                        else // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1 && (int)pb[x, y + 3].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.galley2v;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.galley3v;
                                pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.galley4v;
                            }
                        }
                    }

                    break;
                case ShipModels.battleship: // battleship
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1 && (int)this.pb[x - 2, y].Tag != 1 && (int)this.pb[x - 3, y].Tag != 1 && (int)this.pb[x - 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                            }
                        }
                        else
                        {
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1 && (int)this.pb[x + 2, y].Tag != 1 && (int)this.pb[x + 3, y].Tag != 1 && (int)this.pb[x + 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                this.pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1 && (int)pb[x, y - 3].Tag != 1 && (int)pb[x, y - 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z4v1;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pb[x, y - 3].BackgroundImage = Battleships.Properties.Resources.z1v1;
                            }
                        }
                        else // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1 && (int)pb[x, y + 3].Tag != 1 && (int)pb[x, y + 4].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z1v1;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.z2v1;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.z3v1;
                                pb[x, y + 3].BackgroundImage = Battleships.Properties.Resources.z4v1;
                            }
                        }
                    }

                    break;
                case ShipModels.cruiser: // Cruiser
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1 && (int)this.pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                            }
                        }
                        else
                        {
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1 && (int)this.pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                this.pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // Cruiser ist 5 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1 && (int)pb[x, y - 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pb[x, y - 2].BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                            }
                        }
                        else // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1 && (int)pb[x, y + 2].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.cruiser2v;
                                pb[x, y + 2].BackgroundImage = Battleships.Properties.Resources.cruiser3v;
                            }
                        }
                    }

                    break;
                case ShipModels.boat: // boat
                    if (this.Horizontal)
                    {
                        // Horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)this.pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                this.pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                            }
                        }
                        else
                        {
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                            if ((int)tmp.Tag != 1 && (int)this.pb[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                this.pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                            }
                        }
                    }
                    else
                    {
                        // Vertikal
                        // boat ist 2 Fleder groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (y >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y - 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2v;
                                pb[x, y - 1].BackgroundImage = Battleships.Properties.Resources.boat1v;
                            }
                        }
                        else // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x, y + 1].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1v;
                                pb[x, y + 1].BackgroundImage = Battleships.Properties.Resources.boat2v;
                            }
                        }
                    }

                    break;
            }
        }

        protected virtual void setTextLblStatus(string text)
        {
            if (BattleshipsForm.lblStatus.InvokeRequired)
            {
                setTextCallback d = new setTextCallback(this.setTextLblStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                BattleshipsForm.lblStatus.Text += text;
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
                this.pb[x, y].Controls.Add(contr);
                //// Control[] s = this.Controls.Find(contr.ShipName, false);
                //// Die PictureBox in den Fordergrund bringen
                //// s[0].BringToFront();
            }
        }
    }
}