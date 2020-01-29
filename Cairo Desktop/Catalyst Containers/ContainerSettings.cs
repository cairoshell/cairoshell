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
    public partial class ContainerSettings : Form
    {
        ContainerInstancing.Container container;
        public ContainerSettings(ContainerInstancing.Container cont)
        {
            container = cont;
            InitializeComponent();
        }

        private void ContainerSettings_Load(object sender, EventArgs e)
        {
            foreach (string x in PluginLoader.FriendlyNames.Keys)
            {
                CoreBox.Items.Add(x);
            }
            IDBox.Text = container.ID;
            DisplayNameBox.Text = container.DisplayName;
            var cm = PluginLoader.Plugins[container.CoreModID];
            try
            {
                CoreBox.SelectedItem = "(" + cm.UniqueID + ") - " + cm.DisplayName;
            } catch
            {
                CoreBox.Text = "Invalid";
            }
            catBox.Checked = container.theCatExp;
            HFSBox.Checked = container.fsExp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            container.DisplayName = DisplayNameBox.Text;
            container.CoreModID = PluginLoader.FriendlyNames[CoreBox.Text];
            container.theCatExp = catBox.Checked;
            container.fsExp = HFSBox.Checked;
            ContainerInstancing.saveCont(container);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CoreBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
