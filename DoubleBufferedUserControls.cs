using System.Drawing;
using System.Windows.Forms;

namespace Battleships
{
	public class FormDoubleBuffered : Form
		{
		public FormDoubleBuffered()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      this.UpdateStyles();
	}
  }
	public class PanelDoubleBuffered : Panel
    {
		public PanelDoubleBuffered()
    {
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      this.UpdateStyles();
    }
  }
	public class PictureBoxDoubleBuffered : PictureBox
	{
        public PictureBoxDoubleBuffered()
	{
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
      this.UpdateStyles();
    }
  }
	public class TransparentListBox : ListBox
    {
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams createParams = base.CreateParams;
        createParams.ExStyle |= 32;
        return createParams;
      }
    }

    public TransparentListBox()
    {
      this.SetStyle(ControlStyles.Opaque | ControlStyles.SupportsTransparentBackColor, true);
      this.BackColor = Color.Transparent;
    }
  }
}