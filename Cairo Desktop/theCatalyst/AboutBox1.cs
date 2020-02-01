using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NeoGeniX.Forms;

namespace theCatalyst
{
    partial class AboutBox1 : DarkForm
    {
        public AboutBox1()
        {
            InitializeComponent();
        }

        private void AboutBox1_Load(object sender, EventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
