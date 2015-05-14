//-----------------------------------------------------------------------
// <copyright file="HostGameForm.designer.cs" company="Team 17">
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
    partial class HostGameForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.btnCloseGame = new System.Windows.Forms.Button();
            this.btnHostGame = new System.Windows.Forms.Button();
            this.lblStatusMsg = new System.Windows.Forms.Label();
            this.listboxMessage = new System.Windows.Forms.ListBox();
            this.btnRdy = new System.Windows.Forms.Button();
            this.btnExtIp = new System.Windows.Forms.Button();
            this.toolTip_Btns = new System.Windows.Forms.ToolTip(this.components);
            this.btnInternIP = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(85, 12);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.ReadOnly = true;
            this.textBoxIP.Size = new System.Drawing.Size(120, 20);
            this.textBoxIP.TabIndex = 16;
            // 
            // lblIP
            // 
            this.lblIP.Location = new System.Drawing.Point(13, 12);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(56, 16);
            this.lblIP.TabIndex = 15;
            this.lblIP.Text = "Server IP";
            // 
            // lblPort
            // 
            this.lblPort.Location = new System.Drawing.Point(13, 36);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(48, 16);
            this.lblPort.TabIndex = 14;
            this.lblPort.Text = "Port";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(85, 36);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(40, 20);
            this.textBoxPort.TabIndex = 13;
            this.textBoxPort.Text = "8000";
            // 
            // btnCloseGame
            // 
            this.btnCloseGame.BackColor = System.Drawing.Color.Red;
            this.btnCloseGame.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCloseGame.ForeColor = System.Drawing.Color.Yellow;
            this.btnCloseGame.Location = new System.Drawing.Point(308, 58);
            this.btnCloseGame.Name = "btnCloseGame";
            this.btnCloseGame.Size = new System.Drawing.Size(88, 40);
            this.btnCloseGame.TabIndex = 18;
            this.btnCloseGame.Text = "Spiel beenden";
            this.btnCloseGame.UseVisualStyleBackColor = false;
            this.btnCloseGame.Click += new System.EventHandler(this.btnCloseGame_Click);
            // 
            // btnHostGame
            // 
            this.btnHostGame.BackColor = System.Drawing.Color.Blue;
            this.btnHostGame.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHostGame.ForeColor = System.Drawing.Color.Yellow;
            this.btnHostGame.Location = new System.Drawing.Point(308, 12);
            this.btnHostGame.Name = "btnHostGame";
            this.btnHostGame.Size = new System.Drawing.Size(88, 40);
            this.btnHostGame.TabIndex = 17;
            this.btnHostGame.Text = "Spiel starten";
            this.toolTip_Btns.SetToolTip(this.btnHostGame, "Starte ein Spiel, auf das sich ein Freund einlogen kann");
            this.btnHostGame.UseVisualStyleBackColor = false;
            this.btnHostGame.Click += new System.EventHandler(this.btnHostGame_Click);
            // 
            // lblStatusMsg
            // 
            this.lblStatusMsg.Location = new System.Drawing.Point(13, 89);
            this.lblStatusMsg.Name = "lblStatusMsg";
            this.lblStatusMsg.Size = new System.Drawing.Size(112, 16);
            this.lblStatusMsg.TabIndex = 20;
            this.lblStatusMsg.Text = "Status Message:";
            // 
            // listboxMessage
            // 
            this.listboxMessage.FormattingEnabled = true;
            this.listboxMessage.HorizontalScrollbar = true;
            this.listboxMessage.Location = new System.Drawing.Point(16, 108);
            this.listboxMessage.Name = "listboxMessage";
            this.listboxMessage.Size = new System.Drawing.Size(283, 121);
            this.listboxMessage.TabIndex = 21;
            // 
            // btnRdy
            // 
            this.btnRdy.BackColor = System.Drawing.Color.Blue;
            this.btnRdy.Enabled = false;
            this.btnRdy.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRdy.ForeColor = System.Drawing.Color.Yellow;
            this.btnRdy.Location = new System.Drawing.Point(308, 108);
            this.btnRdy.Name = "btnRdy";
            this.btnRdy.Size = new System.Drawing.Size(88, 40);
            this.btnRdy.TabIndex = 22;
            this.btnRdy.Text = "Bereit";
            this.toolTip_Btns.SetToolTip(this.btnRdy, "Gib an, dass du bereit zum Spielen bist");
            this.btnRdy.UseVisualStyleBackColor = false;
            this.btnRdy.Click += new System.EventHandler(this.BtnRdy_Click);
            // 
            // btnExtIp
            // 
            this.btnExtIp.Location = new System.Drawing.Point(224, 12);
            this.btnExtIp.Name = "btnExtIp";
            this.btnExtIp.Size = new System.Drawing.Size(75, 40);
            this.btnExtIp.TabIndex = 23;
            this.btnExtIp.Text = "Externe IP \r\nfinden";
            this.toolTip_Btns.SetToolTip(this.btnExtIp, "Ermittelt deine externe IP-Adresse um mit einem Freund über das Intenet spielen z" +
                    "u können");
            this.btnExtIp.UseVisualStyleBackColor = true;
            this.btnExtIp.Click += new System.EventHandler(this.btnExtIp_Click);
            // 
            // toolTip_Btns
            // 
            this.toolTip_Btns.IsBalloon = true;
            // 
            // btnInternIP
            // 
            this.btnInternIP.Location = new System.Drawing.Point(224, 58);
            this.btnInternIP.Name = "btnInternIP";
            this.btnInternIP.Size = new System.Drawing.Size(75, 40);
            this.btnInternIP.TabIndex = 24;
            this.btnInternIP.Text = "Interne IP \r\nfinden";
            this.toolTip_Btns.SetToolTip(this.btnInternIP, "Ermittelt deine interne IP-Adresse um mit einem Freund über LAN spielen zu können" +
                    "");
            this.btnInternIP.UseVisualStyleBackColor = true;
            this.btnInternIP.Click += new System.EventHandler(this.btnInternIP_Click);
            // 
            // HostGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 237);
            this.Controls.Add(this.btnInternIP);
            this.Controls.Add(this.btnExtIp);
            this.Controls.Add(this.btnRdy);
            this.Controls.Add(this.listboxMessage);
            this.Controls.Add(this.lblStatusMsg);
            this.Controls.Add(this.btnCloseGame);
            this.Controls.Add(this.btnHostGame);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.textBoxPort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "HostGameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HostGameForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HostGameForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Button btnCloseGame;
        private System.Windows.Forms.Button btnHostGame;
        private System.Windows.Forms.Label lblStatusMsg;
        private System.Windows.Forms.ListBox listboxMessage;
        private System.Windows.Forms.Button btnRdy;
        private System.Windows.Forms.Button btnExtIp;
        private System.Windows.Forms.ToolTip toolTip_Btns;
        private System.Windows.Forms.Button btnInternIP;
    }
}