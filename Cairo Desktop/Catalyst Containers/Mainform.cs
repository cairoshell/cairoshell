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
    public partial class MainForm : Form
    {
        PluginLoader loader = new PluginLoader();
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                loader.LoadPlugins();
            } catch
            {
                MessageBox.Show("Failed to Load Any Core Modules");
                this.Close();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Player player = new Player();
            player.Text = "Test";
            player.Show();
            PluginLoader.Plugins["NT2.2020"].Boot("nil", player);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("List of IDS");
            foreach (string x in PluginLoader.Plugins.Keys)
            {
                listBox1.Items.Add(x);
            }
        }
    }
}
