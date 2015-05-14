//-----------------------------------------------------------------------
// <copyright file="ClientGameForm.designer.cs" company="Team 17">
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
    public partial class ClientGameForm
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
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.lblIP = new System.Windows.Forms.Label();
            this.textboxIP = new System.Windows.Forms.TextBox();
            this.textboxPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.listboxRx = new System.Windows.Forms.ListBox();
            this.btnRdy = new System.Windows.Forms.Button();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(13, 20);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(54, 13);
            this.lblIP.TabIndex = 0;
            this.lblIP.Text = "Server IP:";
            // 
            // textboxIP
            // 
            this.textboxIP.Location = new System.Drawing.Point(73, 13);
            this.textboxIP.Name = "textboxIP";
            this.textboxIP.Size = new System.Drawing.Size(124, 20);
            this.textboxIP.TabIndex = 1;
            // 
            // textboxPort
            // 
            this.textboxPort.Location = new System.Drawing.Point(73, 39);
            this.textboxPort.Name = "textboxPort";
            this.textboxPort.Size = new System.Drawing.Size(61, 20);
            this.textboxPort.TabIndex = 3;
            this.textboxPort.Text = "8000";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(13, 46);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port:";
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.Blue;
            this.btnConnect.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnConnect.ForeColor = System.Drawing.Color.Yellow;
            this.btnConnect.Location = new System.Drawing.Point(224, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(90, 46);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.toolTipButtons.SetToolTip(this.btnConnect, "Verbinde dich mit einem Spiel (Über das Internet oder LAN)");
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Message from Server:";
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.BackColor = System.Drawing.Color.Red;
            this.btnDisconnect.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnDisconnect.ForeColor = System.Drawing.Color.Yellow;
            this.btnDisconnect.Location = new System.Drawing.Point(224, 65);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(90, 46);
            this.btnDisconnect.TabIndex = 7;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = false;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // listboxRx
            // 
            this.listboxRx.FormattingEnabled = true;
            this.listboxRx.HorizontalScrollbar = true;
            this.listboxRx.Location = new System.Drawing.Point(16, 90);
            this.listboxRx.Name = "listboxRx";
            this.listboxRx.Size = new System.Drawing.Size(202, 121);
            this.listboxRx.TabIndex = 8;
            // 
            // btnRdy
            // 
            this.btnRdy.BackColor = System.Drawing.Color.Blue;
            this.btnRdy.Enabled = false;
            this.btnRdy.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnRdy.ForeColor = System.Drawing.Color.Yellow;
            this.btnRdy.Location = new System.Drawing.Point(224, 117);
            this.btnRdy.Name = "btnRdy";
            this.btnRdy.Size = new System.Drawing.Size(90, 46);
            this.btnRdy.TabIndex = 9;
            this.btnRdy.Text = "Ready";
            this.toolTipButtons.SetToolTip(this.btnRdy, "Gib an, dass du Bereit zum Spielen bist");
            this.btnRdy.UseVisualStyleBackColor = false;
            this.btnRdy.Click += new System.EventHandler(this.btnRdy_Click);
            // 
            // toolTipButtons
            // 
            this.toolTipButtons.IsBalloon = true;
            // 
            // ClientGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 218);
            this.Controls.Add(this.btnRdy);
            this.Controls.Add(this.listboxRx);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.textboxPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.textboxIP);
            this.Controls.Add(this.lblIP);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ClientGameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ConnectGameForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientGameForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.TextBox textboxIP;
        private System.Windows.Forms.TextBox textboxPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.ListBox listboxRx;
        private System.Windows.Forms.Button btnRdy;
        private System.Windows.Forms.ToolTip toolTipButtons;
    }
}