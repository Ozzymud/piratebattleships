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
#region directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#endregion

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

        #region fields
        /// <summary>
        /// Client Form.
        /// </summary> 
        private static ClientGameForm clientGameForm;

        private static HostGameForm hostGameForm;

        private static int counterCruiser = 0;

        private static int networkFormOpen = 0;

        private static int counterBoat = 0;

        private static int counterBattleship = 0;

        private static int counterGalley = 0;

        // Saves the state of whos turn it is
        private static TurnIdentifier whosTurn;

        /// <summary>
        /// Flag indicating whether the enemy is ready.
        /// </summary>
        private static bool opponentReadyToPlay;

        /// <summary>
        /// Flag indicating whether the player is ready.
        /// </summary>
        private static bool playerReadyToPlay;

        /// <summary>
        /// The battlefield of the enemy.
        /// </summary>
        private static BattlefieldOpponent battlefieldOpponent;

        private static DoubleBufferedPanel panelStatus;

        /// <summary>
        /// The battlefield of the player.
        /// </summary>
        private static BattlefieldPlayer battlefieldPlayer;

        private static Label labelStatus;

        /// <summary>
        /// Sound playback object.
        /// </summary>
        private static SoundClass soundPlayer;

        /// <summary>
        /// The label displays the status of the game.
        /// </summary>
        private static InfoForm infoForm;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the BattleshipsForm class.
        /// The game's interface constructor.
        /// </summary>
        public BattleshipsForm()
        {
            this.InitializeComponent();

            // Player is not yet ready (start of the game)
            PlayerReadyToPlay = false;
            OpponentReadyToPlay = false;
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
            LabelStatus.Font = new System.Drawing.Font("Book Antiqua", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)0);
            PanelStatus.Controls.Add(LabelStatus);
            this.Controls.Add(PanelStatus);
            SoundPlayer = new SoundClass();
            BattlefieldPlayer = new BattlefieldPlayer(BattlefieldPlayerX, BattlefieldPlayerY);
            BattlefieldOpponent = new BattlefieldOpponent(BattlefieldOpponentX, BattlefieldOpponentY);

            // Add the battlefield forms.
            this.Controls.Add(BattlefieldPlayer);
            this.Controls.Add(BattlefieldOpponent);

            SplashScreen splash = new SplashScreen();
            splash.ShowForm(); // Show splash screen (slow fade-in).
            Thread.Sleep(1000); // Show Splash Screen for 1 sec.
            splash.Close(); // Splash screen close.
            splash.Dispose(); // Release resources.
        }
        #endregion

        #region enum
        /// <summary>
        /// List of players.
        /// </summary>
        public enum TurnIdentifier
        {
            /// <summary>
            /// The player.
            /// </summary>
            player = 0,

            /// <summary>
            /// The enemy.
            /// </summary>
            enemy = 1
        }
        #endregion

        #region properties
        public static ClientGameForm ClientGameForm
        {
            get { return clientGameForm; }
            set { clientGameForm = value; }
        }

        public static HostGameForm HostGameForm
        {
            get { return hostGameForm; }
            set { hostGameForm = value; }
        }

        public static SoundClass SoundPlayer
        {
            get { return soundPlayer; }
            set { soundPlayer = value; }
        }

        public static Label LabelStatus
        {
            get { return labelStatus; }
            set { labelStatus = value; }
        }

        public static DoubleBufferedPanel PanelStatus
        {
            get { return panelStatus; }
            set { panelStatus = value; }
        }

        public static BattlefieldPlayer BattlefieldPlayer
        {
            get { return battlefieldPlayer; }
            set { battlefieldPlayer = value; }
        }

        public static BattlefieldOpponent BattlefieldOpponent
        {
            get { return battlefieldOpponent; }
            set { battlefieldOpponent = value; }
        }

        public static bool PlayerReadyToPlay
        {
            get { return playerReadyToPlay; }
            set { playerReadyToPlay = value; }
        }

        public static bool OpponentReadyToPlay
        {
            get { return opponentReadyToPlay; }
            set { opponentReadyToPlay = value; }
        }

        public static TurnIdentifier WhosTurn
        {
            get { return whosTurn; }
            set { whosTurn = value; }
        }

        public static int CounterGalley
        {
            get { return counterGalley; }
            set { counterGalley = value; }
        }

        public static int CounterBattleship
        {
            get { return counterBattleship; }
            set { counterBattleship = value; }
        }

        public static int CounterCruiser
        {
            get { return counterCruiser; }
            set { counterCruiser = value; }
        }

        public static int CounterBoat
        {
            get { return counterBoat; }
            set { counterBoat = value; }
        }

        /// <summary>
        /// Gets or sets networkFormOpen value.
        /// 0 for neither, 1 for join window, 2 for host window.
        /// </summary>
        public static int NetworkFormOpen
        {
            get { return networkFormOpen; }
            set { networkFormOpen = value; }
        }
        #endregion

        #region Form-Events
        private void BattleshipsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Do you want to quit the game?", "Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
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
                // Left click
                case System.Windows.Forms.MouseButtons.Left:
                    this.btnGalley.BackgroundImage = Battleships.Properties.Resources.btn_galley_click;

                    // Schiff darf nur ausgewählt werden, wenn nicht gerade ein schiff gesetzt wird
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.NoShip))
                    {
                        BattlefieldPlayer.CounterGalley = CounterGalley;
                        CounterGalley++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.Galley;

                        if (CounterGalley == MaxNumberGalley)
                        {
                            this.btnGalley.Enabled = false;
                        }
                        
                        this.lblGalley.Text = "Number: " + (MaxNumberGalley - CounterGalley).ToString();
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
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.NoShip))
                    {
                        BattlefieldPlayer.CounterBattleship = CounterBattleship;
                        CounterBattleship++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.Battleship;

                        if (CounterBattleship == MaxNumberBattleship)
                        {
                            this.btnBattleship.Enabled = false;
                        }
                        
                        this.lblBattleship.Text = "Number: " + (MaxNumberBattleship - CounterBattleship).ToString();
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
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.NoShip))
                    {
                        BattlefieldPlayer.CounterCruiser = CounterCruiser;
                        CounterCruiser++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.Cruiser;

                        if (CounterCruiser == MaxNumberCruiser)
                        {
                            this.btnCruiser.Enabled = false;
                        }
                        
                        this.lblCruiser.Text = "Number: " + (MaxNumberCruiser - CounterCruiser).ToString();
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

                    // Ship may be selected only when a ship is not already selected
                    if (!(BattlefieldPlayer.Ships != BattlefieldPlayer.ShipModels.NoShip))
                    {
                        BattlefieldPlayer.CounterBoat = CounterBoat;
                        CounterBoat++;
                        BattlefieldPlayer.Ships = BattlefieldPlayer.ShipModels.Boat;

                        if (CounterBoat == MaxNumberBoat)
                        {
                            this.btnBoat.Enabled = false;
                        }

                        this.lblBoat.Text = "Number: " + (MaxNumberBoat - CounterBoat).ToString();
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
        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Application.Exit();        
        }

        private void NetworkToolStripMenuItemClick(object sender, System.EventArgs e)
        {
            if (BattleshipsForm.networkFormOpen == 0)
            {
                this.joinToolStripMenuItem.Enabled = true;
                this.hostToolStripMenuItem.Enabled = true;
            }
            else if (BattleshipsForm.networkFormOpen == 1)
            {
                ClientGameForm.WindowState = FormWindowState.Normal;
                ClientGameForm.Activate();
            }
            else if (BattleshipsForm.networkFormOpen == 2)
            {
                HostGameForm.WindowState = FormWindowState.Normal;
                HostGameForm.Activate();
            }
        }

        private void JoinToolStripMenuItemClick(object sender, EventArgs e)
        {
                BattleshipsForm.NetworkFormOpen = 1;
                this.hostToolStripMenuItem.Enabled = false;
                this.joinToolStripMenuItem.Enabled = false;
            if (ClientGameForm == null)
            { 
                ClientGameForm = new ClientGameForm();
                ClientGameForm.Show(this);
            }
            else if (!ClientGameForm.IsHandleCreated)
            {
                ClientGameForm = new ClientGameForm();
                ClientGameForm.Show(this);
            }
       }

        private void HostToolStripMenuItemClick(object sender, EventArgs e)
        {
            BattleshipsForm.NetworkFormOpen = 2;
            this.hostToolStripMenuItem.Enabled = false;
            this.joinToolStripMenuItem.Enabled = false;
            if (HostGameForm == null)
            {
                HostGameForm = new HostGameForm();
                HostGameForm.Show(this);
            }
            else if (!HostGameForm.IsHandleCreated)
            {
                HostGameForm = new HostGameForm();
                HostGameForm.Show(this);
            }
        }
        
        private void AboutToolStripMenuItemClick(object sender, System.EventArgs e)
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
    }
}