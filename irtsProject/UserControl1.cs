using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace irtsProject
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public int UserControl1_MouseClick(object sender, EventArgs e)
        {
            return 1;
        }

        private void UserControl1_SizeChanged(object sender, EventArgs e)
        {
            //label1.Font = new Font(label1.Font.FontFamily, this.Width / 17);
            //pictureBox1.Size = new Size(this.Width / 5, this.Height / 2);
        }

        //private void UserControl1_MouseClick(object sender, MouseEventArgs e)
        //{

        //}
    }
}
