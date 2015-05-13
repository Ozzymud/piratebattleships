/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: BattlefieldPlayer
 * Beschreibung:
 * Autor: Markus Bohnert
 * Team: Simon Hodler, Markus Bohnert
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DoubleBufferedUserControls;

namespace Battleships
{
    public class BattlefieldPlayer : Panel_DoubleBuffered
    {
        delegate void addControlCallback(Control contr, int x, int y);
        delegate void setTextCallback(string text);
        delegate void showDestroyedShipsCallback(int[] args, bool horizontal);

        /// <summary>
        /// Auflistung der Schiffsmodelle
        /// </summary>
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
        public Panel_DoubleBuffered[,] pb = new Panel_DoubleBuffered[10, 10];
        /// <summary>
        /// Enthält eine Schattenkopie des Spielfeldes
        /// </summary>
        public Panel_DoubleBuffered[,] pb_Store = new Panel_DoubleBuffered[10, 10];

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
            horizontal = true;

            collisionColor = new Color();
            // Ein helles Rot
            collisionColor = Color.FromArgb(90, 210, 0, 0);

            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //this.BackgroundImageLayout = ImageLayout.Stretch;
            //this.BackgroundImage = Battleships.Properties.Resources.meer_player;
            this.BackColor = Color.Transparent;
            //this.BorderStyle = BorderStyle.FixedSingle;
            //this.BorderStyle = BorderStyle.None;

            //Matrix Spieler
            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; j < pb.GetLength(1); j++)
                {
                    Panel_DoubleBuffered p = new Panel_DoubleBuffered();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Padding = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.MouseClick += new MouseEventHandler(p_MouseClick);
                    p.MouseEnter += new EventHandler(p_MouseEnter);
                    p.MouseLeave += new EventHandler(p_MouseLeave);
                    //p.Visible = false;
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    pb[i, j] = p;
                    this.Controls.Add(p);

                    pb_Store[i, j] = new Panel_DoubleBuffered();
                }
            }
        }

        #region Mouse-Events
        public void p_MouseClick(object sender, MouseEventArgs e)
        {
            // Das Panel holen, welches das MouseClick-Event ausgelöst hat
            Panel_DoubleBuffered tmp = (Panel_DoubleBuffered)sender;

            // Linke Maustaste gedrückt
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Schiff an angeklickte Position setzen
                setShips(ref tmp);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Felder durchlaufen (PictureBox_DoubleBuffereden)
                for (int i = 0; i < pb.GetLength(0); i++)
                {
                    for (int j = 0; j < pb.GetLength(1); j++)
                    {
                        // Prüfen ob Feld ein Schiffsteil beihnaltet (Tag = 1)
                        if ((int)pb[i, j].Tag != (int)1)
                            // Wenn nein, dann Bild im Feld löschen
                            pb[i, j].BackgroundImage = null;
                    }
                }
                // Wert negieren
                horizontal = !horizontal;
                // Schiff zeichnen
                drawShips(ref tmp);
            }
        }

        public void p_MouseEnter(object sender, EventArgs e)
        {
            // Event wurde von einer Panel_DoubleBuffered ausgelöst...
            // Senderobjekt erhalten --> Panel welches das Event ausgelöst hat
            Panel_DoubleBuffered tmp = (Panel_DoubleBuffered)sender;
            // Schiff zeichnen
            drawShips(ref tmp);
        }

        public void p_MouseLeave(object sender, EventArgs e)
        {
            // Alle Panels durchlaufen
            for (int x = 0; x < pb.GetLength(0); x++)
            {
                for (int y = 0; y < pb.GetLength(1); y++)
                {
                    // Wenn aktuellen Panel kein Bild gespeichert sit
                    if ((int)pb[x, y].Tag != (int)1)
                    {
                        // Dann Bild löschen
                        pb[x, y].BackgroundImage = null;
                        // Bildflag auf false
                        pb[x, y].Tag = 0;
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
            if (pb[x, y].BackgroundImage == null) return false;
            else return true;
        }

        /// <summary>
        /// Setzt einen Treffer auf das angegebene Feld
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        public bool setImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.soundPlayer.playSoundAsync(BattleshipsForm.soundPlayer.currentSoundDir + "\\explo2.wav");
                // Explosion auf dem Spielfeld darstellen
                drawExplosion(x, y);
                // Überprüfen welches Schiff wo getroffen wurde
                checkShips(x, y);
                // Spielstatus prüfen
                if (checkGameStatus())
                {
                    // Gegner hat gewonnen, da alle Schiffe zerstört wurden
                    return true;
                }
                // Bis her hat noch keiner gewonnen...
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public void setMiss(int x, int y)
        {
            BattleshipsForm.soundPlayer.playSoundAsync(BattleshipsForm.soundPlayer.currentSoundDir + "\\platsch3.wav");
            // Fehlschuss auf dem Spielfeld darstellen
            drawMiss(x, y);
        }

        private void drawMiss(int x, int y)
        {
            //PictureBox missPicture = new PictureBox();
            //missPicture.Name = "miss_" + x.ToString() + ":" + y.ToString();
            //missPicture.Location = new Point(x * 30, y * 30);
            //missPicture.Size = new Size(30, 30);
            //missPicture.Margin = new Padding(0);
            //missPicture.Padding = new Padding(0);
            //missPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            //missPicture.BackColor = Color.Transparent;
            //missPicture.Image = Properties.Resources.platsch;

            //addControl(missPicture, x, y);
            pb[x, y].BackgroundImage = Properties.Resources.splash;
            pb[x, y].Tag = 1;
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
                showDestroyedShipsCallback d = new showDestroyedShipsCallback(showDestroyedBoat);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                BattleshipsForm.soundPlayer.playSoundAsync(BattleshipsForm.soundPlayer.currentSoundDir + "\\explo_big1.wav");
                // Explosionsbild an der angegeben Stelle entfernen (Control entfernen --> PictureBox)
                pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                if (horizontal)
                {
                    pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_h2;
                    pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_h1;
                }
                else
                {
                    pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v1;
                    pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                }
            }
        }

        /// <summary>
        /// Zeigt auf dem Gegnerspielfeld einen zerstörten Cruiser an
        /// </summary>
        /// <param name="args">Enthält die Coordinaten des Schiffes</param>
        /// <param name="horizontal">Gibt an ob das Schiff horizontal oder vertikal gesetzt wurde</param>
        public void showDestroyedCruiser(int[] args, bool horizontal)
        {
            if (this.InvokeRequired)
            {
                showDestroyedShipsCallback d = new showDestroyedShipsCallback(showDestroyedCruiser);
                this.Invoke(d, new object[] { args, horizontal });
            }
            else
            {
                // ToDo: Siehe showDestroyedBoat
                BattleshipsForm.soundPlayer.playSoundAsync(BattleshipsForm.soundPlayer.currentSoundDir + "\\explo_big1.wav");
                // Explosionsbild an der angegebeben Stelle entfernen (Control entfernen --> PictureBox)
                pb[args[0], args[1]].Controls.RemoveByKey("expl_" + args[0].ToString() + ":" + args[1].ToString());
                pb[args[2], args[3]].Controls.RemoveByKey("expl_" + args[2].ToString() + ":" + args[3].ToString());
                pb[args[4], args[5]].Controls.RemoveByKey("expl_" + args[4].ToString() + ":" + args[5].ToString());

                if (horizontal)
                {
                    pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_h1;
                    pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_h2;
                    pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_h3;
                }
                else
                {
                    pb[args[0], args[1]].BackgroundImage = Properties.Resources.cruiser_dmg_v1;
                    pb[args[2], args[3]].BackgroundImage = Properties.Resources.cruiser_dmg_v2;
                    pb[args[4], args[5]].BackgroundImage = Properties.Resources.cruiser_dmg_v3;
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
            for (int i = 0; i < boatRef.Length; i++)
            {
                // Überprüfen ob alle Boote zerstört sind
                if (!boatRef[i].shipDestryoed)
                    // Wenn auch nur noch ein Boot existiert, dann ist das Spiel nocht nicht vorbei
                    return false;
            }
            // Alle Cruiser überprüfen
            for (int i = 0; i < cruiserRef.Length; i++)
            {
                // Überprüfen ob alle Cruiser zerstört sind
                if (!cruiserRef[i].shipDestryoed)
                    // Wenn auch nur noch ein Cruiser existiert, dann ist das Spiel noch nicht vorbei
                    return false;
            }
            // Die Galley überprüfen
            if (!galleyRef.shipDestryoed)
                // Existiert die Galley noch, dann ist das Spiel nocht nicht vorbei
                return false;
            // Das Battleship überprüfen
            if (!battleshipRef.shipDestryoed)
                // Exisitiert das Battleship noch, dann ist das Spiel noch nicht vorbei
                return false;

            // Wurden ALLE Schiffe zerstört, dann ist das Spiel vorbei und DU hast verloren!!
            return true;
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
            if (pb_Store[x, y].Name.StartsWith("Boat_"))
            {
                // Die Nr. des Bootes herausfinden (von 1-3)
                string[] sBoat = pb_Store[x, y].Name.Split('_');
                int boatNr = int.Parse(sBoat[1]);
                string boatPart = sBoat[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (boatPart)
                {
                    case ("Heck"):
                        // Heck wurde getroffen (Horizontal)
                        boatRef[boatNr].Heck = true;
                        setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde am Heck getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        boatRef[boatNr].Front = true;
                        setTextLblStatus("Boat Nr. " + boatNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Boot komplett zerstört ist
                if (boatRef[boatNr].Front && boatRef[boatNr].Heck)
                {
                    boatRef[boatNr].shipDestryoed = true;
                    // Das zerstörte Boot auf dem Spielfeld darstellen
                    showDestroyedBoat(new int[4] { 
                        boatRef[boatNr].posHeckX, boatRef[boatNr].posHeckY,
                        boatRef[boatNr].posFrontX, boatRef[boatNr].posFrontY },
                        boatRef[boatNr].horizontal);
                    BattleshipsForm.battlefieldOpponent.showDestroyedBoat(new int[4] { 
                        boatRef[boatNr].posHeckX, boatRef[boatNr].posHeckY,
                        boatRef[boatNr].posFrontX, boatRef[boatNr].posFrontY },
                        boatRef[boatNr].horizontal);
                    setTextLblStatus("\nBoat Nr. " + boatNr.ToString() + " destroyed!");
                }
            }
            // Wurde ein Cruiser getroffen?
            else if (pb_Store[x, y].Name.StartsWith("Cruiser_"))
            {
                string[] sCruiser = pb_Store[x, y].Name.Split('_');
                int cruiserNr = int.Parse(sCruiser[1]);
                string cruiserPart = sCruiser[2];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (cruiserPart)
                {
                    case ("Heck"):
                        // Heck wurde getroffen (Horizontal)
                        cruiserRef[cruiserNr].Heck = true;
                        setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Heck getroffen!\n");
                        break;
                    case ("Middle"):
                        // Mittelteil wurde getroffen
                        cruiserRef[cruiserNr].middle = true;
                        setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde am Mittelteil getroffen!\n");
                        break;
                    case ("Front"):
                        // Front wurde getroffen (Horizontal)
                        cruiserRef[cruiserNr].Front = true;
                        setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob der Cruiser komplett zerstört wurde
                if (cruiserRef[cruiserNr].Heck && cruiserRef[cruiserNr].middle && cruiserRef[cruiserNr].Front)
                {
                    cruiserRef[cruiserNr].shipDestryoed = true;
                    // Den zerstörten Cruiser auf dem Spielfeld darstellen
                    showDestroyedCruiser(new int[6] {
                        cruiserRef[cruiserNr].posHeckX, cruiserRef[cruiserNr].posHeckY,
                        cruiserRef[cruiserNr].posMiddleX, cruiserRef[cruiserNr].posMiddleY,
                        cruiserRef[cruiserNr].posFrontX, cruiserRef[cruiserNr].posFrontY },
                        cruiserRef[cruiserNr].horizontal);
                    BattleshipsForm.battlefieldOpponent.showDestroyedCruiser(new int[6] {
                        cruiserRef[cruiserNr].posHeckX, cruiserRef[cruiserNr].posHeckY,
                        cruiserRef[cruiserNr].posMiddleX, cruiserRef[cruiserNr].posMiddleY,
                        cruiserRef[cruiserNr].posFrontX, cruiserRef[cruiserNr].posFrontY },
                        cruiserRef[cruiserNr].horizontal);
                    setTextLblStatus("Cruiser Nr. " + cruiserNr.ToString() + " destroyed!\n");
                }
            }
            // Wurde die Galley getroffen?
            else if (pb_Store[x, y].Name.StartsWith("Galley_"))
            {
                string[] sGalley = pb_Store[x, y].Name.Split('_');
                string galleyPart = sGalley[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (galleyPart)
                {
                    case ("Heck"):
                        galleyRef.Heck = true;
                        setTextLblStatus("Galley wurde am Heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        galleyRef.middle1 = true;
                        setTextLblStatus("Galley wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        galleyRef.middle2 = true;
                        setTextLblStatus("Galley wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        galleyRef.Front = true;
                        setTextLblStatus("Galley wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob die Galley komplett zerstört wurde
                if (galleyRef.Heck && galleyRef.middle1 && galleyRef.middle2 && galleyRef.Front)
                {
                    galleyRef.shipDestryoed = true;
                    setTextLblStatus("Galley destroyed!\n");
                }
            }
            // Wurde das Battleship getroffen?
            else if (pb_Store[x, y].Name.StartsWith("Battleship_"))
            {
                string[] sBattleship = pb_Store[x, y].Name.Split('_');
                string battleshipPart = sBattleship[1];

                // Welchen Teil des Schiffes hat es erwischt?
                switch (battleshipPart)
                {
                    case ("Heck"):
                        battleshipRef.Heck = true;
                        setTextLblStatus("Battleship wurde am Heck getroffen!\n");
                        break;
                    case ("Middle1"):
                        battleshipRef.middle1 = true;
                        setTextLblStatus("Battleship wurde am Mittelteil 1 getroffen!\n");
                        break;
                    case ("Middle2"):
                        battleshipRef.middle2 = true;
                        setTextLblStatus("Battleship wurde am Mittelteil 2 getroffen!\n");
                        break;
                    case ("Front"):
                        battleshipRef.Front = true;
                        setTextLblStatus("Battleship wurde an der Front getroffen!\n");
                        break;
                }

                // Überprüfen ob das Battleship komplett zerstört wurde
                if (battleshipRef.Heck && battleshipRef.middle1 && battleshipRef.middle2 && battleshipRef.Front)
                {
                    battleshipRef.shipDestryoed = true;
                    setTextLblStatus("Battleship destroyed!\n");
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
            //PictureBox_DoubleBuffered explPicture = new PictureBox_DoubleBuffered();
            PictureBox explPicture = new PictureBox();
            explPicture.Name = "expl_" + x.ToString() + ":" + y.ToString();
            //explPicture.Location = new Point(x * 30, y * 30);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explo6;
            // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
            addControl(explPicture, x, y);
        }

        /// <summary>
        /// Setzt die Schiffsteile an den jeweiligen Positionen fest (Bei einem MouseClick-Event)
        /// </summary>
        /// <param name="tmp">Das Panel welches das MouseClick-Event ausgelöst hat (als Referenz)</param>
        private void setShips(ref Panel_DoubleBuffered tmp)
        {
            String positionString = tmp.Name;
            // pb_ aus dem String entfernen
            positionString = positionString.Remove(0, 3);
            // x und y Position
            String[] position = positionString.Split(':');
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (ships)
            {
                // galley
                case shipModels.galley:
                    if (horizontal)
                    {
                        // Horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != (int)1 && (int)pb[x - 1, y].Tag != (int)1 && (int)pb[x - 2, y].Tag != (int)1 && (int)pb[x - 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                                tmp.Tag = 1;
                                pb[x - 1, y].Tag = 1;
                                pb[x - 2, y].Tag = 1;
                                pb[x - 3, y].Tag = 1;

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
                                pb_Store[x - 1, y].Name = galleyRef.name + "_" + "Middle2";
                                pb_Store[x - 2, y].Name = galleyRef.name + "_" + "Middle1";
                                pb_Store[x - 3, y].Name = galleyRef.name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x - 1, y].BackColor = collisionColor;
                                pb[x - 2, y].BackColor = collisionColor;
                                pb[x - 3, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x - 1, y].BackColor = Color.Transparent;
                                pb[x - 2, y].BackColor = Color.Transparent;
                                pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        else
                        {
                            if ((int)tmp.Tag != (int)1 && (int)pb[x + 1, y].Tag != (int)1 && (int)pb[x + 2, y].Tag != (int)1 && (int)pb[x + 3, y].Tag != (int)1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
                                tmp.Tag = 1;
                                pb[x + 1, y].Tag = 1;
                                pb[x + 2, y].Tag = 1;
                                pb[x + 3, y].Tag = 1;

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
                                pb_Store[x + 1, y].Name = galleyRef.name + "_" + "Middle1";
                                pb_Store[x + 2, y].Name = galleyRef.name + "_" + "Middle2";
                                pb_Store[x + 3, y].Name = galleyRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x + 1, y].BackColor = collisionColor;
                                pb[x + 2, y].BackColor = collisionColor;
                                pb[x + 3, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x + 1, y].BackColor = Color.Transparent;
                                pb[x + 2, y].BackColor = Color.Transparent;
                                pb[x + 3, y].BackColor = Color.Transparent;
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
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        else
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
                // battleship
                case shipModels.battleship:
                    if (horizontal)
                    {
                        // Horizontal
                        // battleship ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1 && (int)pb[x - 2, y].Tag != 1 && (int)pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                                tmp.Tag = 1;
                                pb[x - 1, y].Tag = 1;
                                pb[x - 2, y].Tag = 1;
                                pb[x - 3, y].Tag = 1;

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;

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
                                pb_Store[x - 1, y].Name = battleshipRef.name + "_" + "Middle2";
                                pb_Store[x - 2, y].Name = battleshipRef.name + "_" + "Middle1";
                                pb_Store[x - 3, y].Name = battleshipRef.name + "_" + "Heck";
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x - 1, y].BackColor = collisionColor;
                                pb[x - 2, y].BackColor = collisionColor;
                                pb[x - 3, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x - 1, y].BackColor = Color.Transparent;
                                pb[x - 2, y].BackColor = Color.Transparent;
                                pb[x - 3, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1 && (int)pb[x + 2, y].Tag != 1 && (int)pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
                                tmp.Tag = 1;
                                pb[x + 1, y].Tag = 1;
                                pb[x + 2, y].Tag = 1;
                                pb[x + 3, y].Tag = 1;

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
                                pb_Store[x + 1, y].Name = battleshipRef.name + "_" + "Middle1";
                                pb_Store[x + 2, y].Name = battleshipRef.name + "_" + "Middle2";
                                pb_Store[x + 3, y].Name = battleshipRef.name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x + 1, y].BackColor = collisionColor;
                                pb[x + 2, y].BackColor = collisionColor;
                                pb[x + 3, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x + 1, y].BackColor = Color.Transparent;
                                pb[x + 2, y].BackColor = Color.Transparent;
                                pb[x + 3, y].BackColor = Color.Transparent;
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
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        else
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
                // Cruiser
                case shipModels.cruiser:
                    if (horizontal)
                    {
                        // Horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1 && (int)pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                tmp.Tag = 1;
                                pb[x - 1, y].Tag = 1;
                                pb[x - 2, y].Tag = 1;

                                cruiserRef[zaehler_cruiser].name = "Cruiser_" + zaehler_cruiser.ToString();
                                cruiserRef[zaehler_cruiser].posHeckX = x;
                                cruiserRef[zaehler_cruiser].posHeckY = y;
                                cruiserRef[zaehler_cruiser].shipDestryoed = false;
                                cruiserRef[zaehler_cruiser].Front = false;
                                cruiserRef[zaehler_cruiser].Heck = false;
                                cruiserRef[zaehler_cruiser].middle = false;
                                cruiserRef[zaehler_cruiser].horizontal = horizontal;

                                pb_Store[x, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Front";
                                pb_Store[x - 1, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Middle";
                                pb_Store[x - 2, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x - 1, y].BackColor = collisionColor;
                                pb[x - 2, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x - 1, y].BackColor = Color.Transparent;
                                pb[x - 2, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1 && (int)pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                tmp.Tag = 1;
                                pb[x + 1, y].Tag = 1;
                                pb[x + 2, y].Tag = 1;

                                cruiserRef[zaehler_cruiser].name = "Cruiser_" + zaehler_cruiser.ToString();
                                cruiserRef[zaehler_cruiser].posHeckX = x;
                                cruiserRef[zaehler_cruiser].posHeckY = y;
                                cruiserRef[zaehler_cruiser].shipDestryoed = false;
                                cruiserRef[zaehler_cruiser].Front = false;
                                cruiserRef[zaehler_cruiser].Heck = false;
                                cruiserRef[zaehler_cruiser].middle = false;
                                cruiserRef[zaehler_cruiser].horizontal = horizontal;

                                pb_Store[x, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Heck";
                                pb_Store[x + 1, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Middle";
                                pb_Store[x + 2, y].Name = cruiserRef[zaehler_cruiser].name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x + 1, y].BackColor = collisionColor;
                                pb[x + 2, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x + 1, y].BackColor = Color.Transparent;
                                pb[x + 2, y].BackColor = Color.Transparent;
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
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        else
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
                // boat
                case shipModels.boat:
                    if (horizontal)
                    {
                        // Horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                                tmp.Tag = 1;
                                pb[x - 1, y].Tag = 1;

                                // Position sowie name des Schiffes speichern
                                boatRef[zaehler_boat].name = "Boat_" + zaehler_boat.ToString();
                                boatRef[zaehler_boat].posHeckX = x;
                                boatRef[zaehler_boat].posHeckY = y;
                                boatRef[zaehler_boat].posFrontX = x - 1;
                                boatRef[zaehler_boat].posFrontY = y;
                                boatRef[zaehler_boat].shipDestryoed = false;
                                boatRef[zaehler_boat].Front = false;
                                boatRef[zaehler_boat].Heck = false;
                                boatRef[zaehler_boat].horizontal = horizontal;

                                pb_Store[x, y].Name = boatRef[zaehler_boat].name + "_" + "Front";
                                pb_Store[x - 1, y].Name = boatRef[zaehler_boat].name + "_" + "Heck";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x - 1, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x - 1, y].BackColor = Color.Transparent;
                                return;
                            }
                        }
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
                                tmp.Tag = 1;
                                pb[x + 1, y].Tag = 1;

                                // Position sowie name des schiffes speichern
                                boatRef[zaehler_boat].name = "Boat_" + zaehler_boat.ToString();
                                boatRef[zaehler_boat].posHeckX = x;
                                boatRef[zaehler_boat].posHeckY = y;
                                boatRef[zaehler_boat].posFrontX = x + 1;
                                boatRef[zaehler_boat].posFrontY = y; ;
                                boatRef[zaehler_boat].shipDestryoed = false;
                                boatRef[zaehler_boat].Front = false;
                                boatRef[zaehler_boat].Heck = false;
                                boatRef[zaehler_boat].horizontal = horizontal;

                                pb_Store[x, y].Name = boatRef[zaehler_boat].name + "_" + "Heck";
                                pb_Store[x + 1, y].Name = boatRef[zaehler_boat].name + "_" + "Front";

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
                            }
                            else
                            {
                                // Anzeigen, dass hier kein Schiff plaziert werden kann (entsprechende Felder rot blinken lassen)
                                tmp.BackColor = collisionColor;
                                pb[x + 1, y].BackColor = collisionColor;
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(500);
                                tmp.BackColor = Color.Transparent;
                                pb[x + 1, y].BackColor = Color.Transparent;
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

                                // Position sowie name des schiffes speichern
                                boatRef[zaehler_boat].name = "Boat_" + zaehler_boat.ToString();
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

                                // Schiffsauswahl auf nothing setzen
                                ships = shipModels.nothing;
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
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        else
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
        private void drawShips(ref Panel_DoubleBuffered tmp)
        {
            String positionString = tmp.Name;
            // pb_ aus dem String entfernen
            positionString = positionString.Remove(0, 3);
            // x und y Position
            String[] position = positionString.Split(':');
            int x = int.Parse(position[0]);
            int y = int.Parse(position[1]);

            switch (ships)
            {
                // galley
                case shipModels.galley:
                    if (horizontal)
                    {
                        // Horizontal
                        // galley ist 4 Fleder groß, wenn Feld 7 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 7)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1 && (int)pb[x - 2, y].Tag != 1 && (int)pb[x - 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley4h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.galley1h;
                            }
                        }
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1 && (int)pb[x + 2, y].Tag != 1 && (int)pb[x + 3, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.galley1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.galley2h;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.galley3h;
                                pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.galley4h;
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
                        // Ansonsten galley in normaler Richtung zusammenbauen (4 Felder)
                        else
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
                // battleship
                case shipModels.battleship:
                    if (horizontal)
                    {
                        // Horizontal
                        // battleship ist 5 Fleder groß, wenn Feld 6 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 6)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1 && (int)pb[x - 2, y].Tag != 1 && (int)pb[x - 3, y].Tag != 1 && (int)pb[x - 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z41;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                pb[x - 3, y].BackgroundImage = Battleships.Properties.Resources.z11;
                            }
                        }
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1 && (int)pb[x + 2, y].Tag != 1 && (int)pb[x + 3, y].Tag != 1 && (int)pb[x + 4, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.z11;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.z21;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.z31;
                                pb[x + 3, y].BackgroundImage = Battleships.Properties.Resources.z41;
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
                        // Ansonsten battleship in normaler Richtung zusammenbauen (5 Felder)
                        else
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
                // Cruiser
                case shipModels.cruiser:
                    if (horizontal)
                    {
                        // Horizontal
                        // Cruiser ist 3 Fleder groß, wenn Feld 8 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 8)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1 && (int)pb[x - 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser3h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                pb[x - 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                            }
                        }
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1 && (int)pb[x + 2, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.cruiser1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.cruiser2h;
                                pb[x + 2, y].BackgroundImage = Battleships.Properties.Resources.cruiser3h;
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
                        // Ansonsten Cruiser in normaler Richtung zusammenbauen (3 Felder)
                        else
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
                // boat
                case shipModels.boat:
                    if (horizontal)
                    {
                        // Horizontal
                        // boat ist 1 Fled groß, wenn Feld 9 erreicht, dann Schiff in gegengesetzte Richtung aufbauen
                        if (x >= 9)
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x - 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat2h;
                                pb[x - 1, y].BackgroundImage = Battleships.Properties.Resources.boat1h;
                            }
                        }
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        else
                        {
                            if ((int)tmp.Tag != 1 && (int)pb[x + 1, y].Tag != 1)
                            {
                                tmp.BackgroundImage = Battleships.Properties.Resources.boat1h;
                                pb[x + 1, y].BackgroundImage = Battleships.Properties.Resources.boat2h;
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
                        // Ansonsten boat in normaler Richtung zusammenbauen (2 Felder)
                        else
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
                setTextCallback d = new setTextCallback(setTextLblStatus);
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
                addControlCallback d = new addControlCallback(addControl);
                this.Invoke(d, new object[] { contr, x, y });
            }
            else
            {
                // Die Explosion dem Panel zuordnen, in dem der Treffer war (Die Explosion wird somit vor dem Panelbild angezeigt)
                pb[x, y].Controls.Add(contr);
                //Control[] s = this.Controls.Find(contr.Name, false);
                //// Die PictureBox in den Fordergrund bringen
                //s[0].BringToFront();
            }
        }
    }
}

