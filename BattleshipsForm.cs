/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: BattleshipsForm
 * Beschreibung:
 * Autor: Markus Bohnert
 * Team: Simon Hodler, Markus Bohnert
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DoubleBufferedUserControls;

namespace Battleships
{
    /// <summary>
    /// Spieloberfläche
    /// </summary>
    public partial class BattleshipsForm : Form_DoubleBuffered
    {
        /// <summary>
        /// Client-Form
        /// </summary> 
        public static ClientGameForm clientGameForm;
        /// <summary>
        /// Host-Form
        /// </summary>
        public static HostGameForm hostGameForm;

        /// <summary>
        /// Soundwiedergabe-Objekt
        /// </summary>
        public static SoundClass soundPlayer;

        public static InfoForm infoForm;

        /// <summary>
        /// Label der den Status des Spiels anzeigt
        /// </summary>
        public static Label lblStatus;

        public static DoubleBufferedUserControls.Panel_DoubleBuffered panelStatus;

        // MenuItems
        public static ToolStripMenuItem lanMenuItem;
        public static ToolStripMenuItem spielBeitretenMenuItem;
        public static ToolStripMenuItem spielHostenMenuItem;
        public static ToolStripMenuItem helpMenuItem;
        public static ToolStripMenuItem infoMenuItem;

        /// <summary>
        /// Das Schlachtfeld des Spielers
        /// </summary>
        public static BattlefieldPlayer battlefieldPlayer;
        /// <summary>
        /// Das Schlachtfeld des Gegners
        /// </summary>
        public static BattlefieldOpponent battlefieldOpponent;

        /// <summary>
        /// Flag der angibt ob der Spieler bereit ist
        /// </summary>
        public static bool playerReady2Play;
        /// <summary>
        /// Flag der angibt ob der Gegner bereit ist
        /// </summary>
        public static bool opponendReady2Play;

        /// <summary>
        /// Auflistung der Spielzüge
        /// </summary>
        public enum spielzug
        {
            player = 0,
            opponend = 1
        }
        /// <summary>
        /// Speichert den Status, welcher Spieler gerade am Zug ist
        /// </summary>
        public static spielzug whosTurn;

        public static int zaehler_galley = 0;
        public static int zaehler_battleship = 0;
        public static int zaehler_cruiser = 0;
        public static int zaehler_boat = 0;

        #region Klassenkonstanten
        // 1 Schlachtschiff
        private const short MAXANZAHLBATTLESHIP = 1;
        // 1 Galley
        private const short MAXANZAHLGALLEY = 1;
        // 3 Cruiser
        private const short MAXANZAHLCRUISER = 3;
        // 3 Boote
        private const short MAXANZAHLBOAT = 3;

        private const int BATTLEFIELDPLAYER_X = 40;
        private const int BATTLEFIELDPLAYER_Y = 180;

        private const int BATTLEFIELDOPPONNENT_X = 460;
        private const int BATTLEFIELDOPPONNENT_Y = 180;
        #endregion

        /// <summary>
        /// Konstruktor der Spieloberfläche
        /// </summary>
        public BattleshipsForm()
        {
            try
            {
                InitializeComponent();

                // Spieler ist noch nicht bereit (Start des Spiels)
                playerReady2Play = false;
                opponendReady2Play = false;

                lanMenuItem = new ToolStripMenuItem("&Netzwerk");
                spielBeitretenMenuItem = new ToolStripMenuItem("&Spiel beitreten");
                spielHostenMenuItem = new ToolStripMenuItem("Spiel &hosten");
                helpMenuItem = new ToolStripMenuItem("?");
                infoMenuItem = new ToolStripMenuItem("&Info");

                spielBeitretenMenuItem.Click += new EventHandler(spielBeitretenToolStripMenuItem_Click);
                spielHostenMenuItem.Click += new EventHandler(spielHostenToolStripMenuItem_Click);
                infoMenuItem.Click += new EventHandler(infoMenuItem_Click);

                panelStatus = new Panel_DoubleBuffered();
                panelStatus.Location = new Point(597, 47);
                panelStatus.Size = new System.Drawing.Size(197, 100);
                panelStatus.AutoScroll = true;
                panelStatus.BackColor = Color.Transparent;
                panelStatus.VerticalScroll.SmallChange = 50;
                panelStatus.HorizontalScroll.Enabled = true;

                lblStatus = new Label();
                //lblStatus.Dock = DockStyle.Fill;
                lblStatus.TextAlign = ContentAlignment.TopLeft;
                lblStatus.AutoSize = true;
                lblStatus.Font = new System.Drawing.Font("Book Antiqua", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                panelStatus.Controls.Add(lblStatus);

                this.Controls.Add(panelStatus);

                lanMenuItem.DropDownItems.Add(spielHostenMenuItem);
                lanMenuItem.DropDownItems.Add(spielBeitretenMenuItem);
                helpMenuItem.DropDownItems.Add(infoMenuItem);
                menuStripMain.Items.Add(lanMenuItem);
                menuStripMain.Items.Add(helpMenuItem);

                soundPlayer = new SoundClass();

                battlefieldPlayer = new BattlefieldPlayer(BATTLEFIELDPLAYER_X, BATTLEFIELDPLAYER_Y);
                battlefieldOpponent = new BattlefieldOpponent(BATTLEFIELDOPPONNENT_X, BATTLEFIELDOPPONNENT_Y);

                // Schlachtfelder der Hautpform Hinzufügen
                this.Controls.Add(battlefieldPlayer);
                this.Controls.Add(battlefieldOpponent);

                SplashScreen splash = new SplashScreen();
                // Splashscreen anzeigen (langsam einblenden)
                splash.showForm();
                // Splashscreen für 1sek. anzeigen
                Thread.Sleep(1000);
                // Splashscreen schließen
                splash.Close();
                // Ressourcen freigeben
                splash.Dispose();
            }
            //this throws an error
            //catch (Exception ex)
            //sloppy change to at least run
            finally
            {
                //MessageBox.Show(this, ex.Message + " " + ex.InnerException.ToString());
            }
        }

        #region Form-Events
        private void BattleshipsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Wollen Sie das Spiel wirklich beenden?", "Beenden?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                e.Cancel = true;
        }
        #endregion

        #region Mouse-Events
        private void btnGalley_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // Linksklick
                case System.Windows.Forms.MouseButtons.Left:
                    btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_click;
                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_galley = zaehler_galley;
                        zaehler_galley++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.galley;

                        if (zaehler_galley == MAXANZAHLGALLEY)
                        {
                            btnGalley.Enabled = false;
                        }
                        
                        lblGalley.Text = "Anzahl: " + (MAXANZAHLGALLEY - (zaehler_galley)).ToString();
                    }
                    break;
            }
        }

        private void btnBattleship_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // Linksklick
                case System.Windows.Forms.MouseButtons.Left:
                    btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_click;
                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_battleship = zaehler_battleship;
                        zaehler_battleship++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.battleship;

                        if (zaehler_battleship == MAXANZAHLBATTLESHIP)
                        {
                            btnBattleship.Enabled = false;
                        }
                        
                        lblBattleship.Text = "Anzahl: " + (MAXANZAHLBATTLESHIP - (zaehler_battleship)).ToString();
                    }
                    break;
            }

        }

        private void btnCruiser_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_click;
                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_cruiser = zaehler_cruiser;
                        zaehler_cruiser++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.cruiser;

                        if (zaehler_cruiser == MAXANZAHLCRUISER)
                        {
                            btnCruiser.Enabled = false;
                        }
                        
                        lblCruiser.Text = "Anzahl: " + (MAXANZAHLCRUISER - (zaehler_cruiser)).ToString();
                    }
                    break;
            }

        }

        private void btnBoat_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_click;
                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_boat = zaehler_boat;
                        zaehler_boat++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.boat;

                        if (zaehler_boat == MAXANZAHLBOAT)
                        {
                            btnBoat.Enabled = false;
                        }
                        lblBoat.Text = "Anzahl: " + (MAXANZAHLBOAT - (zaehler_boat)).ToString();
                    }
                    break;
            }
        }

        private void btnGalley_MouseLeave(object sender, EventArgs e)
        {
            if (btnGalley.Enabled == false)
            {
                btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_disable;
            }
            else
            {
                btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley;
            }
        }

        private void btnBattleship_MouseLeave(object sender, EventArgs e)
        {
            if (btnBattleship.Enabled == false)
            {
                btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_disable;
            }
            else
            {
                btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z;
            }
        }

        private void btnCruiser_MouseLeave(object sender, EventArgs e)
        {
            if (btnCruiser.Enabled == false)
            {
                btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_disable;
            }
            else
            {
                btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser;
            }
        }

        private void btnBoat_MouseLeave(object sender, EventArgs e)
        {
            if (btnBoat.Enabled == false)
            {
                btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_disable;
            }
            else
            {
                btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat;
            }
        }

        private void btnGalley_MouseEnter(object sender, EventArgs e)
        {
            if (btnGalley.Enabled == false)
            {
                btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_disable;
            }
            else
            {
                btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_enter;
            }
        }

        private void btnBattleship_MouseEnter(object sender, EventArgs e)
        {
            if (btnBattleship.Enabled == false)
            {
                btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_disable;
            }
            else
            {
                btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_enter;
            }
        }

        private void btnCruiser_MouseEnter(object sender, EventArgs e)
        {
            if (btnCruiser.Enabled == false)
            {
                btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_disable;
            }
            else
            {
                btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_enter;
            }
        }

        private void btnBoat_MouseEnter(object sender, EventArgs e)
        {
            if (btnBoat.Enabled == false)
            {
                btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_disable;
            }
            else
            {
                btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_enter;
            }
        }
        #endregion      

        #region ToolStripMenuItems-Events
        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();        
        }

        private void spielBeitretenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clientGameForm == null)
            { 
                clientGameForm = new ClientGameForm();
                spielHostenMenuItem.Enabled = false;
                clientGameForm.Show(this);
            }
            else if (!clientGameForm.IsHandleCreated)
            {
                clientGameForm = new ClientGameForm();
                spielHostenMenuItem.Enabled = false;
                clientGameForm.Show(this);
            }
        }

        private void spielHostenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hostGameForm == null)
            {
                hostGameForm = new HostGameForm();
                spielBeitretenMenuItem.Enabled = false;
                hostGameForm.Show(this);
            }
            else if (!hostGameForm.IsHandleCreated)
            {
                hostGameForm = new HostGameForm();
                spielBeitretenMenuItem.Enabled = false;
                hostGameForm.Show(this);
            }
        }

        private void infoMenuItem_Click(object sender, EventArgs e)
        {
            if (infoForm == null)
            {
                infoForm = new InfoForm();
                infoForm.ShowDialog();
            }
            else if (!infoForm.IsHandleCreated)
            {
                infoForm = new InfoForm();
                infoForm.ShowDialog();
            }
        }
        #endregion 

        private void button1_Click(object sender, EventArgs e)
        {
            lblStatus.Text += "Test\n";
            panelStatus.VerticalScroll.Value += panelStatus.VerticalScroll.SmallChange;
            panelStatus.Refresh();
        }

        private void panel_Status_Scroll(object sender, ScrollEventArgs e)
        {
            MessageBox.Show("TEst");
        }

        private void panel_Status_SizeChanged(object sender, EventArgs e)
        {
            MessageBox.Show("TEst");
        }

        private void BattleshipsForm_Load(object sender, EventArgs e)
        {

        }


    }
}

