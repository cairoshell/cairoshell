using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ProshellNT2.TargetingPack;
using System.IO;

namespace ProshellNT2.Accessories
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string currentdir;
        public Dictionary<string, IFileOpen> Fileopendict { get; set; }
        Dictionary<string, TreeNode> drivePairs = new Dictionary<string, TreeNode>();
        private void aboutFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.MdiParent = this.MdiParent;
            about.Show();
        }
        private void refreshtree()
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.IsReady)
                {
                    TreeNode treeNode;
                    treeNode = treeView1.Nodes.Add(d.Name);
                    drivePairs.Add(d.Name, treeNode);
                    
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
            {
            toolStripTextBox1.KeyDown += new KeyEventHandler(tb_KeyDown);
            refreshtree();
            }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(toolStripTextBox1.Text);
            }
        }
        public void Navigate(string dir)
        {   
            if (Directory.Exists(dir))
            {
                refreshItems(dir);
            } else
            {
                DialogResult x = MessageBox.Show("New Directory will be created.", "Files");
                Directory.CreateDirectory(dir);
                refreshItems(dir);
            }
            
            
        }
        private void refreshItems(string dir)
        {
            listView1.Items.Clear();
            foreach (string x in Directory.GetFiles(dir))
            {
                FileInfo file = new FileInfo(x);
                Icon iconForFile = SystemIcons.WinLogo;

                ListViewItem item = new ListViewItem(file.Name, 1);

                // Check to see if the image collection contains an image
                // for this extension, using the extension as a key
                string altExt = file.Extension;
                if (new string[]{ ".lnk", ".url", ".exe", ".ico" }.Contains(altExt))
                {
                    altExt = file.FullName;
                }
                if (!smallImageList.Images.ContainsKey(altExt))
                {
                    // If not, add the image to the image list.
                    iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
                    smallImageList.Images.Add(altExt, iconForFile);
                }
                if (!largeImageList.Images.ContainsKey(altExt))
                {
                    // If not, add the image to the image list.
                    iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
                    largeImageList.Images.Add(altExt, iconForFile);
                }
                item.ImageKey = altExt;
                listView1.Items.Add(item);
            }
            currentdir = dir;
        }
            
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (drivePairs[e.Node.FullPath].Nodes.Count == 0)
            {
                
                    foreach (string x in Directory.GetDirectories(Path.GetFullPath(e.Node.FullPath)))
                    {
                    
                        TreeNode tn = drivePairs[e.Node.FullPath].Nodes.Add(Path.GetFileName(Path.GetDirectoryName(x + "\\")));
                        drivePairs.Add(tn.FullPath, tn);
                    }
                
            }
            refreshItems(e.Node.FullPath);
            }
            catch
            {
                MessageBox.Show("Access Denied.");
            }
        }


        private string beginningto(string[] array, int index, string sep)
        {
            string x = "";
            for (int i = 0; i < index; i++)
            {
                if (i != 0)
                {
                    x = x + array[i] + sep;
                } else if (i == index){
                    x = x + array[i];
                } else
                {
                    x = array[i] + sep;
                }
            }
            x = x.TrimEnd(sep.ToCharArray());
            MessageBox.Show(x);
            return x;
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                foreach(ListViewItem item in listView1.SelectedItems)
                {
                    if (Fileopendict.ContainsKey(Path.GetExtension(item.Text)))
                    {
                        Fileopendict[Path.GetExtension(item.Text)].useFile(treeView1.SelectedNode.FullPath + "\\" + item.Text, this.MdiParent);
                    } else
                    {
                        MessageBox.Show("No Extension Found.");
                    }
                }
            }
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {

        }
        private void createTXTDoc()
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    }

