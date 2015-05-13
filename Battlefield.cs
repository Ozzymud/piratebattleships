using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Battleships
{
    class Battlefield : Panel
    {
        public PictureBox[,] pb = new PictureBox[10, 10];
        private bool destroyer;

        public Battlefield(int x, int y)
        {
            this.Location = new Point(x, y);
            this.Width = 300;
            this.Height = 300;
            this.Size = new Size(300, 300);
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.BackgroundImage = Properties.Resources.meer_big;
            this.BorderStyle = BorderStyle.FixedSingle;
            
            //Matrix Mensch
            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; j < pb.GetLength(1); j++)
                {
                    PictureBox p = new PictureBox();
                    p.Location = new Point(i*30, j*30);
                    p.Tag = 0;
                    p.Name = "pb_" + i.ToString() + ":" + j.ToString();
                    p.Size = new Size(30, 30);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.Click += new EventHandler(pb_Clicked);
                    p.MouseEnter += new EventHandler(pb_MouseEnter);
                    p.MouseLeave += new EventHandler(pb_MouseLeave);
                    //p.Visible = false;
                    p.BackColor = Color.Transparent;
                    pb[i, j] = p;
                    this.Controls.Add(p);
                    //this.Controls.Add(p);
                    //pb[i, j].BackColor = Color.Transparent;
                }
            } 
        }

        public void pb_MouseEnter(object sender, EventArgs e)
        {
            PictureBox tmp = (PictureBox)sender;
            int x = 0, y = 0;

            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; i < pb.GetLength(1); i++)
                {
                    if (tmp.Name == pb[i, j].Name)
                    {
                        x = i;
                        y = j;
                        j = pb.GetLength(1);
                        i = pb.GetLength(0);
                    }
                }
            }

            if (destroyer)
            {
                switch (tmp.Name.ToString())
                {
                    case "pb_0:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_1:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_2:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_3:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_4:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_5:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_6:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x + 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x + 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x + 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_7:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x - 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x - 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x - 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_8:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x - 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x - 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x - 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                    case "pb_9:0":
                        //Erstes Feld links oben
                        tmp.Image = Battleships.Properties.Resources.z1;
                        pb[x - 1, y].Image = Battleships.Properties.Resources.z2;
                        pb[x - 2, y].Image = Battleships.Properties.Resources.z3;
                        pb[x - 3, y].Image = Battleships.Properties.Resources.z4;
                        break;
                }
            }
        }

        public void pb_MouseLeave(object sender, EventArgs e)
        {
            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; i < pb.GetLength(1); i++)
                {
                    if ((int)pb[i,j].Tag != (int)1)
                    pb[i, j].Image = null;
                }
            }
        }

        public void pb_Clicked(object sender, EventArgs e)
        {
            PictureBox temp = (PictureBox)sender;

            for (int i = 0; i < pb.GetLength(0); i++)
            {
                for (int j = 0; i < pb.GetLength(1); i++)
                {
                    if (pb[i,j].Image != null)
                    {
                       pb[i, j].Tag = 1;
                    }
                }
            }

            destroyer = false;
            //btnZerstoerer.Enabled = true;
        }

        public bool destroyer_
        {
            get { return destroyer; }
            set { destroyer = value; }
        }
    }
}
