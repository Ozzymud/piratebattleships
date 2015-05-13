/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: BattlefieldOpponent
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
using System.Runtime.InteropServices;

namespace Battleships
{
    public class BattlefieldOpponent : Panel_DoubleBuffered
    {
        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        public Panel_DoubleBuffered[,] pb = new Panel_DoubleBuffered[10, 10];

        private delegate void addControlCallback(Control contr);
        delegate void showDestroyedShipsCallback(int[] args, bool horizontal);

        //private System.IO.MemoryStream ms = new System.IO.MemoryStream(Battleships.Properties.Resources.MyCursor);

        /// <summary>
        /// Positionsfarbe (Wenn mit Maus über Feld gefahren wird)
        /// </summary>
        private Color positionColor;

        public BattlefieldOpponent(int x, int y)
        {
            positionColor = new Color();
            // Ein helles Blau
            positionColor = Color.FromArgb(120, 30, 151, 255);

            // Cursor aus PNG-File laden
            // Edit: use embedded resource
            // Bitmap bitmap = new Bitmap(System.IO.Directory.GetCurrentDirectory() + "\\aim.png");
            Bitmap bitmap = new Bitmap(Battleships.Properties.Resources.aim);

            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            //this.BackgroundImageLayout = ImageLayout.Stretch;
            //this.BackgroundImage = Battleships.Properties.Resources.meer_opponent;
            this.BackColor = Color.Transparent;
            //this.BorderStyle = BorderStyle.FixedSingle;
            //this.BorderStyle = BorderStyle.None;

            //Matrix Gegner
            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; j < pb.GetLength(1); j++)
                {
                    Panel_DoubleBuffered p = new Panel_DoubleBuffered();
                    p.Location = new Point(i * 30, j * 30);
                    p.Tag = 0;
                    p.Margin = new Padding(0);
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.MouseClick += new MouseEventHandler(p_Clicked);
                    p.MouseEnter += new EventHandler(p_MouseEnter);
                    p.MouseLeave += new EventHandler(p_MouseLeave);
                    p.Cursor = CreateCursor(bitmap, 3, 3);
                    p.BackColor = Color.Transparent;
                    p.BorderStyle = BorderStyle.None;
                    pb[i, j] = p;
                    this.Controls.Add(p);
                }
            }
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
                BattleshipsForm.soundPlayer.playSoundAsync("explosion1.wav");
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
                    pb[args[0], args[1]].BackgroundImage = Properties.Resources.boat_dmg_v2;
                    pb[args[2], args[3]].BackgroundImage = Properties.Resources.boat_dmg_v1;
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
                BattleshipsForm.soundPlayer.playSoundAsync("explosion1.wav");
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
        /// Setzt einen Treffer auf das angegebene Feld
        /// </summary>
        /// <param name="x">X-Koordinate des Treffers</param>
        /// <param name="y">Y-Koordinate des Treffers</param>
        public void setImpact(int x, int y)
        {
            try
            {
                BattleshipsForm.soundPlayer.playSoundAsync("explosion2.wav");
                drawExplosion(x, y);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Setzt einen Fehlschuss auf das angegebene Feld
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void setMiss(int x, int y)
        {
            BattleshipsForm.soundPlayer.playSoundAsync("splash.wav");
            drawMiss(x, y);
        }

        private void drawMiss(int x, int y)
        {
            PictureBox missPicture = new PictureBox();
            missPicture.Name = "miss_" + x.ToString() + ":" + y.ToString();
            missPicture.Location = new Point(x * 30, y * 30);
            missPicture.Size = new Size(30, 30);
            missPicture.Margin = new Padding(0);
            missPicture.Padding = new Padding(0);
            missPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            missPicture.BackColor = Color.Transparent;
            missPicture.Image = Properties.Resources.splash;

            addControl(missPicture);
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
            explPicture.Location = new Point(x * 30, y * 30);
            explPicture.Size = new Size(30, 30);
            explPicture.Margin = new Padding(0);
            explPicture.Padding = new Padding(0);
            explPicture.BorderStyle = System.Windows.Forms.BorderStyle.None;
            explPicture.BackColor = Color.Transparent;
            explPicture.Image = Properties.Resources.explo6;
            
            // PictureBox-Explosion dem Panel hinzufügen in welchem der Einschlag ist
            addControl(explPicture);
        }
        
        public void addControl(Control contr)
        {
            if (this.InvokeRequired)
            {
                addControlCallback d = new addControlCallback(addControl);
                this.Invoke(d, new object[] { contr });
            }
            else
            {
                this.Controls.Add(contr);
                Control[] s = this.Controls.Find(contr.Name, false);
                s[0].BringToFront();
            }
        }

        #region Mouse-Events
        public void p_Clicked(object sender, MouseEventArgs e)
        {
            // Erst loslegen, wenn beide Spieler bereit sind
            if (BattleshipsForm.opponendReady2Play && BattleshipsForm.playerReady2Play)
            {
                // Es darf nur geschossen werden, wenn man auch an der reihe ist!
                if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
                {
                    Panel_DoubleBuffered tmp = (Panel_DoubleBuffered)sender;
                    switch (e.Button)
                    {
                        case System.Windows.Forms.MouseButtons.Left:
                            // Bin ich einem Spiel beigetreten?
                            if (BattleshipsForm.clientGameForm != null)
                            {
                                if (BattleshipsForm.clientGameForm.m_clientSocket != null)
                                {
                                    try
                                    {
                                        object sendData = tmp.Name;
                                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());
                                        // Koordinaten des aktuellen Feldes an Server schicken
                                        BattleshipsForm.clientGameForm.m_clientSocket.Send(byData);
                                        // Auf Antwort warten (HIT\MISS\WIN\LOSE)
                                        BattleshipsForm.clientGameForm.WaitForData();
                                    }
                                    catch (Exception ex)
                                    {
                                        BattleshipsForm.clientGameForm.SetText(ex.ToString());
                                    }
                                }
                            }
                            // Oder Hoste ich selber ein Spiel?
                            else if (BattleshipsForm.hostGameForm != null)
                            {
                                try
                                {
                                    if (BattleshipsForm.hostGameForm.m_workerSocket != null)
                                    {
                                        try
                                        {
                                            object sendData = tmp.Name;
                                            byte[] byData = System.Text.Encoding.ASCII.GetBytes(sendData.ToString());
                                            // Koordinaten des aktuellen Feldes an Server schicken
                                            BattleshipsForm.hostGameForm.m_workerSocket.Send(byData);
                                            // Auf Antwort warten (HIT\MISS\WIN\LOSE)
                                            BattleshipsForm.hostGameForm.WaitForData(BattleshipsForm.hostGameForm.m_workerSocket);
                                        }
                                        catch (Exception ex)
                                        {
                                            BattleshipsForm.hostGameForm.SetText(ex.ToString());
                                            BattleshipsForm.hostGameForm.m_clientCount--;
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                }
                            }
                            break;
                    }
                }
            }
        }

        public void p_MouseEnter(object sender, EventArgs e)
        {
            if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
            {
                Panel_DoubleBuffered tmp = (Panel_DoubleBuffered)sender;

                for (int i = 0; i < pb.GetLength(0); i++)
                {
                    for (int j = 0; j < pb.GetLength(1); j++)
                    {
                        if (tmp.Name.ToString() == pb[i, j].Name.ToString())
                        {
                            pb[i, j].BackColor = positionColor;
                        }
                    }
                }
            }
        }

        public void p_MouseLeave(object sender, EventArgs e)
        {
            if (BattleshipsForm.whosTurn == BattleshipsForm.spielzug.player)
            {
                Panel_DoubleBuffered tmp = (Panel_DoubleBuffered)sender;

                for (int i = 0; i < pb.GetLength(0); i++)
                {
                    for (int j = 0; j < pb.GetLength(1); j++)
                    {
                        if ((int)pb[i, j].Tag != (int)1)
                        {
                            pb[i, j].BackColor = Color.Transparent;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Erstellt einen Cursor aus einer Bitmap (PNG, JPEG geht auch)
        /// </summary>
        /// <param name="bmp">Das Image-File welches als Cursor angezeigt werden soll</param>
        /// <param name="xHotSpot"></param>
        /// <param name="yHotSpot"></param>
        /// <returns>Den erstellten Cursor</returns>
        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            return new Cursor(CreateIconIndirect(ref tmp));
        }
    }
}

