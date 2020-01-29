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
                for (int i = 0; i < containers.Length; i++)
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
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                this.Close();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            ContainerInstancing.Container cont = containers[listBox1.SelectedIndex];
            Player p = new Player();
            CatalystContainer.CoreModContracts.Exposure exp = new CatalystContainer.CoreModContracts.Exposure();
            if (cont.theCatExp)
            {
                theCatalystLauncher theCat = new theCatalystLauncher();
                theCat.MdiParent = p;
                theCat.mainform = p;
                theCat.Show();
            }
            p.Text = cont.DisplayName;
            PluginLoader.Plugins[cont.CoreModID].Boot(cont.ID, p, exp);
            p.Show();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CreateContainer cc = new CreateContainer();
            if (cc.ShowDialog() == DialogResult.OK)
            {
                SetUpContainers();
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                label1.Text = containers[listBox1.SelectedIndex].DisplayName;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
            } catch
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            ContainerInstancing.Container cont = containers[listBox1.SelectedIndex];
            ContainerSettings cs = new ContainerSettings(cont);
            cs.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ContainerInstancing.Container cont = containers[listBox1.SelectedIndex];
            ContainerInstancing.deleteCont(cont);
            SetUpContainers();
        }
    }
}
