using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalystContainer.CoreModContracts;
using ProshellNT2.TargetingPack;

namespace ProshellNT2
{
    public partial class MainForm : Form
    {
        public Exposure exp;
        Dictionary<ListViewItem, I_NT2Addon> _Plugins;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _Plugins = new Dictionary<ListViewItem, I_NT2Addon>();
            //ICollection<IPlugin> plugins = PluginLoader.LoadPlugins("Plugins"); 
            ICollection<I_NT2Addon> plugins = PluginLoader.LoadPlugins(exp.containerfs + @"\software");
            if (plugins != null)
            {
                foreach (var item in plugins)
                {
                    imageList1.Images.Add(item.Icon);
                    ListViewItem listViewItem = new ListViewItem(item.Name, imageList1.Images.Count - 1);
                    _Plugins.Add(listView1.Items.Add(listViewItem), item);
                    
                    
                }
            } else
            {
                MessageBox.Show("No Plugins Installed");
            }
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MdiParent.Close();
        }

        private void aboutProshellNTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.MdiParent = MdiParent;
            ab.Show();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            _Plugins[listView1.SelectedItems[0]].OnRun(MdiParent);
        }
    }
}
