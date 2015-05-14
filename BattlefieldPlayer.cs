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
        delegate void AddControlCallback(Control contr, int x, int y);

        delegate void setTextCallback(string text);

        delegate void showDestroyedShipsCallback(int[] args, bool horizontal);

        // Auflistung der Schiffsmodelle
        public enum shipModels
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
        public Ships.boat[] boatRef = new Ships.boat[3];                        // 3 Boote
        public Ships.cruiser[] cruiserRef = new Ships.cruiser[3];               // 3 Cruiser
        public Ships.galley galleyRef = new Ships.galley();                     // 1 Galley
        public Ships.battleship battleshipRef = new Ships.battleship();         // 1 Schlachtschiff

        public int zaehler_galley = 0;
        public int zaehler_battleship = 0;
        public int zaehler_cruiser = 0;
        public int zaehler_boat = 0;

        /// <summary>
        /// Enthält eine Auflistung an Schiffsmodellen
        /// </summary>
        public shipModels ships;

        /// <summary>
        /// Enthält das Spielfeld und alle darin gesetzten Schiffe
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] pb = new Battleships.DoubleBufferedPanel[10, 10];

        /// <summary>
        /// Enthält eine Schattenkopie des Spielfeldes
        /// </summary>
        public Battleships.DoubleBufferedPanel[,] pb_Store = new Battleships.DoubleBufferedPanel[10, 10];

        /// <summary>
        /// Farbwert der angezeigt wird, wenn eine Kollsision beim Schiffe setzen erkannt wird
        /// </summary>
        private Color collisionColor;

        /// <summary>
        /// Flag welches angibt, ob ein Schiff horizontal oder vertikal gesetzt werden soll
        /// </summary>
        private bool horizontal;

        public BattlefieldPlayer(int x, int y)
        {
            // Schiffe werden standardgemäß Horizontal gesetzt (mit einem Klick auf die rechte Maustaste kann das geändert werden)
            this.horizontal = true;
            this.collisionColor = new Color();
            this.collisionColor = Color.FromArgb(90, 210, 0, 0); // Ein helles Rot
            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //// this.BackgroundImageLayout = ImageLayout.Stretch;
            //// this.BackgroundImage = Battleships.Properties.Resources.meer_player;
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

                    this.pb_Store[i, j] = new Battleships.DoubleBufferedPanel();
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

                this.horizontal = !this.horizontal; // Wert negieren
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
        /// <returns>true or false if opponent won or not</returns>
        public bool setImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.soundPlayer.PlaySoundAsync("explosion2.wav");
                this.drawExplosion(x, y); // Explosion auf dem Spielfeld darstellen
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

        public void setMiss(int x, int y)
        {
            BattleshipsForm.soundPlayer.PlaySoundAsync("splash.wav");
            this.drawMiss(x, y); // Fehlschuss auf dem Spielfeld darstellen
        }

        private void drawMiss(int x, int y)
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

            // addControl(missPicture, x, y);
            this.pb[x, y].BackgroundImage = Properties.Resources.splash;
            this.pb[x, y].Tag = 1;
        }

        /// <summary>
        /// Shows a destroyed boat on the opponents playing field
        /// </summary>
        /// <param name="args">Contains the coordinates of the vessel</param>
        /// <param name="horizontal">Specifies whether the ship was used horizontally or vertically</param>
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
                    this.pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                    this.pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                }
            }
        }

        /// <summary>
        /// Displays on the opponent field a ruined cruiser
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
        /// Überprüft den Spielstatus (Sind alle Schiffe von mir zerstört, hat der Gegner gewonnen)
        /// </summary>
        /// <returns>
        /// true wenn alle meine Schiffe zerstört sind
        /// false wenn auch nur noch ein Schiff von mir existiert
        /// </returns>
        private bool checkGameStatus()
        {
            // Alle boote überprüfen
            for (int i = 0; i < this.boatRef.Length; i++)
            {
                // Überprüfen ob alle Boote zerstört sind
                if (!this.boatRef[i].shipDestryoed)
                {
                    // Wenn auch nur noch ein Boot existiert, dann ist das Spiel nocht nicht vorbei
                    return false;
                }
            }
            
            // Alle Cruiser überprüfen
            for (int i = 0; i < this.cruiserRef.Length; i++)
            {
                // Überprüfen ob alle Cruiser zerstört sind
                if (!this.cruiserRef[i].shipDestryoed)
                { 
                    // Wenn auch nur noch ein Cruiser existiert, dann ist das Spiel noch nicht vorbei
                    return false;
                }
            }

             // Die Galley überprüfen
            if (!this.galleyRef.shipDestryoed)
            {
                // Existiert die Galley noch, dann ist das Spiel nocht nicht vorbei
                return false;
            }

            // Das Battleship überprüfen
            if (!this.battleshipRef.shipDestryoed)
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
            if (this.pb_Store[x, y].Name.StartsWith("Boat_"))
            {
                // Die Nr. des Bootes herausfinden (von 1-3)
                string[] sBoat = this.pb_Store[x, y].Name.Split('_');
                int boatNr = int.Parse(sBoat[1]);
                string boatPart = sBoat[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (boatPart)
                {
                    case ("Heck"):
                        // Heck wurde getroffen (Horizontal)
                        this.boatRef[boatNr].Heck = true;
                        this.setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde am Heck getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        this.boatRef[boatNr].Front = true;
                        this.setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Boot komplett zerstört ist
                if (this.boatRef[boatNr].Front && this.boatRef[boatNr].Heck)
                {
                    this.boatRef[boatNr].shipDestryoed = true;
                    this.showDestroyedBoat( // Das zerstörte Boot auf dem Spielfeld darstellen
                        new int[4]
                        {
                        this.boatRef[boatNr].posHeckX, this.boatRef[boatNr].posHeckY,
                        this.boatRef[boatNr].posFrontX, this.boatRef[boatNr].posFrontY
                        },
                        this.boatRef[boatNr].horizontal);
                    BattleshipsForm.battlefieldOpponent.showDestroyedBoat(
                        new int[4]
                        {
                        this.boatRef[boatNr].posHeckX, this.boatRef[boatNr].posHeckY,
                        this.boatRef[boatNr].posFrontX, this.boatRef[boatNr].posFrontY
                        },
                        this.boatRef[boatNr].horizontal);
                    this.setTextLblStatus("\nBoat Nr. " + boatNr.ToString() + " destroyed!");
                }
            }
            else if (this.pb_Store[x, y].Name.StartsWith("Cruiser_"))
            {
            // Wurde ein Cruiser getroffen?
                string[] sCruiser = this.pb_Store[x, y].Name.Split('_');
                int cruiserNr = int.Parse(sCruiser[1]);
                string cruiserPart = sCruiser[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (cruiserPart)
                {
                    case ("Heck"):
                        // Heck wurde getroffen (Horizontal)
                        this.cruiserRef[cruiserNr].Heck = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Heck getroffen!\n");
                        break;
                    case ("Middle"):
                        // Mittelteil wurde getroffen
                        this.cruiserRef[cruiserNr].middle = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Mittelteil getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        this.cruiserRef[cruiserNr].Front = true;
                        this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob der Cruiser komplett zerstört wurde
                if (this.cruiserRef[cruiserNr].Heck && this.cruiserRef[cruiserNr].middle && this.cruiserRef[cruiserNr].Front)
                {
                    this.cruiserRef[cruiserNr].shipDestryoed = true;
                    
                    this.showDestroyedCruiser( // Den zerstörten Cruiser auf dem Spielfeld darstellen
                        new int[6]
                        {
                        this.cruiserRef[cruiserNr].posHeckX, this.cruiserRef[cruiserNr].posHeckY,
                        this.cruiserRef[cruiserNr].posMiddleX, this.cruiserRef[cruiserNr].posMiddleY,
                        this.cruiserRef[cruiserNr].posFrontX, this.cruiserRef[cruiserNr].posFrontY
                        },
                        this.cruiserRef[cruiserNr].horizontal);
                    BattleshipsForm.battlefieldOpponent.showDestroyedCruiser(
                        new int[6]
                        {
                        this.cruiserRef[cruiserNr].posHeckX, this.cruiserRef[cruiserNr].posHeckY,
                        this.cruiserRef[cruiserNr].posMiddleX, this.cruiserRef[cruiserNr].posMiddleY,
                        this.cruiserRef[cruiserNr].posFrontX, this.cruiserRef[cruiserNr].posFrontY
                        },
                        this.cruiserRef[cruiserNr].horizontal);
                    this.setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            else if (this.pb_Store[x, y].Name.StartsWith("Galley_"))
            {
            // Wurde die Galley getroffen?
                string[] sGalley = this.pb_Store[x, y].Name.Split('_');
                string galleyPart = sGalley[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (galleyPart)
                {
                    case ("Heck"):
                        this.galleyRef.Heck = true;
                        this.setTextLblStatus("Galley wurde am Heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.galleyRef.middle1 = true;
                        this.setTextLblStatus("Galley wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.galleyRef.middle2 = true;
                        this.setTextLblStatus("Galley wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.galleyRef.Front = true;
                        this.setTextLblStatus("Galley wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob die Galley komplett zerstört wurde
                if (this.galleyRef.Heck && this.galleyRef.middle1 && this.galleyRef.middle2 && this.galleyRef.Front)
                {
                    this.galleyRef.shipDestryoed = true;
                    this.setTextLblStatus("Galley destroyed!\n");
                }
            }
            else if (this.pb_Store[x, y].Name.StartsWith("Battleship_"))
            {
            // Wurde das Battleship getroffen?
                string[] sBattleship = this.pb_Store[x, y].Name.Split('_');
                string battleshipPart = sBattleship[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (battleshipPart)
                {
                    case ("Heck"):
                        this.battleshipRef.Heck = true;
                        this.setTextLblStatus("Battleship wurde am Heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        this.battleshipRef.middle1 = true;
                        this.setTextLblStatus("Battleship wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        this.battleshipRef.middle2 = true;
                        this.setTextLblStatus("Battleship wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        this.battleshipRef.Front = true;
                        this.setTextLblStatus("Battleship wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Battleship komplett zerstört wurde
                if (this.battleshipRef.Heck && this.battleshipRef.middle1 && this.battleshipRef.middle2 && this.battleshipRef.Front)
                {
                    this.battleshipRef.shipDestryoed = true;
                    this.setTextLblStatus("Battleship destroyed!\n");
                }
            }
        }

        /// <summary>
        /// Entscheidet anhand des bereits im Panel gespeicherten Bildes welche Explosion dargestellt werden soll
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        public void drawExplosion(int x, int y)
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
            this.addControl(explPicture, x, y); // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
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
                case shipModels.galley:
                    if (this.horizontal)
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

                                this.galleyRef.name = "Galley";
                                this.galleyRef.posHeckX = x;
                                this.galleyRef.posHeckY = y;
                                this.galleyRef.shipDestryoed = false;
                                this.galleyRef.Heck = false;
                                this.galleyRef.Front = false;
                                this.galleyRef.middle1 = false;
                                this.galleyRef.middle2 = false;
                                this.galleyRef.horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.galleyRef.name + "_" + "Front";
                                this.pb_Store[x - 1, y].Name = this.galleyRef.name + "_" + "Middle2";
                                this.pb_Store[x - 2, y].Name = this.galleyRef.name + "_" + "Middle1";
                                this.pb_Store[x - 3, y].Name = this.galleyRef.name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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

                                this.galleyRef.name = "Galley";
                                this.galleyRef.posHeckX = x;
                                this.galleyRef.posHeckY = y;
                                this.galleyRef.shipDestryoed = false;
                                this.galleyRef.Heck = false;
                                this.galleyRef.Front = false;
                                this.galleyRef.middle1 = false;
                                this.galleyRef.middle2 = false;
                                this.galleyRef.horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.galleyRef.name + "_" + "Heck";
                                this.pb_Store[x + 1, y].Name = this.galleyRef.name + "_" + "Middle1";
                                this.pb_Store[x + 2, y].Name = this.galleyRef.name + "_" + "Middle2";
                                this.pb_Store[x + 3, y].Name = this.galleyRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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

                                galleyRef.name = "Galley";
                                galleyRef.posHeckX = x;
                                galleyRef.posHeckY = y;
                                galleyRef.shipDestryoed = false;
                                galleyRef.Heck = false;
                                galleyRef.Front = false;
                                galleyRef.middle1 = false;
                                galleyRef.middle2 = false;
                                galleyRef.horizontal = horizontal;

                                pb_Store[x, y].Name = galleyRef.name + "_" + "Front";
                                pb_Store[x, y - 1].Name = galleyRef.name + "_" + "Middle2";
                                pb_Store[x, y - 2].Name = galleyRef.name + "_" + "Middle1";
                                pb_Store[x, y - 3].Name = galleyRef.name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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

                                galleyRef.name = "Galley";
                                galleyRef.posHeckX = x;
                                galleyRef.posHeckY = y;
                                galleyRef.shipDestryoed = false;
                                galleyRef.Heck = false;
                                galleyRef.Front = false;
                                galleyRef.middle1 = false;
                                galleyRef.middle2 = false;
                                galleyRef.horizontal = horizontal;

                                pb_Store[x, y].Name = galleyRef.name + "_" + "Heck";
                                pb_Store[x, y + 1].Name = galleyRef.name + "_" + "Middle1";
                                pb_Store[x, y + 2].Name = galleyRef.name + "_" + "Middle2";
                                pb_Store[x, y + 3].Name = galleyRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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
                case shipModels.battleship: // battleship
                    if (this.horizontal)
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
                                this.ships = shipModels.nothing;

                                this.battleshipRef.name = "Battleship";
                                this.battleshipRef.posHeckX = x;
                                this.battleshipRef.posHeckY = y;
                                this.battleshipRef.shipDestryoed = false;
                                this.battleshipRef.Heck = false;
                                this.battleshipRef.Front = false;
                                this.battleshipRef.middle1 = false;
                                this.battleshipRef.middle2 = false;
                                this.battleshipRef.horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.battleshipRef.name + "_" + "Front";
                                this.pb_Store[x - 1, y].Name = this.battleshipRef.name + "_" + "Middle2";
                                this.pb_Store[x - 2, y].Name = this.battleshipRef.name + "_" + "Middle1";
                                this.pb_Store[x - 3, y].Name = this.battleshipRef.name + "_" + "Heck";
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

                                this.battleshipRef.name = "Battleship";
                                this.battleshipRef.posHeckX = x;
                                this.battleshipRef.posHeckY = y;
                                this.battleshipRef.shipDestryoed = false;
                                this.battleshipRef.Heck = false;
                                this.battleshipRef.Front = false;
                                this.battleshipRef.middle1 = false;
                                this.battleshipRef.middle2 = false;
                                this.battleshipRef.horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.battleshipRef.name + "_" + "Heck";
                                this.pb_Store[x + 1, y].Name = this.battleshipRef.name + "_" + "Middle1";
                                this.pb_Store[x + 2, y].Name = this.battleshipRef.name + "_" + "Middle2";
                                this.pb_Store[x + 3, y].Name = this.battleshipRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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

                                battleshipRef.name = "Battleship";
                                battleshipRef.posHeckX = x;
                                battleshipRef.posHeckY = y;
                                battleshipRef.shipDestryoed = false;
                                battleshipRef.Heck = false;
                                battleshipRef.Front = false;
                                battleshipRef.middle1 = false;
                                battleshipRef.middle2 = false;
                                battleshipRef.horizontal = horizontal;

                                pb_Store[x, y].Name = battleshipRef.name + "_" + "Front";
                                pb_Store[x, y - 1].Name = battleshipRef.name + "_" + "Middle2";
                                pb_Store[x, y - 2].Name = battleshipRef.name + "_" + "Middle1";
                                pb_Store[x, y - 3].Name = battleshipRef.name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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

                                battleshipRef.name = "Battleship";
                                battleshipRef.posHeckX = x;
                                battleshipRef.posHeckY = y;
                                battleshipRef.shipDestryoed = false;
                                battleshipRef.Heck = false;
                                battleshipRef.Front = false;
                                battleshipRef.middle1 = false;
                                battleshipRef.middle2 = false;
                                battleshipRef.horizontal = horizontal;

                                pb_Store[x, y].Name = battleshipRef.name + "_" + "Heck";
                                pb_Store[x, y + 1].Name = battleshipRef.name + "_" + "Middle1";
                                pb_Store[x, y + 2].Name = battleshipRef.name + "_" + "Middle2";
                                pb_Store[x, y + 3].Name = battleshipRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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
                case shipModels.cruiser: // Cruiser
                    if (this.horizontal)
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

                                this.cruiserRef[this.zaehler_cruiser].name = "Cruiser_" + this.zaehler_cruiser.ToString();
                                this.cruiserRef[this.zaehler_cruiser].posHeckX = x;
                                this.cruiserRef[this.zaehler_cruiser].posHeckY = y;
                                this.cruiserRef[this.zaehler_cruiser].shipDestryoed = false;
                                this.cruiserRef[this.zaehler_cruiser].Front = false;
                                this.cruiserRef[this.zaehler_cruiser].Heck = false;
                                this.cruiserRef[this.zaehler_cruiser].middle = false;
                                this.cruiserRef[this.zaehler_cruiser].horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Front";
                                this.pb_Store[x - 1, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Middle";
                                this.pb_Store[x - 2, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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

                                this.cruiserRef[this.zaehler_cruiser].name = "Cruiser_" + this.zaehler_cruiser.ToString();
                                this.cruiserRef[this.zaehler_cruiser].posHeckX = x;
                                this.cruiserRef[this.zaehler_cruiser].posHeckY = y;
                                this.cruiserRef[this.zaehler_cruiser].shipDestryoed = false;
                                this.cruiserRef[this.zaehler_cruiser].Front = false;
                                this.cruiserRef[this.zaehler_cruiser].Heck = false;
                                this.cruiserRef[this.zaehler_cruiser].middle = false;
                                this.cruiserRef[this.zaehler_cruiser].horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Heck";
                                this.pb_Store[x + 1, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Middle";
                                this.pb_Store[x + 2, y].Name = this.cruiserRef[this.zaehler_cruiser].name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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

                                cruiserRef[zaehler_cruiser].name = "Cruiser_" + zaehler_cruiser.ToString();
                                cruiserRef[zaehler_cruiser].posHeckX = x;
                                cruiserRef[zaehler_cruiser].posHeckY = y;
                                cruiserRef[zaehler_cruiser].shipDestryoed = false;
                                cruiserRef[zaehler_cruiser].Front = false;
                                cruiserRef[zaehler_cruiser].Heck = false;
                                cruiserRef[zaehler_cruiser].middle = false;
                                cruiserRef[zaehler_cruiser].horizontal = horizontal;

                                pb_Store[x, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Front";
                                pb_Store[x, y - 1].Name = cruiserRef[zaehler_cruiser].name + "_" + "Middle";
                                pb_Store[x, y - 2].Name = cruiserRef[zaehler_cruiser].name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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

                                cruiserRef[zaehler_cruiser].name = "Cruiser_" + zaehler_cruiser.ToString();
                                cruiserRef[zaehler_cruiser].posHeckX = x;
                                cruiserRef[zaehler_cruiser].posHeckY = y;
                                cruiserRef[zaehler_cruiser].shipDestryoed = false;
                                cruiserRef[zaehler_cruiser].Front = false;
                                cruiserRef[zaehler_cruiser].Heck = false;
                                cruiserRef[zaehler_cruiser].middle = false;
                                cruiserRef[zaehler_cruiser].horizontal = horizontal;

                                pb_Store[x, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Heck";
                                pb_Store[x, y + 1].Name = cruiserRef[zaehler_cruiser].name + "_" + "Middle";
                                pb_Store[x, y + 2].Name = cruiserRef[zaehler_cruiser].name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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
                case shipModels.boat: // boat
                    if (this.horizontal)
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
                                this.boatRef[this.zaehler_boat].name = "Boat_" + this.zaehler_boat.ToString();
                                this.boatRef[this.zaehler_boat].posHeckX = x;
                                this.boatRef[this.zaehler_boat].posHeckY = y;
                                this.boatRef[this.zaehler_boat].posFrontX = x - 1;
                                this.boatRef[this.zaehler_boat].posFrontY = y;
                                this.boatRef[this.zaehler_boat].shipDestryoed = false;
                                this.boatRef[this.zaehler_boat].Front = false;
                                this.boatRef[this.zaehler_boat].Heck = false;
                                this.boatRef[this.zaehler_boat].horizontal = this.horizontal;

                                this.pb_Store[x, y].Name = this.boatRef[this.zaehler_boat].name + "_" + "Front";
                                this.pb_Store[x - 1, y].Name = this.boatRef[this.zaehler_boat].name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                this.ships = shipModels.nothing;
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
                                this.boatRef[this.zaehler_boat].name = "Boat_" + this.zaehler_boat.ToString(); // Position sowie name des schiffes speichern
                                this.boatRef[this.zaehler_boat].posHeckX = x;
                                this.boatRef[this.zaehler_boat].posHeckY = y;
                                this.boatRef[this.zaehler_boat].posFrontX = x + 1;
                                this.boatRef[this.zaehler_boat].posFrontY = y;
                                this.boatRef[this.zaehler_boat].shipDestryoed = false;
                                this.boatRef[this.zaehler_boat].Front = false;
                                this.boatRef[this.zaehler_boat].Heck = false;
                                this.boatRef[this.zaehler_boat].horizontal = this.horizontal;
                                this.pb_Store[x, y].Name = this.boatRef[this.zaehler_boat].name + "_" + "Heck";
                                this.pb_Store[x + 1, y].Name = this.boatRef[this.zaehler_boat].name + "_" + "Front";
                                this.ships = shipModels.nothing; // Schiffsauswahl auf nothing setzen
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
                                boatRef[zaehler_boat].name = "Boat_" + zaehler_boat.ToString(); // Position sowie name des schiffes speichern
                                boatRef[zaehler_boat].posHeckX = x;
                                boatRef[zaehler_boat].posHeckY = y;
                                boatRef[zaehler_boat].posFrontX = x;
                                boatRef[zaehler_boat].posFrontY = y - 1;
                                boatRef[zaehler_boat].shipDestryoed = false;
                                boatRef[zaehler_boat].Front = false;
                                boatRef[zaehler_boat].Heck = false;
                                boatRef[zaehler_boat].horizontal = horizontal;
                                pb_Store[x, y].Name = boatRef[zaehler_boat].name + "_" + "Front";
                                pb_Store[x, y - 1].Name = boatRef[zaehler_boat].name + "_" + "Heck";
                                ships = shipModels.nothing; // Schiffsauswahl auf nothing setzen
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
                                boatRef[zaehler_boat].name = "Boat_" + zaehler_boat.ToString();
                                boatRef[zaehler_boat].posHeckX = x;
                                boatRef[zaehler_boat].posHeckY = y;
                                boatRef[zaehler_boat].posFrontX = x;
                                boatRef[zaehler_boat].posFrontY = y + 1;
                                boatRef[zaehler_boat].shipDestryoed = false;
                                boatRef[zaehler_boat].Front = false;
                                boatRef[zaehler_boat].Heck = false;
                                boatRef[zaehler_boat].horizontal = horizontal;

                                pb_Store[x, y].Name = boatRef[zaehler_boat].name + "_" + "Heck";
                                pb_Store[x, y + 1].Name = boatRef[zaehler_boat].name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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
                case shipModels.galley: // galley
                    if (this.horizontal)
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
                case shipModels.battleship: // battleship
                    if (this.horizontal)
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
                case shipModels.cruiser: // Cruiser
                    if (this.horizontal)
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
                case shipModels.boat: // boat
                    if (this.horizontal)
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
                BattleshipsForm.panelStatus.VerticalScroll.Value += BattleshipsForm.panelStatus.VerticalScroll.SmallChange;
                BattleshipsForm.panelStatus.Refresh();
            }
        }

        protected virtual void addControl(Control contr, int x, int y)
        {
            if (this.InvokeRequired)
            {
                AddControlCallback d = new AddControlCallback(this.addControl);
                this.Invoke(d, new object[] { contr, x, y });
            }
            else
            {
                // Die Explosion dem Panel zuordnen, in dem der Treffer war (Die Explosion wird somit vor dem Panelbild angezeigt)
                this.pb[x, y].Controls.Add(contr);
                //// Control[] s = this.Controls.Find(contr.Name, false);
                //// Die PictureBox in den Fordergrund bringen
                //// s[0].BringToFront();
            }
        }
    }
}