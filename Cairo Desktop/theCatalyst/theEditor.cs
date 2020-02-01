using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NeoGeniX.Forms;

namespace theCatalyst
{
    public partial class theEditor : DarkForm
    {
        string path = "";
        public theEditor()
        {
            InitializeComponent();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            path = "";
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Undo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            StreamReader streamReader = new StreamReader(openFileDialog1.OpenFile());
            textBox1.Text = streamReader.ReadToEnd();
            path = openFileDialog1.FileName;
            streamReader.Close();
        }
        public void openFile(string path)
        {
            StreamReader streamReader = new StreamReader(File.OpenRead(path));
            textBox1.Text = streamReader.ReadToEnd();
            path = openFileDialog1.FileName;
            streamReader.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (path == "")
            {
                saveAsToolStripMenuItem_Click(sender, e);
            } else
            {
                Stream stream = File.Open(path, FileMode.Open);
                StreamWriter streamWriter = new StreamWriter(stream);
                streamWriter.WriteLine(textBox1.Text);
                streamWriter.Close();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            StreamWriter streamWriter = new StreamWriter(saveFileDialog1.OpenFile());
            streamWriter.WriteLine(textBox1.Text);
            streamWriter.Close();
        }

        private void theEditor_Load(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox1 = new AboutBox1();
            aboutBox1.Show();
        }
    }
}
