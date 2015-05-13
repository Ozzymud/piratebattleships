//-----------------------------------------------------------------------
// <copyright file="BattleshipsForm.cs" company="Team 17">
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

/// <summary>
/// Game upper surface (?? machine translated from German: Spieloberfläche)
/// </summary>
public partial class BattleshipsForm : DoubleBuffered.FormDoubleBuffered
    {
        /// <summary>
        /// Client Form
        /// </summary> 
        public static ClientGameForm clientGameForm;

        /// <summary>
        /// Host Form
        /// </summary>
        public static HostGameForm hostGameForm;

        /// <summary>
        /// Soundwiedergabe Objekt
        /// </summary>
        public static SoundClass soundPlayer;

        public static InfoForm infoForm;

        /// <summary>
        /// Label der den Status des Spiels anzeigt
        /// </summary>
        public static Label lblStatus;

        public static DoubleBuffered.PanelDoubleBuffered panelStatus;

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

        // Auflistung der Spielzüge
        public enum spielzug
        {
            player = 0,
            opponend = 1
        }

        // Speichert den Status, welcher Spieler gerade am Zug ist
        public static spielzug whosTurn;

        public static int zaehler_galley = 0;
        public static int zaehler_battleship = 0;
        public static int zaehler_cruiser = 0;
        public static int zaehler_boat = 0;

        #region Class constants
        private const short MAXANZAHLBATTLESHIP = 1; // 1 Schlachtschiff
        private const short MAXANZAHLGALLEY = 1; // 1 Galley
        private const short MAXANZAHLCRUISER = 3; // 3 Cruiser
        private const short MAXANZAHLBOAT = 3; // 3 Boote
        private const int BATTLEFIELDPLAYER_X = 40;
        private const int BATTLEFIELDPLAYER_Y = 180;
        private const int BATTLEFIELDOPPONNENT_X = 460;
        private const int BATTLEFIELDOPPONNENT_Y = 180;
        #endregion

        /// <summary>
        /// Initializes a new instance of the BattleshipsForm class.
        /// The game's interface constructor
        /// </summary>
        public BattleshipsForm()
        {
            try
            {
                this.InitializeComponent();

                // Spieler ist noch nicht bereit (Start des Spiels)
                playerReady2Play = false;
                opponendReady2Play = false;

                lanMenuItem = new ToolStripMenuItem("&Netzwerk");
                spielBeitretenMenuItem = new ToolStripMenuItem("&Spiel beitreten");
                spielHostenMenuItem = new ToolStripMenuItem("Spiel &hosten");
                helpMenuItem = new ToolStripMenuItem("?");
                infoMenuItem = new ToolStripMenuItem("&Info");

                spielBeitretenMenuItem.Click += new EventHandler(this.spielBeitretenToolStripMenuItem_Click);
                spielHostenMenuItem.Click += new EventHandler(this.spielHostenToolStripMenuItem_Click);
                infoMenuItem.Click += new EventHandler(this.infoMenuItem_Click);

                panelStatus = new DoubleBuffered.PanelDoubleBuffered();
                panelStatus.Location = new Point(597, 47);
                panelStatus.Size = new System.Drawing.Size(197, 100);
                panelStatus.AutoScroll = true;
                panelStatus.BackColor = Color.Transparent;
                panelStatus.VerticalScroll.SmallChange = 50;
                panelStatus.HorizontalScroll.Enabled = true;

                lblStatus = new Label();

                // lblStatus.Dock = DockStyle.Fill;
                lblStatus.TextAlign = ContentAlignment.TopLeft;
                lblStatus.AutoSize = true;
                lblStatus.Font = new System.Drawing.Font("Book Antiqua", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                panelStatus.Controls.Add(lblStatus);

                this.Controls.Add(panelStatus);

                lanMenuItem.DropDownItems.Add(spielHostenMenuItem);
                lanMenuItem.DropDownItems.Add(spielBeitretenMenuItem);
                helpMenuItem.DropDownItems.Add(infoMenuItem);
                this.menuStripMain.Items.Add(lanMenuItem);
                this.menuStripMain.Items.Add(helpMenuItem);

                soundPlayer = new SoundClass();

                battlefieldPlayer = new BattlefieldPlayer(BATTLEFIELDPLAYER_X, BATTLEFIELDPLAYER_Y);
                battlefieldOpponent = new BattlefieldOpponent(BATTLEFIELDOPPONNENT_X, BATTLEFIELDOPPONNENT_Y);

                // Schlachtfelder der Hautpform Hinzufügen
                this.Controls.Add(battlefieldPlayer);
                this.Controls.Add(battlefieldOpponent);

                SplashScreen splash = new SplashScreen();
                splash.showForm(); // Splashscreen anzeigen (langsam einblenden)
                Thread.Sleep(1000); // Splashscreen für 1sek. anzeigen
                splash.Close(); // Splashscreen schließen
                splash.Dispose(); // Ressourcen freigeben
            }
            finally
            {
                // HACK: this throws an error, but caught errors need to be fixed eventually i think
                // sloppy change to at least run program without crash on start
                //// catch (Exception ex)
                //// MessageBox.Show(this, ex.Message + " " + ex.InnerException.ToString());
            }
        }

        #region Form-Events
        private void BattleshipsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Wollen Sie das Spiel wirklich beenden?", "Beenden?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region Mouse-Events
        private void btnGalley_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // Linksklick
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_galley = zaehler_galley;
                        zaehler_galley++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.galley;

                        if (zaehler_galley == MAXANZAHLGALLEY)
                        {
                            this.btnGalley.Enabled = false;
                        }
                        
                        this.lblGalley.Text = "Number: " + (MAXANZAHLGALLEY - (zaehler_galley)).ToString();
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
                    this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_battleship = zaehler_battleship;
                        zaehler_battleship++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.battleship;

                        if (zaehler_battleship == MAXANZAHLBATTLESHIP)
                        {
                            this.btnBattleship.Enabled = false;
                        }
                        
                        this.lblBattleship.Text = "Number: " + (MAXANZAHLBATTLESHIP - (zaehler_battleship)).ToString();
                    }

                    break;
            }
        }

        private void btnCruiser_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_cruiser = zaehler_cruiser;
                        zaehler_cruiser++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.cruiser;

                        if (zaehler_cruiser == MAXANZAHLCRUISER)
                        {
                            this.btnCruiser.Enabled = false;
                        }
                        
                        this.lblCruiser.Text = "Number: " + (MAXANZAHLCRUISER - (zaehler_cruiser)).ToString();
                    }

                    break;
            }
        }

        private void btnBoat_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.shipModels.nothing))
                    {
                        battlefieldPlayer.zaehler_boat = zaehler_boat;
                        zaehler_boat++;
                        battlefieldPlayer.ships = BattlefieldPlayer.shipModels.boat;

                        if (zaehler_boat == MAXANZAHLBOAT)
                        {
                            this.btnBoat.Enabled = false;
                        }

                        this.lblBoat.Text = "Number: " + (MAXANZAHLBOAT - (zaehler_boat)).ToString();
                    }

                    break;
            }
        }

        private void btnGalley_MouseLeave(object sender, EventArgs e)
        {
            if (this.btnGalley.Enabled == false)
            {
                this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_disable;
            }
            else
            {
                this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley;
            }
        }

        private void btnBattleship_MouseLeave(object sender, EventArgs e)
        {
            if (this.btnBattleship.Enabled == false)
            {
                this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_disable;
            }
            else
            {
                this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z;
            }
        }

        private void btnCruiser_MouseLeave(object sender, EventArgs e)
        {
            if (this.btnCruiser.Enabled == false)
            {
                this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_disable;
            }
            else
            {
                this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser;
            }
        }

        private void btnBoat_MouseLeave(object sender, EventArgs e)
        {
            if (this.btnBoat.Enabled == false)
            {
                this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_disable;
            }
            else
            {
                this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat;
            }
        }

        private void btnGalley_MouseEnter(object sender, EventArgs e)
        {
            if (this.btnGalley.Enabled == false)
            {
                this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_disable;
            }
            else
            {
                this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_enter;
            }
        }

        private void btnBattleship_MouseEnter(object sender, EventArgs e)
        {
            if (this.btnBattleship.Enabled == false)
            {
                this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_disable;
            }
            else
            {
                this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_enter;
            }
        }

        private void btnCruiser_MouseEnter(object sender, EventArgs e)
        {
            if (this.btnCruiser.Enabled == false)
            {
                this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_disable;
            }
            else
            {
                this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_enter;
            }
        }

        private void btnBoat_MouseEnter(object sender, EventArgs e)
        {
            if (this.btnBoat.Enabled == false)
            {
                this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_disable;
            }
            else
            {
                this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_enter;
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