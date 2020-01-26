using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CairoDesktop.Common;

namespace FrontPage
{
    public partial class CancelSession : Form
    {
        public CancelSession()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SystemPower.ShowLogOffConfirmation();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SystemPower.ShowShutdownConfirmation();
        }
    }
}
