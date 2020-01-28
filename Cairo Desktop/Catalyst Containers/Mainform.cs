using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Catalyst_Containers
{
    public partial class MainForm : Form
    {
        ContainerInstancing.Container[] containers;
        PluginLoader loader = new PluginLoader();
        public MainForm()
        {
            InitializeComponent();
        }
        private void SetUpContainers()
        {
            containers = ContainerInstancing.getListOfContainers();
            listBox1.Items.Clear();
            if (containers.Length > 0) {
                listBox1.Enabled = true;
                for (int i = 0; i < containers.Length - 1; i++)
                {
                    listBox1.Items.Add(containers[i].DisplayName);
                }
            } else
            {
                listBox1.Enabled = false;
                label1.Text = "No Containers Exist";
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //Create Directories For First Start.
                Directory.CreateDirectory(Constants.ApplicationFolder());
                Directory.CreateDirectory(Constants.CMFolderName());
                Directory.CreateDirectory(Constants.ContainerPath());
                loader.LoadPlugins();
                SetUpContainers();
            } catch
            {
                MessageBox.Show("Please Restart.");
                this.Close();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = containers[listBox1.SelectedIndex].DisplayName;
        }
    }
}
