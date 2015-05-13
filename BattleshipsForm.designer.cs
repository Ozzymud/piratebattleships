namespace Battleships
{
    partial class BattleshipsForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BattleshipsForm));
        	this.btnCruiser = new System.Windows.Forms.Button();
        	this.btnBattleship = new System.Windows.Forms.Button();
        	this.btnBoat = new System.Windows.Forms.Button();
        	this.lblGalley = new System.Windows.Forms.Label();
        	this.btnGalley = new System.Windows.Forms.Button();
        	this.lblBattleship = new System.Windows.Forms.Label();
        	this.lblCruiser = new System.Windows.Forms.Label();
        	this.lblBoat = new System.Windows.Forms.Label();
        	this.toolTip_Mouse = new System.Windows.Forms.ToolTip(this.components);
        	this.menuStripMain = new System.Windows.Forms.MenuStrip();
        	this.dateiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.beendenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.menuStripMain.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnCruiser
        	// 
        	this.btnCruiser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCruiser.BackColor = System.Drawing.Color.Transparent;
        	this.btnCruiser.BackgroundImage = global::Battleships.Properties.Resources.btn_cruiser;
        	this.btnCruiser.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.btnCruiser.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(166)))), ((int)(((byte)(114)))));
        	this.btnCruiser.FlatAppearance.BorderSize = 0;
        	this.btnCruiser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnCruiser.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnCruiser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnCruiser.Location = new System.Drawing.Point(340, 320);
        	this.btnCruiser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        	this.btnCruiser.Name = "btnCruiser";
        	this.btnCruiser.Size = new System.Drawing.Size(120, 37);
        	this.btnCruiser.TabIndex = 5;
        	this.btnCruiser.UseVisualStyleBackColor = false;
        	this.btnCruiser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnCruiser_MouseDown);
        	this.btnCruiser.MouseEnter += new System.EventHandler(this.btnCruiser_MouseEnter);
        	this.btnCruiser.MouseLeave += new System.EventHandler(this.btnCruiser_MouseLeave);
        	// 
        	// btnBattleship
        	// 
        	this.btnBattleship.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.btnBattleship.BackColor = System.Drawing.Color.Transparent;
        	this.btnBattleship.BackgroundImage = global::Battleships.Properties.Resources.btn_z;
        	this.btnBattleship.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.btnBattleship.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(166)))), ((int)(((byte)(114)))));
        	this.btnBattleship.FlatAppearance.BorderSize = 0;
        	this.btnBattleship.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnBattleship.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnBattleship.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnBattleship.Location = new System.Drawing.Point(348, 169);
        	this.btnBattleship.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        	this.btnBattleship.Name = "btnBattleship";
        	this.btnBattleship.Size = new System.Drawing.Size(100, 28);
        	this.btnBattleship.TabIndex = 6;
        	this.btnBattleship.UseVisualStyleBackColor = false;
        	this.btnBattleship.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnBattleship_MouseDown);
        	this.btnBattleship.MouseEnter += new System.EventHandler(this.btnBattleship_MouseEnter);
        	this.btnBattleship.MouseLeave += new System.EventHandler(this.btnBattleship_MouseLeave);
        	// 
        	// btnBoat
        	// 
        	this.btnBoat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.btnBoat.BackColor = System.Drawing.Color.Transparent;
        	this.btnBoat.BackgroundImage = global::Battleships.Properties.Resources.btn_boat;
        	this.btnBoat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.btnBoat.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(166)))), ((int)(((byte)(114)))));
        	this.btnBoat.FlatAppearance.BorderSize = 0;
        	this.btnBoat.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnBoat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnBoat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnBoat.Location = new System.Drawing.Point(360, 396);
        	this.btnBoat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        	this.btnBoat.Name = "btnBoat";
        	this.btnBoat.Size = new System.Drawing.Size(83, 33);
        	this.btnBoat.TabIndex = 7;
        	this.btnBoat.UseVisualStyleBackColor = false;
        	this.btnBoat.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnBoat_MouseDown);
        	this.btnBoat.MouseEnter += new System.EventHandler(this.btnBoat_MouseEnter);
        	this.btnBoat.MouseLeave += new System.EventHandler(this.btnBoat_MouseLeave);
        	// 
        	// lblGalley
        	// 
        	this.lblGalley.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.lblGalley.AutoSize = true;
        	this.lblGalley.BackColor = System.Drawing.Color.Transparent;
        	this.lblGalley.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblGalley.Location = new System.Drawing.Point(356, 219);
        	this.lblGalley.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblGalley.Name = "lblGalley";
        	this.lblGalley.Size = new System.Drawing.Size(88, 20);
        	this.lblGalley.TabIndex = 8;
        	this.lblGalley.Text = "Anzahl: 1";
        	// 
        	// btnGalley
        	// 
        	this.btnGalley.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.btnGalley.BackColor = System.Drawing.Color.Transparent;
        	this.btnGalley.BackgroundImage = global::Battleships.Properties.Resources.btn_galley;
        	this.btnGalley.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.btnGalley.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(162)))), ((int)(((byte)(117)))));
        	this.btnGalley.FlatAppearance.BorderSize = 0;
        	this.btnGalley.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnGalley.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnGalley.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnGalley.Location = new System.Drawing.Point(348, 245);
        	this.btnGalley.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        	this.btnGalley.Name = "btnGalley";
        	this.btnGalley.Size = new System.Drawing.Size(100, 31);
        	this.btnGalley.TabIndex = 4;
        	this.btnGalley.UseVisualStyleBackColor = false;
        	this.btnGalley.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnGalley_MouseDown);
        	this.btnGalley.MouseEnter += new System.EventHandler(this.btnGalley_MouseEnter);
        	this.btnGalley.MouseLeave += new System.EventHandler(this.btnGalley_MouseLeave);
        	// 
        	// lblBattleship
        	// 
        	this.lblBattleship.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.lblBattleship.AutoSize = true;
        	this.lblBattleship.BackColor = System.Drawing.Color.Transparent;
        	this.lblBattleship.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblBattleship.Location = new System.Drawing.Point(356, 144);
        	this.lblBattleship.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblBattleship.Name = "lblBattleship";
        	this.lblBattleship.Size = new System.Drawing.Size(88, 20);
        	this.lblBattleship.TabIndex = 9;
        	this.lblBattleship.Text = "Anzahl: 1";
        	// 
        	// lblCruiser
        	// 
        	this.lblCruiser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.lblCruiser.AutoSize = true;
        	this.lblCruiser.BackColor = System.Drawing.Color.Transparent;
        	this.lblCruiser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblCruiser.Location = new System.Drawing.Point(356, 299);
        	this.lblCruiser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblCruiser.Name = "lblCruiser";
        	this.lblCruiser.Size = new System.Drawing.Size(88, 20);
        	this.lblCruiser.TabIndex = 10;
        	this.lblCruiser.Text = "Anzahl: 3";
        	// 
        	// lblBoat
        	// 
        	this.lblBoat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.lblBoat.AutoSize = true;
        	this.lblBoat.BackColor = System.Drawing.Color.Transparent;
        	this.lblBoat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblBoat.Location = new System.Drawing.Point(356, 370);
        	this.lblBoat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblBoat.Name = "lblBoat";
        	this.lblBoat.Size = new System.Drawing.Size(88, 20);
        	this.lblBoat.TabIndex = 11;
        	this.lblBoat.Text = "Anzahl: 3";
        	// 
        	// menuStripMain
        	// 
        	this.menuStripMain.BackColor = System.Drawing.Color.Transparent;
        	this.menuStripMain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.menuStripMain.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.dateiToolStripMenuItem});
        	this.menuStripMain.Location = new System.Drawing.Point(0, 0);
        	this.menuStripMain.Name = "menuStripMain";
        	this.menuStripMain.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
        	this.menuStripMain.Size = new System.Drawing.Size(794, 28);
        	this.menuStripMain.TabIndex = 12;
        	// 
        	// dateiToolStripMenuItem
        	// 
        	this.dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.beendenToolStripMenuItem});
        	this.dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
        	this.dateiToolStripMenuItem.Size = new System.Drawing.Size(54, 24);
        	this.dateiToolStripMenuItem.Text = "S&piel";
        	// 
        	// beendenToolStripMenuItem
        	// 
        	this.beendenToolStripMenuItem.BackColor = System.Drawing.Color.Transparent;
        	this.beendenToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.beendenToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
        	this.beendenToolStripMenuItem.Name = "beendenToolStripMenuItem";
        	this.beendenToolStripMenuItem.Size = new System.Drawing.Size(139, 24);
        	this.beendenToolStripMenuItem.Text = "&Beenden";
        	this.beendenToolStripMenuItem.Click += new System.EventHandler(this.beendenToolStripMenuItem_Click);
        	// 
        	// BattleshipsForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.BackgroundImage = global::Battleships.Properties.Resources.Battleshipsv8;
        	this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.ClientSize = new System.Drawing.Size(794, 592);
        	this.Controls.Add(this.lblBoat);
        	this.Controls.Add(this.lblCruiser);
        	this.Controls.Add(this.lblBattleship);
        	this.Controls.Add(this.lblGalley);
        	this.Controls.Add(this.btnBoat);
        	this.Controls.Add(this.btnBattleship);
        	this.Controls.Add(this.btnCruiser);
        	this.Controls.Add(this.btnGalley);
        	this.Controls.Add(this.menuStripMain);
        	this.DoubleBuffered = true;
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        	this.MaximizeBox = false;
        	this.Name = "BattleshipsForm";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "Battleships - The Game";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BattleshipsForm_FormClosing);
        	this.Load += new System.EventHandler(this.BattleshipsForm_Load);
        	this.menuStripMain.ResumeLayout(false);
        	this.menuStripMain.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnGalley;
        private System.Windows.Forms.Button btnCruiser;
        private System.Windows.Forms.Button btnBattleship;
        private System.Windows.Forms.Button btnBoat;
        private System.Windows.Forms.Label lblGalley;
        private System.Windows.Forms.Label lblBattleship;
        private System.Windows.Forms.Label lblCruiser;
        private System.Windows.Forms.Label lblBoat;
        private System.Windows.Forms.ToolTip toolTip_Mouse;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beendenToolStripMenuItem;
        // unused?
        // private DoubleBufferedUserControls.Panel_DoubleBuffered panel_Status;
    }
}

