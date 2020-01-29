using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Catalyst_Containers
{
    public partial class theCatalystLauncher : Form
    {
        public Form mainform;
        public theCatalystLauncher()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openEditor(mainform);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openTime(mainform);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openCalc(mainform);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openAbout(mainform);
        }
    }
}
