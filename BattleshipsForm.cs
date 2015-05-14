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
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// Game upper surface (?? machine translated from German: Spieloberfläche)
/// </summary>
public partial class BattleshipsForm : Battleships.DoubleBufferedForm
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
        public static SoundClass SoundPlayer;

        public static InfoForm infoForm;

        /// <summary>
        /// Label der den Status des Spiels anzeigt
        /// </summary>
        public static Label lblStatus;

        public static Battleships.DoubleBufferedPanel PanelStatus;

        // MenuItems
        public static ToolStripMenuItem NetworkMenuItem;
        public static ToolStripMenuItem JoinGameMenuItem;
        public static ToolStripMenuItem HostGameMenuItem;
        public static ToolStripMenuItem HelpMenuItem;
        public static ToolStripMenuItem InfoMenuItem;

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
        public static bool PlayerReadyToPlay;

        /// <summary>
        /// Flag der angibt ob der Gegner bereit ist
        /// </summary>
        public static bool OpponentReadyToPlay;

        // List of the moves
        public enum TurnIdentifier
        {
            player = 0,
            enemy = 1
        }

        // Saves the state of whos turn it is
        public static TurnIdentifier WhosTurn;

        public static int CounterGalley = 0;
        public static int CounterBattleship = 0;
        public static int CounterCruiser = 0;
        public static int CounterBoat = 0;

        #region Class constants
        private const short MaxNumberBattleship = 1; // 1 Schlachtschiff
        private const short MaxNumberGalley = 1; // 1 Galley
        private const short MaxNumberCruiser = 3; // 3 Cruiser
        private const short MaxNumberBoat = 3; // 3 Boote
        private const int BattlefieldPlayerX = 40;
        private const int BattlefieldPlayerY = 180;
        private const int BattlefieldOpponentX = 460;
        private const int BattlefieldOpponentY = 180;
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
                PlayerReadyToPlay = false;
                OpponentReadyToPlay = false;

                NetworkMenuItem = new ToolStripMenuItem("&Network");
                JoinGameMenuItem = new ToolStripMenuItem("&Join game");
                HostGameMenuItem = new ToolStripMenuItem("&Host game");
                HelpMenuItem = new ToolStripMenuItem("&Help");
                InfoMenuItem = new ToolStripMenuItem("&About");

                JoinGameMenuItem.Click += new EventHandler(this.spielBeitretenToolStripMenuItem_Click);
                HostGameMenuItem.Click += new EventHandler(this.spielHostenToolStripMenuItem_Click);
                InfoMenuItem.Click += new EventHandler(this.InfoMenuItem_Click);

                PanelStatus = new Battleships.DoubleBufferedPanel();
                PanelStatus.Location = new Point(597, 47);
                PanelStatus.Size = new System.Drawing.Size(197, 100);
                PanelStatus.AutoScroll = true;
                PanelStatus.BackColor = Color.Transparent;
                PanelStatus.VerticalScroll.SmallChange = 50;
                PanelStatus.HorizontalScroll.Enabled = true;

                lblStatus = new Label();

                // lblStatus.Dock = DockStyle.Fill;
                lblStatus.TextAlign = ContentAlignment.TopLeft;
                lblStatus.AutoSize = true;
                lblStatus.Font = new System.Drawing.Font("Book Antiqua", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                PanelStatus.Controls.Add(lblStatus);

                this.Controls.Add(PanelStatus);

                NetworkMenuItem.DropDownItems.Add(HostGameMenuItem);
                NetworkMenuItem.DropDownItems.Add(JoinGameMenuItem);
                HelpMenuItem.DropDownItems.Add(InfoMenuItem);
                this.menuStripMain.Items.Add(NetworkMenuItem);
                this.menuStripMain.Items.Add(HelpMenuItem);

                SoundPlayer = new SoundClass();

                battlefieldPlayer = new BattlefieldPlayer(BattlefieldPlayerX, BattlefieldPlayerY);
                battlefieldOpponent = new BattlefieldOpponent(BattlefieldOpponentX, BattlefieldOpponentY);

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
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        battlefieldPlayer.CounterGalley = CounterGalley;
                        CounterGalley++;
                        battlefieldPlayer.ships = BattlefieldPlayer.ShipModels.galley;

                        if (CounterGalley == MaxNumberGalley)
                        {
                            this.btnGalley.Enabled = false;
                        }
                        
                        this.lblGalley.Text = "Number: " + (MaxNumberGalley - (CounterGalley)).ToString();
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
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        battlefieldPlayer.CounterBattleship = CounterBattleship;
                        CounterBattleship++;
                        battlefieldPlayer.ships = BattlefieldPlayer.ShipModels.battleship;

                        if (CounterBattleship == MaxNumberBattleship)
                        {
                            this.btnBattleship.Enabled = false;
                        }
                        
                        this.lblBattleship.Text = "Number: " + (MaxNumberBattleship - (CounterBattleship)).ToString();
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
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        battlefieldPlayer.CounterCruiser = CounterCruiser;
                        CounterCruiser++;
                        battlefieldPlayer.ships = BattlefieldPlayer.ShipModels.cruiser;

                        if (CounterCruiser == MaxNumberCruiser)
                        {
                            this.btnCruiser.Enabled = false;
                        }
                        
                        this.lblCruiser.Text = "Number: " + (MaxNumberCruiser - (CounterCruiser)).ToString();
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
                    if (!(battlefieldPlayer.ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        battlefieldPlayer.CounterBoat = CounterBoat;
                        CounterBoat++;
                        battlefieldPlayer.ships = BattlefieldPlayer.ShipModels.boat;

                        if (CounterBoat == MaxNumberBoat)
                        {
                            this.btnBoat.Enabled = false;
                        }

                        this.lblBoat.Text = "Number: " + (MaxNumberBoat - (CounterBoat)).ToString();
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
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();        
        }

        private void spielBeitretenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clientGameForm == null)
            { 
                clientGameForm = new ClientGameForm();
                HostGameMenuItem.Enabled = false;
                clientGameForm.Show(this);
            }
            else if (!clientGameForm.IsHandleCreated)
            {
                clientGameForm = new ClientGameForm();
                HostGameMenuItem.Enabled = false;
                clientGameForm.Show(this);
            }
        }

        private void spielHostenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hostGameForm == null)
            {
                hostGameForm = new HostGameForm();
                JoinGameMenuItem.Enabled = false;
                hostGameForm.Show(this);
            }
            else if (!hostGameForm.IsHandleCreated)
            {
                hostGameForm = new HostGameForm();
                JoinGameMenuItem.Enabled = false;
                hostGameForm.Show(this);
            }
        }

        private void InfoMenuItem_Click(object sender, EventArgs e)
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
            PanelStatus.VerticalScroll.Value += PanelStatus.VerticalScroll.SmallChange;
            PanelStatus.Refresh();
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