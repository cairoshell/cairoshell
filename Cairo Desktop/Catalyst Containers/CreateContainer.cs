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
    public partial class CreateContainer : Form
    {
        public CreateContainer()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            RandomGenerator generator = new RandomGenerator();
            ContainerInstancing.Container newC = new ContainerInstancing.Container
            {
                ID = generator.RandomString(10, false),
                DisplayName = textBox2.Text,
                CoreModID = PluginLoader.FriendlyNames[comboBox1.Text],
                fsExp = true,
                theCatExp = true
            };
            ContainerInstancing.saveCont(newC);
        }

        private void CreateContainer_Load(object sender, EventArgs e)
        {
            foreach (string x in PluginLoader.FriendlyNames.Keys)
            {
                comboBox1.Items.Add(x);
            }
        }
    }
}
