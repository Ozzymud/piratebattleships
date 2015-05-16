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
// <project>Battleships Pirate Edition</project>
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
/// The main game interface.
/// </summary>
public partial class BattleshipsForm : Battleships.DoubleBufferedForm
    {
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
        /// Client Form.
        /// </summary> 
        public static ClientGameForm ClientGameForm;

        /// <summary>
        /// Host Form.
        /// </summary>
        public static HostGameForm HostGameForm;

        /// <summary>
        /// Sound playback object.
        /// </summary>
        public static SoundClass SoundPlayer;

        private static InfoForm InfoForm;

        /// <summary>
        /// The label displays the status of the game.
        /// </summary>
        public static Label LabelStatus;

        public static Battleships.DoubleBufferedPanel PanelStatus;

        // MenuItems
        private static ToolStripMenuItem networkMenuItem;
        public static ToolStripMenuItem JoinGameMenuItem;
        public static ToolStripMenuItem HostGameMenuItem;
        private static ToolStripMenuItem helpMenuItem;
        private static ToolStripMenuItem infoMenuItem;

        /// <summary>
        /// The battlefield of the player.
        /// </summary>
        public static BattlefieldPlayer BattlefieldPlayer;

        /// <summary>
        /// The battlefield of the enemy.
        /// </summary>
        public static BattlefieldOpponent BattlefieldOpponent;

        /// <summary>
        /// Flag indicating whether the player is ready.
        /// </summary>
        public static bool PlayerReadyToPlay;

        /// <summary>
        /// Flag indicating whether the enemy is ready.
        /// </summary>
        public static bool OpponentReadyToPlay;

        /// <summary>
        /// List of players.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the BattleshipsForm class.
        /// The game's interface constructor.
        /// </summary>
        public BattleshipsForm()
        {
            ////try
            ////{
                this.InitializeComponent();

                // Spieler ist noch nicht bereit (Start des Spiels)
                PlayerReadyToPlay = false;
                OpponentReadyToPlay = false;

                networkMenuItem = new ToolStripMenuItem("&Network");
                JoinGameMenuItem = new ToolStripMenuItem("&Join game");
                HostGameMenuItem = new ToolStripMenuItem("&Host game");
                helpMenuItem = new ToolStripMenuItem("&Help");
                infoMenuItem = new ToolStripMenuItem("&About");

                JoinGameMenuItem.Click += new EventHandler(this.JoinGameToolStripMenuItem_Click);
                HostGameMenuItem.Click += new EventHandler(this.HostGameToolStripMenuItem_Click);
                infoMenuItem.Click += new EventHandler(this.InfoMenuItem_Click);

                PanelStatus = new Battleships.DoubleBufferedPanel();
                PanelStatus.Location = new Point(597, 47);
                PanelStatus.Size = new System.Drawing.Size(197, 100);
                PanelStatus.AutoScroll = true;
                PanelStatus.BackColor = Color.Transparent;
                PanelStatus.VerticalScroll.SmallChange = 50;
                PanelStatus.HorizontalScroll.Enabled = true;

                LabelStatus = new Label();

                // LabelStatus.Dock = DockStyle.Fill;
                LabelStatus.TextAlign = ContentAlignment.TopLeft;
                LabelStatus.AutoSize = true;
                LabelStatus.Font = new System.Drawing.Font("Book Antiqua", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                PanelStatus.Controls.Add(LabelStatus);

                this.Controls.Add(PanelStatus);

                networkMenuItem.DropDownItems.Add(HostGameMenuItem);
                networkMenuItem.DropDownItems.Add(JoinGameMenuItem);
                helpMenuItem.DropDownItems.Add(infoMenuItem);
                this.menuStripMain.Items.Add(networkMenuItem);
                this.menuStripMain.Items.Add(helpMenuItem);

                SoundPlayer = new SoundClass();

                BattlefieldPlayer = new BattlefieldPlayer(BattlefieldPlayerX, BattlefieldPlayerY);
                BattlefieldOpponent = new BattlefieldOpponent(BattlefieldOpponentX, BattlefieldOpponentY);

                // Schlachtfelder der Hautpform Hinzufügen
                this.Controls.Add(BattlefieldPlayer);
                this.Controls.Add(BattlefieldOpponent);

                SplashScreen splash = new SplashScreen();
                splash.ShowForm(); // Splashscreen anzeigen (langsam einblenden)
                Thread.Sleep(1000); // Splashscreen für 1sek. anzeigen
                splash.Close(); // Splashscreen schließen
                splash.Dispose(); // Ressourcen freigeben
            ////}
            ////finally
            ////{
                // HACK: this throws an error, but caught errors need to be fixed eventually i think
                // sloppy change to at least run program without crash on start
                //// catch (Exception ex)
                //// MessageBox.Show(this, ex.Message + " " + ex.InnerException.ToString());
            ////}
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
        private void ButtonGalleyMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // Linksklick
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        BattlefieldPlayer.CounterGalley = CounterGalley;
                        CounterGalley++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.galley;

                        if (CounterGalley == MaxNumberGalley)
                        {
                            this.btnGalley.Enabled = false;
                        }
                        
                        this.lblGalley.Text = "Number: " + (MaxNumberGalley - (CounterGalley)).ToString();
                    }

                    break;
            }
        }

        private void ButtonBattleshipMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // Linksklick
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnBattleship.BackgroundImage = Battleships.Properties.Resources.btn_z_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        BattlefieldPlayer.CounterBattleship = CounterBattleship;
                        CounterBattleship++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.battleship;

                        if (CounterBattleship == MaxNumberBattleship)
                        {
                            this.btnBattleship.Enabled = false;
                        }
                        
                        this.lblBattleship.Text = "Number: " + (MaxNumberBattleship - (CounterBattleship)).ToString();
                    }

                    break;
            }
        }

        private void ButtonCruiserMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnCruiser.BackgroundImage = Battleships.Properties.Resources.btn_cruiser_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        BattlefieldPlayer.CounterCruiser = CounterCruiser;
                        CounterCruiser++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.cruiser;

                        if (CounterCruiser == MaxNumberCruiser)
                        {
                            this.btnCruiser.Enabled = false;
                        }
                        
                        this.lblCruiser.Text = "Number: " + (MaxNumberCruiser - (CounterCruiser)).ToString();
                    }

                    break;
            }
        }

        private void ButtonBoatMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnBoat.BackgroundImage = Battleships.Properties.Resources.btn_boat_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.nothing))
                    {
                        BattlefieldPlayer.CounterBoat = CounterBoat;
                        CounterBoat++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.boat;

                        if (CounterBoat == MaxNumberBoat)
                        {
                            this.btnBoat.Enabled = false;
                        }

                        this.lblBoat.Text = "Number: " + (MaxNumberBoat - (CounterBoat)).ToString();
                    }

                    break;
            }
        }

        private void ButtonGalleyMouseLeave(object sender, EventArgs e)
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

        private void ButtonBattleshipMouseLeave(object sender, EventArgs e)
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

        private void ButtonCruiserMouseLeave(object sender, EventArgs e)
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

        private void ButtonBoatMouseLeave(object sender, EventArgs e)
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

        private void ButtonGalleyMouseEnter(object sender, EventArgs e)
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

        private void ButtonBattleshipMouseEnter(object sender, EventArgs e)
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

        private void ButtonCruiserMouseEnter(object sender, EventArgs e)
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

        private void ButtonBoatMouseEnter(object sender, EventArgs e)
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
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();        
        }

        private void JoinGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ClientGameForm == null)
            { 
                ClientGameForm = new ClientGameForm();
                HostGameMenuItem.Enabled = false;
                ClientGameForm.Show(this);
            }
            else if (!ClientGameForm.IsHandleCreated)
            {
                ClientGameForm = new ClientGameForm();
                HostGameMenuItem.Enabled = false;
                ClientGameForm.Show(this);
            }
        }

        private void HostGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (HostGameForm == null)
            {
                HostGameForm = new HostGameForm();
                JoinGameMenuItem.Enabled = false;
                HostGameForm.Show(this);
            }
            else if (!HostGameForm.IsHandleCreated)
            {
                HostGameForm = new HostGameForm();
                JoinGameMenuItem.Enabled = false;
                HostGameForm.Show(this);
            }
        }

        private void InfoMenuItem_Click(object sender, EventArgs e)
        {
            if (InfoForm == null)
            {
                InfoForm = new InfoForm();
                InfoForm.ShowDialog();
            }
            else if (!InfoForm.IsHandleCreated)
            {
                InfoForm = new InfoForm();
                InfoForm.ShowDialog();
            }
        }
        #endregion 

        private void ButtonOne_Click(object sender, EventArgs e)
        {
            LabelStatus.Text += "Test\n";
            PanelStatus.VerticalScroll.Value += PanelStatus.VerticalScroll.SmallChange;
            PanelStatus.Refresh();
        }

        private void PanelStatusScroll(object sender, ScrollEventArgs e)
        {
            MessageBox.Show("TEst");
        }

        private void PanelStatusSizeChanged(object sender, EventArgs e)
        {
            MessageBox.Show("TEst");
        }

        private void BattleshipsForm_Load(object sender, EventArgs e)
        {
        }
    }
}