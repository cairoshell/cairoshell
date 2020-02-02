using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using theCatalyst;

namespace theCatalyst_Test_Program
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openEditor();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openCalc();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            theCatalyst.Init.openTime();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Catalyst_Containers.Init.Start();
        }
    }
}
